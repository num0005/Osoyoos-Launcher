using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

internal class JSONShaderPathGen
{
    
    public static void GenerateShaderPathsJSON(string jsonPath, string shaderPath)
    {
        int lastslash = jsonPath.LastIndexOf('\\');
        string directory = jsonPath.Substring(0, lastslash);
        string fullShaderDirectory = shaderPath;

        string shortShaderDirectory = fullShaderDirectory.Split("tags\\").Last().Replace("\\", "\\\\");
        var jsonFiles = Directory.EnumerateFiles(directory, "*.*").Where(s => "json".Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));
        string line;
        foreach (string file in jsonFiles)
        {
            StreamReader sr = new(file);
            int materialsStartLine = 1;

            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains("material_properties"))
                {
                    break;
                }
                materialsStartLine++;
            }

            sr.Close();

            string[] lines = File.ReadLines(file).Skip(materialsStartLine).ToArray();

            int lineNum = materialsStartLine;
            string currentshadername = "";
            foreach (string jsonline in lines)
            {
                if (jsonline.Contains(": {"))
                {
                    currentshadername = jsonline.Split('"')[1].ToLower().Trim(new char[] { '%', '#', '?', '!', '@', '*', '$', '^', '-', '&', '=', '.', ';', ')', '>', '<', '|', '~', '(', '{', '}', '[', '\'', ']'});
                }
                if (jsonline.Contains("bungie_shader_path"))
                {
                    string newLineText = "\t\t\t\"bungie_shader_path\": \"" + shortShaderDirectory + "\\\\" + currentshadername + "\",";
                    jsonLineChanger(newLineText, file, lineNum);
                }
                lineNum++;
            }
        }
    }

    public static string FolderSelector(string jsonPath)
    {
        // Folder select dialog to allow user to specify the folder containing their shaders
        string fullShaderDirectory = "";
        string rootDirectory = jsonPath.Split(new string[] { "data" }, StringSplitOptions.None).Take(1).First() + @"tags";
        var openDialog = new FolderBrowserDialog();
        openDialog.Description = "Select shader folder";
        openDialog.RootFolder = Environment.SpecialFolder.Desktop;
        openDialog.SelectedPath = rootDirectory;
        if (openDialog.ShowDialog() == DialogResult.OK)
        {
            fullShaderDirectory = openDialog.SelectedPath;
        }

        return fullShaderDirectory;
    }

    static void jsonLineChanger(string newText, string fileName, int line_to_edit)
    {
        string[] arrLine = File.ReadAllLines(fileName);
        arrLine[line_to_edit] = newText;
        File.WriteAllLines(fileName, arrLine);
    }
}
