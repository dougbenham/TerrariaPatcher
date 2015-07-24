using System;
using System.IO;
using System.Windows.Forms;
using PluginLoader;

namespace TerrariaPatcher
{
    public partial class CopyPlugins : Form
    {
        private readonly string targetFolder;

        public CopyPlugins(string targetFolder)
        {
            this.targetFolder = targetFolder + @"\Plugins";

            InitializeComponent();

            clearExisting.Checked = bool.Parse(IniAPI.ReadIni("ActivePlugins", "ClearExisting", "true", 255, Main.ConfigPath, true));

            foreach (var filename in Directory.EnumerateFiles(@".\Plugins\", "*.cs"))
            {
                var name = Path.GetFileNameWithoutExtension(filename);
                checkedListBox.Items.Add(name);
                checkedListBox.SetItemChecked(checkedListBox.Items.Count - 1, bool.Parse(IniAPI.ReadIni("ActivePlugins", name, "true", 255, Main.ConfigPath, true)));
            }
        }

        private void copyButton_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(targetFolder) && clearExisting.Checked)
                Directory.Delete(targetFolder, true);

            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);

            CopyFolder(@".\Plugins\Shared", targetFolder + @"\Shared");

            foreach (string pluginName in checkedListBox.CheckedItems)
            {
                var ending = @"\" + pluginName + ".cs";
                File.Copy(@".\Plugins" + ending, targetFolder + ending, true);
            }

            this.Close();
        }

        private static void CopyFolder(string source, string destination)
        {
            foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, destination));

            foreach (string newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, destination), true);
        }

        private void checkedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            IniAPI.WriteIni("ActivePlugins", checkedListBox.Items[e.Index] as string, (e.NewValue == CheckState.Checked).ToString(), Main.ConfigPath);
        }

        private void CopyPlugins_Shown(object sender, EventArgs e)
        {
            copyButton.Focus();
        }

        private void clearExisting_CheckedChanged(object sender, EventArgs e)
        {
            copyButton.Text = clearExisting.Checked ? "Sync" : "Copy";
            IniAPI.WriteIni("ActivePlugins", "ClearExisting", (clearExisting.Checked).ToString(), Main.ConfigPath);
        }
    }
}
