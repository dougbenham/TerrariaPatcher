extern alias PluginLoaderXNA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PluginLoaderXNA::PluginLoader;

namespace TerrariaPatcher
{
    public partial class CopyPlugins : Form
    {
        private readonly string sourceFolder;
        private string sourceSharedFolder => Path.Combine(sourceFolder, "Shared");
        private readonly string targetFolder;
        private string targetSharedFolder => Path.Combine(targetFolder, "Shared");

        public CopyPlugins(string targetFolder)
        {
            this.sourceFolder = @".\Plugins";
            this.targetFolder = targetFolder + @"\Plugins";

            InitializeComponent();

            clearExisting.Checked = bool.Parse(IniAPI.ReadIni("ActivePlugins", "ClearExisting", "true", 255, Main.ConfigPath, true));

            foreach (var folder in Directory.EnumerateDirectories(sourceFolder).Where(s => s != sourceSharedFolder))
            {
                var name = Path.GetFileName(folder);
                checkedListBox.Items.Add(name);
                checkedListBox.SetItemChecked(checkedListBox.Items.Count - 1, bool.Parse(IniAPI.ReadIni("ActivePlugins", name, "true", 255, Main.ConfigPath, true)));
            }
            foreach (var filename in Directory.EnumerateFiles(sourceFolder, "*.cs"))
            {
                var name = Path.GetFileNameWithoutExtension(filename);
                checkedListBox.Items.Add(name);
                checkedListBox.SetItemChecked(checkedListBox.Items.Count - 1, bool.Parse(IniAPI.ReadIni("ActivePlugins", name, "true", 255, Main.ConfigPath, true)));
            }
        }

        private void copyButton_Click(object sender, EventArgs e)
        {
            var toCopy = new List<string>();
            foreach (string pluginName in checkedListBox.CheckedItems)
            {
                if (Directory.Exists(Path.Combine(sourceFolder, pluginName)))
                    toCopy.Add(pluginName + '\\');
                else
                    toCopy.Add(pluginName + ".cs");
            }

            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);

            if (clearExisting.Checked)
            {
                foreach (var folder in Directory.EnumerateDirectories(targetFolder).Where(s => s != targetSharedFolder))
                {
                    var name = Path.GetFileName(folder);
                    if (toCopy.Contains(name + '\\')) continue;

                    if (MessageBox.Show("Delete " + folder + "?", Program.AssemblyName, MessageBoxButtons.YesNo) == DialogResult.Yes)
                        Directory.Delete(folder, true);
                }
                foreach (var file in Directory.EnumerateFiles(targetFolder, "*.cs"))
                {
                    var name = Path.GetFileName(file);
                    if (toCopy.Contains(name)) continue;

                    if (MessageBox.Show("Delete " + file + "?", Program.AssemblyName, MessageBoxButtons.YesNo) == DialogResult.Yes)
                        File.Delete(file);
                }
            }

            CopyFolder(sourceSharedFolder, targetSharedFolder);

            foreach (string pluginName in toCopy)
            {
                var sourcePath = Path.Combine(sourceFolder, pluginName);
                var destinationPath = Path.Combine(targetFolder, pluginName);
                if (Directory.Exists(sourcePath))
                    CopyFolder(sourcePath, destinationPath);
                else
                    File.Copy(sourcePath, destinationPath, true);
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
