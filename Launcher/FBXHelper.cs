using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolkitLauncher.Properties;
using ToolkitLauncher.ToolkitInterface;

namespace ToolkitLauncher
{
    internal static class FBXHelper
    {
        public static async Task CreateJMAFromFBX(IToolkitFBX2Jointed toolkit, string fbxFileName, string outputFileName)
        {
            int? startFrame = null;
            int? endFrame = null;
            AnimLengthPrompt AnimDialog = new AnimLengthPrompt();
            bool? result = AnimDialog.ShowDialog();
            if (result == true)
            {
                int parsed_value;
                if (Int32.TryParse(AnimDialog.start_index.Text, out parsed_value))
                    startFrame = parsed_value;
                if (Int32.TryParse(AnimDialog.last_index.Text, out parsed_value))
                    endFrame = parsed_value;
            }
            await toolkit.JMAFromFBX(fbxFileName, outputFileName, startFrame ?? 0, endFrame);
        }

        public static (string ext, string fbxFileName, string outputFileName)? PromptForFBXPaths(ToolkitBase toolkit, string title_string, string filter_string)
        {
            string outputFileName = "";

            var openDialog = new System.Windows.Forms.OpenFileDialog();
            openDialog.Title = "Select FBX (Filmbox)";
            openDialog.Filter = "FBX (Filmbox)|*.fbx";
            openDialog.InitialDirectory = Settings.Default.last_fbx_path;
            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                // check if we need to update the initial directory
                string? fbxFileDir = Path.GetDirectoryName(openDialog.FileName);
                if (fbxFileDir != Settings.Default.last_fbx_path)
                {
                    Settings.Default.last_fbx_path = fbxFileDir;
                    Settings.Default.Save();
                }

                var saveDialog = new System.Windows.Forms.SaveFileDialog();

                saveDialog.OverwritePrompt = true;
                saveDialog.Title = title_string;
                saveDialog.Filter = filter_string;

                saveDialog.InitialDirectory = toolkit.GetDataDirectory();
                if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    outputFileName = saveDialog.FileName;
                    string ext = Path.GetExtension(outputFileName).ToLowerInvariant();
                    return (ext, openDialog.FileName, outputFileName);
                }
            }
            return null;
        }

    }
}
