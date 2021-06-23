using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Windows.Forms;
using ToolkitLauncher.ToolkitInterface;

public class FilePicker
{
#nullable enable
	public class Options
    {
		public enum PathRoot
        {
			FileSystem,
			Tag,
			Data,
			Tag_Data
        }
		static public Options FileSelect(string title, string filter, PathRoot pathRoot, bool parent = false, bool strip_extension = true)
        {
			Options opt = new Options();
			opt.title = title;
			opt.filter = filter;
			opt.pathRoot = pathRoot;
			opt.parent = parent;
			opt.strip_extension = strip_extension;

			return opt;
		}

		static public Options FolderSelect(string title, PathRoot pathRoot, bool parent = false, bool strip_extension = true)
		{
			Options opt = new Options();
			opt.title = title;
			opt.filter = null;
			opt.pathRoot = pathRoot;
			opt.parent = parent;
			opt.strip_extension = strip_extension;

			return opt;
		}

		internal bool IsFolderSelect() { return filter is null; }

		internal string? title;
		internal string? filter;
		internal PathRoot pathRoot;
		internal bool parent;
		internal bool strip_extension;

	}
#nullable restore

	public FilePicker(System.Windows.Controls.TextBox box, ToolkitBase toolkit, Options options, string InitialDirectory)
	{
		if (toolkit is not null && !Path.IsPathRooted(InitialDirectory))
			InitialDirectory = Path.Join(toolkit.BaseDirectory, InitialDirectory);
		this.textBox = box;
		this.toolkitInterface = toolkit;
		this.options = options;
		if (options.IsFolderSelect()) {
			folderDialog = new CommonOpenFileDialog();
			folderDialog.Title = options.title;
			folderDialog.IsFolderPicker = true;

			folderDialog.AllowNonFileSystemItems = false;
			folderDialog.EnsureFileExists = true;
			folderDialog.EnsurePathExists = true;
			folderDialog.Multiselect = false;
			folderDialog.ShowPlacesList = true;
			folderDialog.InitialDirectory = InitialDirectory;
		} else {
			fileDialog = new OpenFileDialog();
			fileDialog.Title = options.title;
			fileDialog.Filter = options.filter;
			fileDialog.InitialDirectory = InitialDirectory;
		}
	}
	public bool Prompt()
    {
		if (folderDialog is null && fileDialog is null)
			throw new InvalidOperationException("No valid dialog");
		if (folderDialog is not null && folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
        {
			return ProcessInput(folderDialog.FileName);
		}
		if (fileDialog is not null && fileDialog.ShowDialog() == DialogResult.OK)
		{
			return ProcessInput(fileDialog.FileName);
		}
		return false;
    }

#nullable enable

	bool ProcessInput(string path)
    {
		string? local_path = ConvertFSPathToLocalPath(path, options.pathRoot);
		if (local_path is null)
			return false;
		if (options.strip_extension)
			local_path = local_path.Substring(0, local_path.Length - Path.GetExtension(local_path).Length);
		if (options.parent)
			local_path = local_path.Substring(0, local_path.Length - Path.GetFileName(local_path).Length);
		textBox.Text = local_path;
		return true;
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="path"></param>
	/// <param name="root"></param>
	/// <returns></returns>
	string? ConvertFSPathToLocalPath(string path, Options.PathRoot root)
	{
		if (root == Options.PathRoot.FileSystem)
			return Path.GetFullPath(path);
		string base_path;
		switch (root)
        {
			case Options.PathRoot.Data:
				base_path = toolkitInterface.GetDataDirectory();
				break;
			case Options.PathRoot.Tag:
				base_path = toolkitInterface.GetTagDirectory();
				break;
			case Options.PathRoot.Tag_Data:
				base_path = toolkitInterface.GetDataDirectory();
				if (path.StartsWith(toolkitInterface.GetTagDirectory()))
					base_path = toolkitInterface.GetTagDirectory();
				break;
			default:
				throw new InvalidOperationException();
		}
		if (!path.StartsWith(base_path))
			return null;
		return Path.GetRelativePath(base_path, path);
	}

#nullable restore

	private OpenFileDialog fileDialog;
	private CommonOpenFileDialog folderDialog;
	private System.Windows.Controls.TextBox textBox;
	private ToolkitBase toolkitInterface;
	private Options options;
}
