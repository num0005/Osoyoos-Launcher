using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;

internal class AutoShadersGen2
{
    public static bool GenerateEmptyShaders(string BaseDirectory, string path)
    {
        // Variables
        string full_jms_path = "";
        int counter = 0;

        //Grabbing full path from drive letter to render folder
        string jmsPath = (BaseDirectory + @"\data\" + path + @"\render").Replace("\\\\", "\\");

        // Get all files in render foler
        string[] files = Array.Empty<string>();
        try
        {
            files = Directory.GetFiles(jmsPath);
        }
        catch (DirectoryNotFoundException)
        {
            MessageBox.Show("Unable to find JMS filepath!\nThis usually happens if your filepath contains invalid characters.\nAborting model import and shader generation...", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        string destinationShadersFolder = BaseDirectory + @"\tags\" + path + @"\shaders";

        // Checking if shaders already exist, if so don't re-gen them
        // There must be a better way of doing this that doesn't involve try-catch, but I'm tired...
        try
        {
            if (!(Directory.GetFiles(destinationShadersFolder) == Array.Empty<string>()))
            {
                Debug.WriteLine("Shaders already exist!");
                if (MessageBox.Show("Shaders for this model already exist!\nWould you like to generate any missing shaders?", "Shader Gen. Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    shaderGen(files, full_jms_path, counter, destinationShadersFolder, BaseDirectory);
                }
            }
            else
            {
                return true;
            }
        }
        catch (DirectoryNotFoundException)
        {
            Debug.WriteLine("No folders exist, proceeding with shader gen");
            shaderGen(files, full_jms_path, counter, destinationShadersFolder, BaseDirectory);
        }

        static void shaderGen(string[] files, string full_jms_path, int counter, string destinationShadersFolder, string BaseDirectory)
        {
            string line;
            List<string> shaders = new();
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

                    // Open shader_collections.txt
                    List<string> collections = new();
                    try
                    {
                        sr = new(BaseDirectory + @"\tags\scenarios\shaders\shader_collections.shader_collections");
                    }
                    catch (DirectoryNotFoundException)
                    {
                        MessageBox.Show("Could not find shader_collections file!\nMake sure you have shader_collections.shader_collections in\n\"H2EK/tags/scenarios/shaders\"", "Shader Gen. Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    // Grab every shader collection prefix
                    while ((line = sr.ReadLine()) != null)
                    {
                        if ((line.Contains("scenarios") || line.Contains("objects") || line.Contains("test")) && !line.Contains('='))
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
                    // Typically the most "complex" materials come in the format: prefix name extra1 extra2 extra3...
                    // So if a prefix exists, check that it is a valid collection, if so ignore it as shader will be grabbed from collection,
                    // but if it isn't, it should be treated as part of the full shader name
                    string[] extras = { "lm:", "lp:", "hl:", "ds:" };
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
                                string shaderPrefixAndName = "";
                                // Check if any section is an "extra" material property, if so don't include it
                                foreach (string part in shaderNameSections)
                                {
                                    if (!extras.Any(part.Contains))
                                    {
                                        shaderPrefixAndName += part + ' ';
                                    }
                                }
                                shaderNameStripped = Regex.Replace(shaderPrefixAndName, "[^0-9a-zA-Z_. ]", string.Empty).Trim();
                                shaders.Add(shaderNameStripped);
                            }
                            // Otherwise shader is part of an existing collection, so no need to create a new tag for it
                        }
                        // Skip to next shader name
                        currentLine += 4;
                    }
                }
            }

            // Remove duplicate shader names
            shaders = shaders.Distinct().ToList();

            // Create directories               

            Directory.CreateDirectory(destinationShadersFolder);

            string defaultShaderLocation = BaseDirectory + @"\tags\shaders\default.shader";
            if (!File.Exists(defaultShaderLocation))
            {
                File.WriteAllBytes(defaultShaderLocation, ToolkitLauncher.Utility.Resources.defaultH2);
            }

            // Write each shader
            foreach (string shader in shaders)
            {
                string shaderName = shader + ".shader";
                if(!File.Exists(Path.Combine(destinationShadersFolder, shaderName)))
                {
                    try
                    {
                        File.Copy(defaultShaderLocation, Path.Combine(destinationShadersFolder, shaderName));
                    }
                    catch (FileNotFoundException)
                    {
                        // Will probably only occur if user somehow deletes default.shader after the check for its existence occurs,
                        // but before shaders are generated
                        if (MessageBox.Show("Unable to find shader to copy from!\nThis really shouldn't have happened.\nPress OK to try again, or Cancel to skip shader generation.", "Shader Gen. Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                        {
                            File.WriteAllBytes(defaultShaderLocation, ToolkitLauncher.Utility.Resources.defaultH2);
                            counter = 0;
                            shaderGen(files, full_jms_path, counter, destinationShadersFolder, BaseDirectory);
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
        return true;
    }
}