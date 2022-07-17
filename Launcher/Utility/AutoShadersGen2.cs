using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;

class AutoShadersGen2
{
    public static void GenerateEmptyShaders(string BaseDirectory, string path)
    {
        // Variables
        string full_jms_path = "";
        List<string> shaders = new();
        int counter = 0;

        //Grabbing full path from drive letter to render folder
        string jmsPath = (BaseDirectory + @"\data\" + path + @"\render").Replace("\\\\", "\\");

        // Get all files in render foler
        string[] files = Directory.GetFiles(jmsPath);
        string destinationShadersFolder = BaseDirectory + @"\tags\" + path + @"\shaders";

        // Checking if shaders already exist, if so don't re-gen them
        // There must be a better way of doing this that doesn't involve try-catch, but I'm tired...
        try
        {
            if (Directory.GetFiles(destinationShadersFolder) == Array.Empty<string>())
            {
                Debug.WriteLine("Folder exists but contains no shaders, proceeding");
                shaderGen(files, full_jms_path, counter, shaders, destinationShadersFolder, BaseDirectory);
            }
            else
            {
                Debug.WriteLine("Shaders already exist!");
                MessageBox.Show("Shaders for this model already exist, skipping shader generation!", "Shader Gen. Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (DirectoryNotFoundException)
        {
            Debug.WriteLine("No folders exist, proceeding with shader gen");
            shaderGen(files, full_jms_path, counter, shaders, destinationShadersFolder, BaseDirectory);
        }

        static void shaderGen(string[] files, string full_jms_path, int counter, List<string> shaders, string destinationShadersFolder, string BaseDirectory)
        {
            string line;
            // Find name of jms file
            foreach (string file in files)
            {
                counter = 0;

                if (file[^4..] is ".JMS" or ".jms")
                {
                    full_jms_path = file;

                    StreamReader sr = new(full_jms_path);

                    // Find Materials definition header in JMS file
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Contains(";### MATERIALS ###"))
                        {
                            break;
                        }
                        counter++;
                    }

                    //Grab number of materials from file
                    int numMats = int.Parse(File.ReadLines(full_jms_path).Skip(counter + 1).Take(1).First());
                    // Line number of first shader name
                    int currentLine = counter + 7;

                    // Take each material name, strip symbols, add to list
                    // Typically the most "complex" H3 materials come in the format: prefix name extra1 extra2 extra3...
                    // So if a prefix exists, grab second word, else grab first word, ignore any suffixes
                    // TODO: Will break in the case of spaces in the actual material "name", need to think of a workaround
                    string shaderNameStripped;
                    for (int i = 0; i < numMats; i++)
                    {
                        string[] shaderNameSections = File.ReadLines(full_jms_path).Skip(currentLine - 1).Take(1).First().Split(' ');
                        if (shaderNameSections.Length < 2)
                        {
                            shaderNameStripped = Regex.Replace(shaderNameSections[0], "[^0-9a-zA-Z_.]", string.Empty);
                        }
                        else
                        {
                            shaderNameStripped = Regex.Replace(shaderNameSections[1], "[^0-9a-zA-Z_.]", string.Empty);
                        }

                        shaders.Add(shaderNameStripped);
                        currentLine += 4;
                    }
                }
            }

            // Remove duplicate shader names
            shaders = shaders.Distinct().ToList();

            // Create directories               

            Directory.CreateDirectory(destinationShadersFolder);

            string defaultShaderLocation = BaseDirectory + @"\tags\shaders\default.shader";
            if(!File.Exists(defaultShaderLocation))
            {
                File.WriteAllBytes(defaultShaderLocation, ToolkitLauncher.Utility.Resources.defaultH2);
            }

            foreach (string shader in shaders)
            {
                string shaderName = shader + ".shader";
                try
                {
                    File.Copy(defaultShaderLocation, Path.Combine(destinationShadersFolder, shaderName));
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show("Could not find default shader!\nShaders will not be generated - if this\nis your first time using this function,\ncreate a blank shader tag with Guerilla:\n\"H2EK/tags/shaders/default.shader\"", "Shader Gen. Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                }
            }
        }
    }
}