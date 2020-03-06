extern alias PluginLoaderXNA;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using PluginLoaderXNA::PluginLoader;

namespace TerrariaPatcher
{
    public partial class Main : Form
    {
        private static string changelogURL = "https://raw.githubusercontent.com/dougbenham/TerrariaPatcher/master/changelog.txt";
#if PUBLIC
        private static string updateURL = "https://github.com/dougbenham/TerrariaPatcher/raw/master/TerrariaPatcher.public.zip";
#else
        private static string updateURL = "https://github.com/dougbenham/TerrariaPatcher/raw/master/TerrariaPatcher.zip";
#endif
        private static List<Buff> buffs;
        public static string ConfigPath = Environment.CurrentDirectory + "\\TerrariaPatcher.ini";
        private bool loading = false;

        public Main()
        {
            InitializeComponent();

            var asm = Assembly.GetExecutingAssembly();
            var asmName = asm.GetName();
            this.Icon = Icon.ExtractAssociatedIcon(asm.Location);
            this.Text = asmName.Name + " v" + asmName.Version;

            new Thread(() =>
            {
                try
                {
                    // Delete leftover update files
                    foreach (var tmp in Directory.GetFiles(Environment.CurrentDirectory, "*.tmp"))
                        File.Delete(tmp);

                    var client = new WebClient();
                    client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                    var str = client.DownloadString(changelogURL + "?random=" + new Random().Next());
                    var version = str.Substring(1, str.IndexOf(':') - 1);
                    if (version != asmName.Version.ToString())
                    {
                        if (MessageBox.Show(version + " is available. Would you like to automatically update?", Program.AssemblyName, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        {
                            // Download the update
                            var zip = "update.tmp";
                            client.DownloadFile(updateURL, zip);

                            // Rename the currently executing TerrariaPatcher.exe / PluginLoader.XNA.dll / PluginLoader.FNA.dll / Mono.Cecil.dll so that we can update
                            var location = Path.Combine(Environment.CurrentDirectory, "TerrariaPatcher.exe");
                            File.Move(location, location.Replace("exe", "tmp"));
                            location = Path.Combine(Environment.CurrentDirectory, "PluginLoader.XNA.dll");
                            File.Move(location, location.Replace("dll", "tmp"));
                            location = Path.Combine(Environment.CurrentDirectory, "PluginLoader.FNA.dll");
                            File.Move(location, location.Replace("dll", "tmp"));
                            location = Path.Combine(Environment.CurrentDirectory, "Mono.Cecil.dll");
                            File.Move(location, location.Replace("dll", "tmp"));

                            // Extract the update
                            using (var archive = ZipFile.OpenRead(zip))
                            {
                                foreach (ZipArchiveEntry file in archive.Entries)
                                {
                                    string completeFileName = Path.Combine(Environment.CurrentDirectory, file.FullName);
                                    string directory = Path.GetDirectoryName(completeFileName);

                                    if (!Directory.Exists(directory))
                                        Directory.CreateDirectory(directory);

                                    if (file.Name != "")
                                        file.ExtractToFile(completeFileName, true);
                                }
                            }

                            // Restart
                            Application.Restart();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to check for updates." + Environment.NewLine + Environment.NewLine + ex, Program.AssemblyName);
                }
            }) {IsBackground = true}.Start();

            terrariaPath.Text = "";

            if (string.IsNullOrEmpty(terrariaPath.Text))
            {
                using (var reg = Registry.LocalMachine.OpenSubKey(@"Software\Re-Logic\Terraria"))
                {
                    if (reg != null) terrariaPath.Text = reg.GetValue("Exe_Path", "") as string;
                }
            }
            if (string.IsNullOrEmpty(terrariaPath.Text))
            {
                using (var reg = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Re-Logic\Terraria"))
                {
                    if (reg != null) terrariaPath.Text = reg.GetValue("Exe_Path", "") as string;
                }
            }
            if (string.IsNullOrEmpty(terrariaPath.Text))
            {
                var test = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\Steam\steamapps\common\Terraria\Terraria.exe";
                if (File.Exists(test)) terrariaPath.Text = test;
            }
            
            if (!string.IsNullOrEmpty(terrariaPath.Text))
                openFileDialog.InitialDirectory = Path.GetDirectoryName(terrariaPath.Text);

            CheckInstallationFolder();

            buffs = new List<Buff>();
            var buffNames = new string[] { "Obsidian Skin", "Regeneration", "Swiftness", "Gills", "Ironskin", "Mana Regeneration", "Magic Power", "Featherfall", "Spelunker", "Invisibility", "Shine", "Night Owl", "Battle", "Thorns", "Water Walking", "Archery", "Hunter", "Gravitation", "Shadow Orb", "Poisoned", "Potion Sickness", "Darkness", "Cursed", "On Fire!", "Tipsy", "Well Fed", "Fairy", "Werewolf", "Clairvoyance", "Bleeding", "Confused", "Slow", "Weak", "Merfolk", "Silenced", "Broken Armor", "Horrified", "The Tongue", "Cursed Inferno", "Pet Bunny", "Baby Penguin", "Pet Turtle", "Paladin's Shield", "Frostburn", "Baby Eater", "Chilled", "Frozen", "Honey", "Pygmies", "Baby Skeletron Head", "Baby Hornet", "Tiki Spirit", "Pet Lizard", "Pet Parrot", "Baby Truffle", "Pet Sapling", "Wisp", "Rapid Healing", "Shadow Dodge", "Leaf Crystal", "Baby Dinosaur", "Ice Barrier", "Panic!", "Baby Slime", "Eyeball Spring", "Baby Snowman", "Burning", "Suffocation", "Ichor", "Venom", "Midas", "Weapon Imbue: Venom", "Weapon Imbue: Cursed Flames", "Weapon Imbue: Fire", "Weapon Imbue: Gold", "Weapon Imbue: Ichor", "Weapon Imbue: Nanites", "Weapon Imbue: Confetti", "Weapon Imbue: Poison", "Blackout", "Pet Spider", "Squashling", "Ravens", "Black Cat", "Cursed Sapling", "Water Candle", "Campfire", "Chaos State", "Heart Lamp", "Rudolph", "Puppy", "Baby Grinch", "Ammo Box", "Mana Sickness", "Beetle Endurance", "Beetle Endurance", "Beetle Endurance", "Beetle Might", "Beetle Might", "Beetle Might", "Fairy", "Fairy", "Wet", "Mining", "Heartreach", "Calm", "Builder", "Titan", "Flipper", "Summoning", "Dangersense", "Ammo Reservation", "Lifeforce", "Endurance", "Rage", "Inferno", "Wrath", "Minecart", "Lovestruck", "Stinky", "Fishing", "Sonar", "Crate", "Warmth", "Hornet", "Imp", "Zephyr Fish", "Bunny Mount", "Pigron Mount", "Slime Mount", "Turtle Mount", "Bee Mount", "Spider", "Twins", "Pirate", "Mini Minotaur", "Slime", "Minecart", "Sharknado", "UFO", "UFO Mount", "Drill Mount", "Scutlix Mount", "Electrified", "The Line", "Happy!", "Banner", "Feral Bite", "Webbed", "Bewitched", "Life Drain", "Magic Lantern", "Shadowflame", "Baby Face Monster", "Crimson Heart", "Stoned", "Peace Candle", "Star in a Bottle", "Sharpened", "Dazed", "Deadly Sphere", "", "Obstructed", "Distorted", "Dryad's Blessing", "Minecart", "Minecart", "", "Penetrated", "Solar Blaze", "Solar Blaze", "Solar Blaze", "Life Nebula", "Life Nebula", "Life Nebula", "Mana Nebula", "Mana Nebula", "Mana Nebula", "Damage Nebula", "Damage Nebula", "Damage Nebula", "Stardust Cell", "Celled", "Minecart", "Minecart", "Dryad's Bane", "Stardust Guardian", "Stardust Dragon", "Daybroken", "Suspicious Looking Eye" };
            for (int i = 0; i < buffNames.Length; i++)
                buffs.Add(new Buff() { Name = buffNames[i], Index = i + 1 });
            buffs.Sort();

            LoadConfig();
        }

        private void CheckInstallationFolder()
        {
            if (!string.IsNullOrEmpty(terrariaPath.Text) && Environment.CurrentDirectory == Path.GetDirectoryName(terrariaPath.Text))
            {
                MessageBox.Show("Do not run TerrariaPatcher from within your Terraria installation folder.", Program.AssemblyName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }
        
        public void LoadConfig()
        {
            loading = true;

            try
            {
                timeEnabled.Checked = bool.Parse(IniAPI.ReadIni("General", "DisplayTime", "true", 255, ConfigPath));
                removeRodBuffEnabled.Checked = bool.Parse(IniAPI.ReadIni("General", "RemoveRodOfDiscordBuff", "true", 255, ConfigPath));
                removePotionSickness.Checked = bool.Parse(IniAPI.ReadIni("General", "RemovePotionSickness", "true", 255, ConfigPath));
                removeManaCosts.Checked = bool.Parse(IniAPI.ReadIni("General", "RemoveManaCosts", "true", 255, ConfigPath));
                removeAnglerQuestLimit.Checked = bool.Parse(IniAPI.ReadIni("General", "RemoveAnglerQuestLimit", "false", 255, ConfigPath));
                removeDrowning.Checked = bool.Parse(IniAPI.ReadIni("General", "RemoveDrowning", "true", 255, ConfigPath));
                oneHitKill.Checked = bool.Parse(IniAPI.ReadIni("General", "OneHitKill", "false", 255, ConfigPath));
                infiniteAmmo.Checked = bool.Parse(IniAPI.ReadIni("General", "InfiniteAmmo", "true", 255, ConfigPath));
                fixedPrefixes.Checked = bool.Parse(IniAPI.ReadIni("General", "FixedPrefixes", "true", 255, ConfigPath));
                accessoryPrefix.Text = IniAPI.ReadIni("General", "AccessoryPrefix", "Warding", 255, ConfigPath);
                permanentWings.Checked = bool.Parse(IniAPI.ReadIni("General", "PermanentWings", "true", 255, ConfigPath));
                infiniteCloudJumps.Checked = bool.Parse(IniAPI.ReadIni("General", "InfiniteCloudJumps", "false", 255, ConfigPath));
                functionalSocialSlots.Checked = bool.Parse(IniAPI.ReadIni("General", "FunctionalSocialSlots", "true", 255, ConfigPath));
                maxCraftingRange.Checked = bool.Parse(IniAPI.ReadIni("General", "MaxCraftingRange", "true", 255, ConfigPath));
#if !PUBLIC
                steamFixEnabled.Checked = bool.Parse(IniAPI.ReadIni("General", "SteamFixEnabled", "true", 255, ConfigPath));
#else
                steamFixEnabled.Enabled = false;
                steamFixEnabled.Checked = false;
#endif
                plugins.Checked = bool.Parse(IniAPI.ReadIni("General", "Plugins", "true", 255, ConfigPath));
                
                vampiricKnivesHealingRate.Value = decimal.Parse(IniAPI.ReadIni("Healing", "VampiricKnivesHealingRate", (7.5f).ToString(), 255, ConfigPath));
                spectreHealingRate.Value = decimal.Parse(IniAPI.ReadIni("Healing", "SpectreHealingRate", "20", 255, ConfigPath));

                spawnRateVoodoo.Value = decimal.Parse(IniAPI.ReadIni("Spawning", "SpawnRateVoodoo", "15", 255, ConfigPath));

                ResetBuffs();
                foreach (var index in IniAPI.ReadIni("PermanentBuffs", "List", "3, 5, 11, 14, 26, 60, 58, 7, 146", 500, ConfigPath).Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    var nIndex = int.Parse(index.Trim());
                    MoveIn(buffs.Find(buff => buff.Index == nIndex));
                }
            }
            finally
            {
                loading = false;
            }
        }
        public void SaveConfig()
        {
            if (loading) return;

            IniAPI.WriteIni("General", "DisplayTime", timeEnabled.Checked.ToString(), ConfigPath);
            IniAPI.WriteIni("General", "RemoveRodOfDiscordBuff", removeRodBuffEnabled.Checked.ToString(), ConfigPath);
            IniAPI.WriteIni("General", "RemovePotionSickness", removePotionSickness.Checked.ToString(), ConfigPath);
            IniAPI.WriteIni("General", "RemoveManaCosts", removeManaCosts.Checked.ToString(), ConfigPath);
            IniAPI.WriteIni("General", "RemoveAnglerQuestLimit", removeAnglerQuestLimit.Checked.ToString(), ConfigPath);
            IniAPI.WriteIni("General", "RemoveDrowning", removeDrowning.Checked.ToString(), ConfigPath);
            IniAPI.WriteIni("General", "OneHitKill", oneHitKill.Checked.ToString(), ConfigPath);
            IniAPI.WriteIni("General", "InfiniteAmmo", infiniteAmmo.Checked.ToString(), ConfigPath);
            IniAPI.WriteIni("General", "FixedPrefixes", fixedPrefixes.Checked.ToString(), ConfigPath);
            IniAPI.WriteIni("General", "AccessoryPrefix", accessoryPrefix.Text, ConfigPath);
            IniAPI.WriteIni("General", "PermanentWings", permanentWings.Checked.ToString(), ConfigPath);
            IniAPI.WriteIni("General", "InfiniteCloudJumps", infiniteCloudJumps.Checked.ToString(), ConfigPath);
            IniAPI.WriteIni("General", "FunctionalSocialSlots", functionalSocialSlots.Checked.ToString(), ConfigPath);
            IniAPI.WriteIni("General", "MaxCraftingRange", maxCraftingRange.Checked.ToString(), ConfigPath);
            IniAPI.WriteIni("General", "SteamFixEnabled", steamFixEnabled.Checked.ToString(), ConfigPath);
            IniAPI.WriteIni("General", "Plugins", plugins.Checked.ToString(), ConfigPath);

            IniAPI.WriteIni("Healing", "VampiricKnivesHealingRate", vampiricKnivesHealingRate.Value.ToString(), ConfigPath);
            IniAPI.WriteIni("Healing", "SpectreHealingRate", spectreHealingRate.Value.ToString(), ConfigPath);

            IniAPI.WriteIni("Spawning", "SpawnRateVoodoo", spawnRateVoodoo.Value.ToString(), ConfigPath);

            IniAPI.WriteIni("PermanentBuffs", "List", string.Join(", ", (from Buff buff in buffs.Where(buff => buff.Active) select buff.Index)), ConfigPath);
        }

        private void buffs_Update(object sender, EventArgs e)
        {
            moveOut.Enabled = buffsIn.SelectedIndex >= 0;
            moveIn.Enabled = buffsOut.SelectedIndex >= 0 && buffsIn.Items.Count < 22;
            allOut.Enabled = buffsIn.Items.Count > 0;

            buffsOutCount.Text = "(list of possibilities) [" + buffsOut.Items.Count + "]";
            buffsInCount.Text = "(actual persistent buffs) [" + buffsIn.Items.Count + " / 22]";

            SaveConfig();
        }

        private void ResetBuffs()
        {
            buffsIn.Items.Clear();
            buffsOut.Items.Clear();
            foreach (var buff in buffs)
            {
                buff.Active = false;

                if (CheckIncludedByFilter(buff))
                    buffsOut.Items.Add(buff);
            }

            buffs_Update(null, null);
        }

        private void moveIn_Click(object sender, EventArgs e)
        {
            MoveIn((Buff)buffsOut.SelectedItem);
        }

        private void MoveIn(Buff buff)
        {
            buff.Active = true;
            buffsOut.Items.Remove(buff);

            if (CheckIncludedByFilter(buff))
                buffsIn.Items.Add(buff);

            buffs_Update(null, null);
        }

        private void moveOut_Click(object sender, EventArgs e)
        {
            var buff = (Buff)buffsIn.SelectedItem;
            buff.Active = false;
            buffsIn.Items.Remove(buff);

            if (CheckIncludedByFilter(buff))
            {
                int i;
                for (i = buffsOut.Items.Count - 1; i >= 0; i--)
                {
                    var test = buffsOut.Items[i] as Buff;
                    if (test.CompareTo(buff) <= 0)
                    {
                        buffsOut.Items.Insert(i + 1, buff);
                        break;
                    }
                }

                if (i == -1) buffsOut.Items.Insert(0, buff);
            }

            buffs_Update(null, null);
        }

        private void allOut_Click(object sender, EventArgs e)
        {
            ResetBuffs();
        }

        private void browse_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                terrariaPath.Text = openFileDialog.FileName;
            }
        }

        private void save_Click(object sender, EventArgs e)
        {
            if (!File.Exists(terrariaPath.Text))
            {
                MessageBox.Show("Terraria path needs to point at a valid executable.", Program.AssemblyName + " :: Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Utils.IsProcessElevated && plugins.Checked && !steamFixEnabled.Checked)
            {
                MessageBox.Show("Warning, your account does not have elevated administrator privileges. After patching, you might need to run Steam with elevated administrator privileges before running Terraria.", Program.AssemblyName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            CheckInstallationFolder();

            saveFileDialog.InitialDirectory = Path.GetDirectoryName(terrariaPath.Text);
            saveFileDialog.FileName = "Terraria.exe";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                var original = terrariaPath.Text;

                if (File.Exists(saveFileDialog.FileName + ".bak"))
                {
                    var warning = "";
                    try
                    {
                        var versionCurrent = IL.GetAssemblyVersion(saveFileDialog.FileName);
                        var versionBackup = IL.GetAssemblyVersion(saveFileDialog.FileName + ".bak");
                        if (versionCurrent != versionBackup)
                        {
                            warning = Environment.NewLine + Environment.NewLine + "WARNING: Your Terraria.exe is version " + versionCurrent + " and your Terraria.exe.bak is version " + versionBackup +
                                      ".";
                            if (versionCurrent > versionBackup)
                                warning += " It is not recommended to restore a backup of an older version of Terraria!";
                        }
                    }
                    catch
                    { }

                    if (MessageBox.Show("Would you like to restore your backup before patching?" + warning, Program.AssemblyName, MessageBoxButtons.YesNo, string.IsNullOrEmpty(warning) ? MessageBoxIcon.Question : MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        File.Delete(saveFileDialog.FileName);
                        File.Move(saveFileDialog.FileName + ".bak", saveFileDialog.FileName);
                    }
                }
                
                if (File.Exists(saveFileDialog.FileName))
                {
                    if (MessageBox.Show("Would you like to backup the existing file?", Program.AssemblyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        File.Copy(saveFileDialog.FileName, saveFileDialog.FileName + ".bak", true);
                    }
                }

                var buffValues = (from Buff buff in buffs.Where(buff => buff.Active) select buff.Index).ToList();
                var details = new TerrariaDetails()
                {
                    InfiniteCloudJumps = infiniteCloudJumps.Checked,
                    FunctionalSocialSlots = functionalSocialSlots.Checked,
                    VampiricHealing = (float)vampiricKnivesHealingRate.Value,
                    SpectreHealing = (float)spectreHealingRate.Value,
                    FixedPrefixes = fixedPrefixes.Checked,
					AccessoryPrefix = SetAccessoryPrefixValue(),
                    DisplayTime = timeEnabled.Checked,
                    PermanentBuffs = buffValues,
                    PermanentWings = permanentWings.Enabled && permanentWings.Checked,
                    OneHitKill = oneHitKill.Checked,
					InfiniteAmmo = infiniteAmmo.Checked,
                    RemoveDrowning = removeDrowning.Checked,
                    RemoveDiscordBuff = removeRodBuffEnabled.Checked,
                    RemoveManaCost = removeManaCosts.Checked,
                    RemovePotionSickness = removePotionSickness.Checked,
                    RemoveAnglerQuestLimit = removeAnglerQuestLimit.Checked,
                    MaxCraftingRange = maxCraftingRange.Checked,
                    SpawnRateVoodoo = (int)spawnRateVoodoo.Value,
                    SteamFix = steamFixEnabled.Enabled && steamFixEnabled.Checked,
                    Plugins = plugins.Checked,
                };
                try
                {
                    Terraria.Patch(original, saveFileDialog.FileName, details);

                    if (details.Plugins)
                    {
                        var targetFolder = Path.GetDirectoryName(saveFileDialog.FileName);
                        foreach (var t in new[] {"PluginLoader.XNA.dll", "PluginLoader.FNA.dll"})
                        {
                            var target = $"{targetFolder}\\{t}";

                            var pluginLoaderInfo = new FileInfo(target);
                            while (Utils.IsFileLocked(pluginLoaderInfo))
                            {
                                var result = MessageBox.Show(target + " is in use. Please close Terraria then hit OK.", Program.AssemblyName, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                                if (result == DialogResult.Cancel)
                                    return;
                            }

                            File.Copy(t, target, true);
                        }

                        if (!Directory.Exists(@".\Plugins"))
                            MessageBox.Show("Plugins folder is missing from TerrariaPatcher folder. Please re-download.", Program.AssemblyName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        else if (!Directory.Exists(@".\Plugins\Shared"))
                            MessageBox.Show(@"Plugins\Shared folder is missing from TerrariaPatcher folder. Please re-download.", Program.AssemblyName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        else
                            new CopyPlugins(targetFolder).ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    Program.ShowErrorMessage("An error occurred, you possibly have already patched this exe or it is an incompatible version.\n\n" + ex.ToString());
                }

                MessageBox.Show("Done.", Program.AssemblyName);
            }
        }
        
		private int SetAccessoryPrefixValue()
		{
			int accessoryValue;

			switch (accessoryPrefix.Text)
			{
				case "Arcane":
					accessoryValue = 66;
					break;
				case "Lucky":
					accessoryValue = 68;
					break;
				case "Menacing":
					accessoryValue = 72;
					break;
				case "Quick":
					accessoryValue = 76;
					break;
				case "Violent":
					accessoryValue = 80;
					break;
				case "Warding":
					accessoryValue = 65;
					break;
				default:
					accessoryValue = 65;
					break;
			}

			return accessoryValue;
		}

        private void wings_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == permanentWings && permanentWings.Checked)
                infiniteCloudJumps.Checked = !permanentWings.Checked;
            else if (sender == infiniteCloudJumps && infiniteCloudJumps.Checked)
                permanentWings.Checked = !infiniteCloudJumps.Checked;

            SaveConfig();
        }

        private void config_Changed(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(linkLabel.Text);
        }

        private void removeAnglerQuestLimit_CheckedChanged(object sender, EventArgs e)
        {
            if (removeAnglerQuestLimit.Checked && !loading)
                MessageBox.Show("This mod is reported to break Steam achievements for the Angler. It will still allow you to get in-game achievements though.", Program.AssemblyName,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            SaveConfig();
        }

        private bool CheckIncludedByFilter(Buff buff)
        {
            string filter = (buff.Active ? filterBuffsIn : filterBuffsOut).Text.ToLower();
            return buff.ToString().ToLower().Contains(filter);
        }

        private void filterBuffsOut_TextChanged(object sender, EventArgs e)
        {
            buffsOut.Items.Clear();

            foreach (var buff in buffs)
            {
                if (!buff.Active && CheckIncludedByFilter(buff))
                    buffsOut.Items.Add(buff);
            }

            buffs_Update(null, null);
        }

        private void filterBuffsIn_TextChanged(object sender, EventArgs e)
        {
            buffsIn.Items.Clear();

            foreach (var buff in buffs)
            {
                if (buff.Active && CheckIncludedByFilter(buff))
                    buffsIn.Items.Add(buff);
            }

            buffs_Update(null, null);
        }
    }
    class Buff : IComparable<Buff>
    {
        public string Name;
        public int Index;
        public bool Active;

        public int CompareTo(Buff other)
        {
            if (other == null) return 0;
            var result = this.Name.CompareTo(other.Name);
            return result == 0 ? this.Index.CompareTo(other.Index) : result;
        }

        public override string ToString()
        {
            return "[" + Index + "] " + Name;
        }
    }
}
