using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;

internal class AutoShadersGen3
{
    public static bool GenerateEmptyShaders(string BaseDirectory, string path, string gameType)
    {
        // Variables
        string full_jms_path = "";
        int counter = 0;

        //Grabbing full path from drive letter to render folder
        string jmsPath = (BaseDirectory + @"\data\" + path + @"\render").Replace("\\\\", "\\");

        // Get all files in render folder
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
        try
        {
            if (!(Directory.GetFiles(destinationShadersFolder) == Array.Empty<string>()))
            {
                Debug.WriteLine("Shaders already exist!");
                if (MessageBox.Show("Shaders for this model already exist!\nWould you like to generate any missing shaders?", "Shader Gen. Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    string[] shaders = JMSMaterialReader.ReadAllMaterials(files, counter, full_jms_path, BaseDirectory, gameType);
                    shaderGen(shaders, counter, full_jms_path, destinationShadersFolder, BaseDirectory, gameType);
                }
                else
                {
                    return true;
                }
            }
            else
            {
                string[] shaders = JMSMaterialReader.ReadAllMaterials(files, counter, full_jms_path, BaseDirectory, gameType);
                shaderGen(shaders, counter, full_jms_path, destinationShadersFolder, BaseDirectory, gameType);
            }
        }
        catch (DirectoryNotFoundException)
        {
            Debug.WriteLine("No folders exist, proceeding with shader gen");
            string[] shaders = JMSMaterialReader.ReadAllMaterials(files, counter, full_jms_path, BaseDirectory, gameType);
            shaderGen(shaders, counter, full_jms_path, destinationShadersFolder, BaseDirectory, gameType);
        }

        static void shaderGen(string[] shaders, int counter, string full_jms_path, string destinationShadersFolder, string BaseDirectory, string gameType)
        {
            // Create directories               
            Directory.CreateDirectory(destinationShadersFolder);

            // Make sure default.shader exists, if not, create it
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
                if (shaderName == ".shader")
                {
                    MessageBox.Show("Detected an invalid (possibly blank) shader name!\nThis shader will not be generated.\nThis won't work well in-game.", "Shader Gen. Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    try { File.Copy(defaultShaderLocation, Path.Combine(destinationShadersFolder, "im.too.dumb.to.name.my.shader")); } catch { Debug.WriteLine("ah well"); };
                    continue;
                }
                if (!File.Exists(Path.Combine(destinationShadersFolder, shaderName)))
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
                            if (gameType == "H3")
                            {
                                File.WriteAllBytes(defaultShaderLocation, ToolkitLauncher.Utility.Resources.defaultH3);
                            }
                            else
                            {
                                File.WriteAllBytes(defaultShaderLocation, ToolkitLauncher.Utility.Resources.defaultODST);
                            }
                            counter = 0;
                            shaderGen(shaders, counter, full_jms_path, destinationShadersFolder, BaseDirectory, gameType);
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
        // Default fall-through
        return true;
    }
}

// JMS Material Parser
// TODO (PepperMan) - Make this less hardcoded, changes to line positions in JMS format will break this
internal class JMSMaterialReader
{
    public static string[] ReadAllMaterials(string[] files, int counter, string full_jms_path, string BaseDirectory, string gameType)
    {
        string line;
        List<string> shaders = new();
        // Find name of each jms file, then grab every material name from it
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
                if (gameType == "H3" || gameType == "H3ODST")
                {
                    try
                    {
                        sr = new(BaseDirectory + @"\tags\levels\shader_collections.txt");
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Could not find shader_collections.txt!\nMake sure you have shader_collections.txt in\n\"H3EK/tags/levels\"", "Shader Gen. Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    try
                    {
                        sr = new(BaseDirectory + @"\tags\scenarios\shaders\shader_collections.shader_collections");
                    }
                    catch (DirectoryNotFoundException)
                    {
                        MessageBox.Show("Could not find shader_collections file!\nMake sure you have shader_collections.shader_collections in\n\"H2EK/tags/scenarios/shaders\"", "Shader Gen. Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }


                // Grab every shader collection prefix
                if (gameType == "H3" || gameType == "H3ODST")
                {
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
                }
                else
                {
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
                }

                // Take each material name, strip symbols, add to list
                // Typically the most "complex" materials come in the format: prefix name extra1 extra2 extra3...
                // So if a prefix exists, check that it is a valid collection, if so ignore it as shader will be grabbed from collection,
                // but if it isn't, it should be treated as part of the full shader name
                string[] extras = { "lm:", "lp:", "hl:", "ds:", "pf:", "lt:", "to:", "at:", "ro:" };
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

        return shaders.ToArray();
    }
}