using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;

internal class AutoShadersH2
{
    public static bool CreateEmptyShadersH2(string BaseDirectory, string path, string gameType)
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
            string defaultShaderLocation = "";

            // Create directories               
            Directory.CreateDirectory(destinationShadersFolder);

            // Make sure default.shader exists, if not, create it
            defaultShaderLocation = gameType == "H2"
                ? BaseDirectory + @"\tags\shaders\default.shader"
                : BaseDirectory + @"\tags\levels\shared\shaders\simple\default.shader";

            if (!File.Exists(defaultShaderLocation))
            {
                File.WriteAllBytes(defaultShaderLocation, ToolkitLauncher.Utility.Resources.defaultH2);
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
                            else if (gameType == "H3ODST")
                            {
                                File.WriteAllBytes(defaultShaderLocation, ToolkitLauncher.Utility.Resources.defaultODST);
                            }
                            else
                            {
                                File.WriteAllBytes(defaultShaderLocation, ToolkitLauncher.Utility.Resources.defaultH2);
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