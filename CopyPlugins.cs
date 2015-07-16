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
            this.targetFolder = targetFolder;

            InitializeComponent();
            
            foreach (var filename in Directory.EnumerateFiles(@".\Plugins\", "*.cs"))
            {
                var name = Path.GetFileNameWithoutExtension(filename);
                checkedListBox.Items.Add(name);
                checkedListBox.SetItemChecked(checkedListBox.Items.Count - 1, bool.Parse(IniAPI.ReadIni("ActivePlugins", name, "true", 255, Main.ConfigPath, true)));
            }
        }

        private void copyButton_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(targetFolder + "\\Plugins"))
                Directory.CreateDirectory(targetFolder + "\\Plugins");

            foreach (string pluginName in checkedListBox.CheckedItems)
            {
                var ending = @"\Plugins\" + pluginName + ".cs";
                File.Copy("." + ending, targetFolder + ending, true);
            }

            this.Close();
        }

        private void checkedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            IniAPI.WriteIni("ActivePlugins", checkedListBox.Items[e.Index] as string, (e.NewValue == CheckState.Checked).ToString(), Main.ConfigPath);
        }

        private void CopyPlugins_Shown(object sender, EventArgs e)
        {
            copyButton.Focus();
        }
    }
}
