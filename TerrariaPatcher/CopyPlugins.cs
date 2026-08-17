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

        private readonly Dictionary<string, string> descriptions;
        private readonly ToolTip descriptionTip = new ToolTip { AutoPopDelay = 30000, InitialDelay = 400, ReshowDelay = 100 };
        private int tipIndex = -1;

        public CopyPlugins(string targetFolder)
        {
            this.sourceFolder = @".\Plugins";
            this.targetFolder = targetFolder + @"\Plugins";

            InitializeComponent();

            clearExisting.Checked = Main.ReadBool("ActivePlugins", "ClearExisting", true, writeIt: true);

            descriptions = PluginDescriptions.ReadAll(sourceFolder, sourceSharedFolder);

            foreach (var folder in Directory.EnumerateDirectories(sourceFolder).Where(s => s != sourceSharedFolder))
            {
                var name = Path.GetFileName(folder);
                checkedListBox.Items.Add(name);
                checkedListBox.SetItemChecked(checkedListBox.Items.Count - 1, Main.ReadBool("ActivePlugins", name, true, writeIt: true));
            }
            foreach (var filename in Directory.EnumerateFiles(sourceFolder, "*.cs"))
            {
                var name = Path.GetFileNameWithoutExtension(filename);
                checkedListBox.Items.Add(name);
                checkedListBox.SetItemChecked(checkedListBox.Items.Count - 1, Main.ReadBool("ActivePlugins", name, true, writeIt: true));
            }

            checkedListBox.MouseMove += checkedListBox_MouseMove;

            if (checkedListBox.Items.Count > 0)
                checkedListBox.SelectedIndex = 0;
        }

        private string DescriptionOf(string pluginName)
        {
	        return descriptions.TryGetValue(pluginName, out var description) && !string.IsNullOrEmpty(description)
                ? description
                : null;
        }

        private void checkedListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var pluginName = checkedListBox.SelectedItem as string;

            descriptionBox.Text = pluginName == null
                ? ""
                : DescriptionOf(pluginName) ?? "No description.";
        }

        private void checkedListBox_MouseMove(object sender, MouseEventArgs e)
        {
            var index = checkedListBox.IndexFromPoint(e.Location);
            if (index == tipIndex) return;

            tipIndex = index;

            var pluginName = index >= 0 ? checkedListBox.Items[index] as string : null;
            var description = pluginName == null ? null : DescriptionOf(pluginName);

            if (description == null)
                descriptionTip.Hide(checkedListBox);
            else
                descriptionTip.SetToolTip(checkedListBox, description);
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

            MigrateSettings();

            this.Close();
        }

        /// <summary>
        /// Renames settings in the game's Plugins.ini that the plugins no longer read under their old names.
        /// </summary>
        private void MigrateSettings()
        {
            var iniPath = Path.Combine(Path.GetDirectoryName(targetFolder) ?? string.Empty, "Plugins.ini");

            try
            {
                var moved = PluginSettingsMigration.Migrate(iniPath);
                if (moved == 0) return;

                MessageBox.Show(
                    "Moved " + moved + " existing setting" + (moved == 1 ? "" : "s") + " in " + iniPath +
                    " to the names the plugins use now.", Program.AssemblyName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not update " + iniPath + ", so any settings under the old names will be ignored." +
                                Environment.NewLine + Environment.NewLine + ex.Message,
                    Program.AssemblyName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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
            // Focus the list so the arrow keys read through the descriptions straight away.
            checkedListBox.Focus();
        }

        private void clearExisting_CheckedChanged(object sender, EventArgs e)
        {
            copyButton.Text = clearExisting.Checked ? "Sync" : "Copy";
            IniAPI.WriteIni("ActivePlugins", "ClearExisting", (clearExisting.Checked).ToString(), Main.ConfigPath);
        }
    }
}
