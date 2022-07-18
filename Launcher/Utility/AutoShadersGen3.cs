using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;

class AutoShadersGen3
{
    public static void GenerateEmptyShaders(string BaseDirectory, string path, string gameType)
    {
        // Variables
        string full_jms_path = "";
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
                shaderGen(files, full_jms_path, counter, destinationShadersFolder, BaseDirectory, gameType);
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
            shaderGen(files, full_jms_path, counter, destinationShadersFolder, BaseDirectory, gameType);
        }

        static void shaderGen(string[] files, string full_jms_path, int counter, string destinationShadersFolder, string BaseDirectory, string gameType)
        { 
            string line;
            List<string> shaders = new();
            // Find name of jms file
            foreach (string file in files)
            {
                if (file[^4..] is ".JMS" or ".jms")
                {
                    full_jms_path = file;
                }
            }

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

            // Open shader_collections.txt
            List<string> collections = new();
            try
            {
                sr = new(BaseDirectory + @"\tags\levels\shader_collections.txt");
            }
            catch (Exception)
            {
                MessageBox.Show("Could not find shader_collections.txt!\nMake sure you shader_collections.txt in\n\"H3EK/tags/levels\"", "Shader Gen. Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Grab every shader collection prefix
            while ((line = sr.ReadLine()) != null)
            {
                if ((line.Contains("levels") || line.Contains("scenarios") || line.Contains("objects")) && !line.Contains("shader_collections.txt"))
                {
                    if (line.Contains('\t'))
                    {
                        collections.Add(line.Substring(0, line.IndexOf('\t')));
                    }
                    else
                    {
                        collections.Add(line.Substring(0, line.IndexOf(' ')));
                    }       
                }
            }
            
            // Take each material name, strip symbols, add to list
            // Typically the most "complex" H3 materials come in the format: prefix name extra1 extra2 extra3...
            // So if a prefix exists, grab second word, else grab first word, ignore any suffixes
            // TODO: Will break in the case of spaces in the actual material "name", need to think of a workaround
            // Probably best to check shader collections txt and then only strip those prefixes
            string shaderNameStripped;
            for (int i = 0; i < numMats; i++)
            {
                string[] shaderNameSections = File.ReadLines(full_jms_path).Skip(currentLine - 1).Take(1).First().Split(' ');
                if (shaderNameSections.Length < 2)
                {
                    shaderNameStripped = Regex.Replace(shaderNameSections[0], "[^0-9a-zA-Z_.]", string.Empty);
                    shaders.Add(shaderNameStripped);
                }
                else // Shader name has spaces in it
                {
                    // Check if section before first space is a valid shader collection prefix
                    if (!collections.Contains(shaderNameSections[0]))
                    {
                        // Shader is not in a collection, so probably just has a space in the name
                        shaderNameStripped = Regex.Replace(shaderNameSections[0] + ' ' + shaderNameSections[1], "[^0-9a-zA-Z_.]", string.Empty);
                        shaders.Add(shaderNameStripped);
                    }
                    // Otherwise shader is part of an existing collection, so no need to create a new tag for it
                }
                // Skip to next shader name
                currentLine += 4;
            }

            // Remove duplicate shader names
            shaders = shaders.Distinct().ToList();

            // Create directories               
            Directory.CreateDirectory(destinationShadersFolder);

            string defaultShaderLocation = BaseDirectory + @"\tags\levels\shared\shaders\simple\default.shader";
            if (!File.Exists(defaultShaderLocation))
            {
                if (gameType == "H3")
                {
                    File.WriteAllBytes(defaultShaderLocation, ToolkitLauncher.Utility.Resources.defaultH3);
                }
                else
                {
                    File.WriteAllBytes(defaultShaderLocation, ToolkitLauncher.Utility.Resources.defaultODST);
                }
                
            }

            // Write each shader
            foreach (string shader in shaders)
            {
                string shaderName = shader + ".shader";
                try
                {
                    File.Copy(defaultShaderLocation, Path.Combine(destinationShadersFolder, shaderName));
                }
                catch (FileNotFoundException)
                {
                    // Will probably only occur if user somehow deletes default.shader after the check for its existence occurs,
                    // but before shaders are generated
                    if(MessageBox.Show("Unable to find shader to copy from!\nThis really shouldn't have happened.\nPress OK to try again, or Cancel to skip shader generation.", "Shader Gen. Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                    {
                        if (gameType == "H3")
                        {
                            File.WriteAllBytes(defaultShaderLocation, ToolkitLauncher.Utility.Resources.defaultH3);
                        }
                        else
                        {
                            File.WriteAllBytes(defaultShaderLocation, ToolkitLauncher.Utility.Resources.defaultODST);
                        }
                        counter = 0;
                        shaderGen(files, full_jms_path, counter, destinationShadersFolder, BaseDirectory, gameType);
                        break;
                    }
                    else
                    {
                        break;
                    }
                    
                }
            }
        }
    }       
}