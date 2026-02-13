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
        private static readonly int[] defaultBuffs = new[] { 3, 5, 11, 14, 26, 60, 58, 7, 146 };
        private static readonly int[] goodBuffs =
	        new[]
	        {
		        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 26, 29, 43, 48, 58, 59, 60, 62, 63, 71, 73, 74, 75, 76, 77, 78, 79, 87, 89, 93, 95, 96, 97, 98, 99, 100, 104, 105, 106,
		        107,
		        108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 121, 122, 123, 124, 146, 147, 150, 151, 157, 158, 159, 165, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 192, 198, 205,
		        206, 207, 215, 257, 306, 308, 311, 312, 314, 343, 348
	        };
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

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
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

                            // Rename the currently executing TerrariaPatcher.exe / PluginLoader.XNA.dll / Mono.Cecil.dll so that we can update
                            var location = Path.Combine(Environment.CurrentDirectory, "TerrariaPatcher.exe");
                            if (File.Exists(location))
                                File.Move(location, location.Replace("exe", "tmp"));
                            location = Path.Combine(Environment.CurrentDirectory, "PluginLoader.XNA.dll");
                            if (File.Exists(location))
                                File.Move(location, location.Replace("dll", "tmp"));
                            location = Path.Combine(Environment.CurrentDirectory, "Mono.Cecil.dll");
                            if (File.Exists(location))
                                File.Move(location, location.Replace("dll", "tmp"));
                            location = Path.Combine(Environment.CurrentDirectory, "Mono.Cecil.Rocks.dll");
                            if (File.Exists(location))
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
            var buffNames = new string[] { "Obsidian Skin", "Regeneration", "Swiftness", "Gills", "Ironskin", "Mana Regeneration", "Magic Power", "Featherfall", "Spelunker", "Invisibility", "Shine", "Night Owl", "Battle", "Thorns", "Water Walking", "Archery", "Hunter", "Gravitation", "Shadow Orb", "Poisoned", "Potion Sickness", "Darkness", "Cursed", "On Fire!", "Tipsy", "Well Fed", "Fairy (Blue)", "Werewolf", "Clairvoyance", "Bleeding", "Confused", "Slow", "Weak", "Merfolk", "Silenced", "Broken Armor", "Horrified", "The Tongue", "Cursed Inferno", "Pet Bunny", "Baby Penguin", "Pet Turtle", "Paladin's Shield", "Frostburn", "Baby Eater", "Chilled", "Frozen", "Honey", "Pygmies", "Baby Skeletron Head", "Baby Hornet", "Tiki Spirit", "Pet Lizard", "Pet Parrot", "Baby Truffle", "Pet Sapling", "Wisp", "Rapid Healing", "Holy Protection", "Leaf Crystal", "Baby Dinosaur", "Ice Barrier", "Panic!", "Baby Slime", "Eyeball Spring", "Baby Snowman", "Burning", "Suffocation", "Ichor", "Acid Venom", "Weapon Imbue: Acid Venom", "Midas", "Weapon Imbue: Cursed Flames", "Weapon Imbue: Fire", "Weapon Imbue: Gold", "Weapon Imbue: Ichor", "Weapon Imbue: Nanites", "Weapon Imbue: Confetti", "Weapon Imbue: Poison", "Blackout", "Pet Spider", "Squashling", "Ravens", "Black Cat", "Cursed Sapling", "Water Candle", "Cozy Fire", "Chaos State", "Heart Lamp", "Rudolph", "Puppy", "Baby Grinch", "Ammo Box", "Mana Sickness", "Beetle Endurance (15%)", "Beetle Endurance (30%)", "Beetle Endurance (45%)", "Beetle Might (10%)", "Beetle Might (20%)", "Beetle Might (30%)", "Fairy (Red)", "Fairy (Green)", "Wet", "Mining", "Heartreach", "Calm", "Builder", "Titan", "Flipper", "Summoning", "Dangersense", "Ammo Reservation", "Lifeforce", "Endurance", "Rage", "Inferno", "Wrath", "Minecart (Left)", "Lovestruck", "Stinky", "Fishing", "Sonar", "Crate", "Warmth", "Hornet", "Imp", "Zephyr Fish", "Bunny Mount", "Pigron Mount", "Slime Mount", "Turtle Mount", "Bee Mount", "Spider", "Twins", "Pirate", "Mini Minotaur", "Slime", "Minecart (Right)", "Sharknado", "UFO", "UFO Mount", "Drill Mount", "Scutlix Mount", "Electrified", "Moon Bite", "Happy!", "Banner", "Feral Bite", "Webbed", "Bewitched", "Life Drain", "Magic Lantern", "Shadowflame", "Baby Face Monster", "Crimson Heart", "Stoned", "Peace Candle", "Star in a Bottle", "Sharpened", "Dazed", "Deadly Sphere", "Unicorn Mount", "Obstructed", "Distorted", "Dryad's Blessing", "Minecart (Mechanical (Right))", "Minecart (Mechanical (Left))", "Cute Fishron Mount", "Penetrated", "Solar Blaze (1 stack)", "Solar Blaze (2 stacks)", "Solar Blaze (3 stacks)", "Life Nebula (1 stack)", "Life Nebula (2 stacks)", "Life Nebula (3 stacks)", "Mana Nebula (1 stack)", "Mana Nebula (2 stacks)", "Mana Nebula (3 stacks)", "Damage Nebula (1 stack)", "Damage Nebula (2 stacks)", "Damage Nebula (3 stacks)", "Stardust Cell (Stardust Minion)", "Celled", "Minecart (Wooden (Right))", "Minecart (Wooden (Left))", "Dryad's Bane", "Stardust Guardian", "Stardust Dragon", "Daybroken", "Suspicious Looking Eye", "Companion Cube", "Sugar Rush", "Basilisk Mount", "Mighty Wind", "Withered Armor", "Withered Weapon", "Oozed", "Striking Moment", "Creative Shock", "Propeller Gato", "Flickerwick", "Hoardagron", "Betsy's Curse", "Oiled", "Ballista Panic!", "Plenty Satisfied", "Exquisitely Stuffed", "Minecart (Desert (Right))", "Minecart (Desert (Left))", "Minecart (Minecarp (Right))", "Minecart (Minecarp (Left))", "Golf Cart", "Sanguine Bat", "Vampire Frog", "The Bast Defense", "Baby Finch", "Estee", "Sugar Glider", "Shark Pup", "Minecart (Bee (Right))", "Minecart (Bee (Left))", "Minecart (Ladybug (Right))", "Minecart (Ladybug (Left))", "Minecart (Pigron (Right))", "Minecart (Pigron (Left))", "Minecart (Sunflower (Right))", "Minecart (Sunflower (Left))", "Minecart (Demonic Hellcart (Right))", "Minecart (Demonic Hellcart (Left))", "Witch's Broom", "Minecart (Shroom (Right))", "Minecart (Shroom (Left))", "Minecart (Amethyst (Right))", "Minecart (Amethyst (Left))", "Minecart (Topaz (Right))", "Minecart (Topaz (Left))", "Minecart (Sapphire (Right))", "Minecart (Sapphire (Left))", "Minecart (Emerald (Right))", "Minecart (Emerald (Left))", "Minecart (Ruby (Right))", "Minecart (Ruby (Left))", "Minecart (Diamond (Right))", "Minecart (Diamond (Left))", "Minecart (Amber (Right))", "Minecart (Amber (Left))", "Minecart (Beetle (Right))", "Minecart (Beetle (Left))", "Minecart (Meowmere (Right))", "Minecart (Meowmere (Left))", "Minecart (Party (Right))", "Minecart (Party (Left))", "Minecart (The Dutchman (Right))", "Minecart (The Dutchman (Left))", "Minecart (Steampunk (Right))", "Minecart (Steampunk (Left))", "Lucky", "Lil' Harpy", "Fennec Fox", "Glittery Butterfly", "Baby Imp", "Baby Red Panda", "Desert Tiger", "Plantero", "Flamingo", "Dynamite Kitten", "Baby Werewolf", "Shadow Mimic", "Minecart (Coffin (Right))", "Minecart (Coffin (Left))", "Enchanted Daggers", "Digging Molecart (Left)", "Digging Molecart (Right)", "Volt Bunny", "Painted Horse Mount", "Majestic Horse Mount", "Dark Horse Mount", "Pogo Stick Mount", "Pirate Ship Mount", "Tree Mount", "Santank Mount", "Goat Mount", "Book Mount", "Slime Prince", "Suspicious Eye", "Eater of Worms", "Spider Brain", "Skeletron Jr.", "Honey Bee", "Destroyer-Lite", "Rez and Spaz", "Mini Prime", "Plantera Seedling", "Toy Golem", "Tiny Fishron", "Phantasmal Dragon", "Moonling", "Fairy Princess", "Jack 'O Lantern", "Everscream Sapling", "Ice Queen", "Alien Skater", "Baby Ogre", "Itsy Betsy", "Lava Shark Mount", "Titanium Barrier", "", "Durendal's Blessing", "", "", "Harvest Time", "A Nice Buff", "", "Jungle's Fury", "", "", "Slime Princess", "Winged Slime Mount", "", "Sparkle Slime", "Cerebral Mindtrick", "Terraprisma", "Hellfire", "Frostbite", "Flinx", "", "Bernie", "Glommer", "Tiny Deerclops", "Pig", "Chester", "Peckish", "Hungry", "Starving", "Abigail", "Hearty Meal", "", "Fart Kart", "Fart Kart", "", "Slime Royals", "Blessing of the Moon", "Biome Sight", "Blood Butchered", "Junimo", "Terra Fart Kart", "Terra Fart Kart", "Strategist", "Blue Chicken", "Shadow Candle", "Spiffo", "Caveling Gardener", "Shimmering", "The Dirtiest Block" };
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
                permanentWings.Checked = bool.Parse(IniAPI.ReadIni("General", "PermanentWings", "true", 255, ConfigPath));
                infiniteCloudJumps.Checked = bool.Parse(IniAPI.ReadIni("General", "InfiniteCloudJumps", "false", 255, ConfigPath));
                functionalSocialSlots.Checked = bool.Parse(IniAPI.ReadIni("General", "FunctionalSocialSlots", "true", 255, ConfigPath));
                maxCraftingRange.Checked = bool.Parse(IniAPI.ReadIni("General", "MaxCraftingRange", "true", 255, ConfigPath));
                pylonEverywhere.Checked = bool.Parse(IniAPI.ReadIni("General", "PylonEverywhere", "true", 255, ConfigPath));
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
                treasureBagsDropAll.Checked = bool.Parse(IniAPI.ReadIni("Spawning", "TreasureBagsDropAll", "false", 255, ConfigPath));

                ResetBuffs();
                MoveIn(IniAPI.ReadIni("PermanentBuffs", "List", string.Join(", ", defaultBuffs), 2048, ConfigPath)
	                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
	                .Select(s => int.Parse(s.Trim())));
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
            IniAPI.WriteIni("General", "PermanentWings", permanentWings.Checked.ToString(), ConfigPath);
            IniAPI.WriteIni("General", "InfiniteCloudJumps", infiniteCloudJumps.Checked.ToString(), ConfigPath);
            IniAPI.WriteIni("General", "FunctionalSocialSlots", functionalSocialSlots.Checked.ToString(), ConfigPath);
            IniAPI.WriteIni("General", "MaxCraftingRange", maxCraftingRange.Checked.ToString(), ConfigPath);
            IniAPI.WriteIni("General", "PylonEverywhere", pylonEverywhere.Checked.ToString(), ConfigPath);
            IniAPI.WriteIni("General", "SteamFixEnabled", steamFixEnabled.Checked.ToString(), ConfigPath);
            IniAPI.WriteIni("General", "Plugins", plugins.Checked.ToString(), ConfigPath);

            IniAPI.WriteIni("Healing", "VampiricKnivesHealingRate", vampiricKnivesHealingRate.Value.ToString(), ConfigPath);
            IniAPI.WriteIni("Healing", "SpectreHealingRate", spectreHealingRate.Value.ToString(), ConfigPath);

            IniAPI.WriteIni("Spawning", "SpawnRateVoodoo", spawnRateVoodoo.Value.ToString(), ConfigPath);
            IniAPI.WriteIni("Spawning", "TreasureBagsDropAll", treasureBagsDropAll.Checked.ToString(), ConfigPath);

            IniAPI.WriteIni("PermanentBuffs", "List", string.Join(", ", (from Buff buff in buffs.Where(buff => buff.Active) select buff.Index)), ConfigPath);
        }

        private void buffs_Update(object sender, EventArgs e)
        {
            moveOut.Enabled = buffsIn.SelectedIndex >= 0;
            moveIn.Enabled = buffsOut.SelectedIndex >= 0;
            allIn.Enabled = buffsOut.Items.Count > 0;
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
            MoveIn(new[] { (Buff)buffsOut.SelectedItem });
        }

        private bool MoveIn(IEnumerable<int> b)
        {
            return MoveIn(b.Select(i => buffs.Find(buff => buff.Index == i)));
        }

        private bool MoveIn(IEnumerable<Buff> b)
        {
	        var any = false;
	        foreach (var buff in b)
	        {
		        if (!buff.Active)
		        {
			        buff.Active = true;
			        buffsOut.Items.Remove(buff);

			        if (CheckIncludedByFilter(buff))
				        buffsIn.Items.Add(buff);

			        any = true;
		        }
	        }

	        buffs_Update(null, null);
	        return any;
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

        private void allIn_Click(object sender, EventArgs e)
        {
	        if (!MoveIn(goodBuffs))
		        MoveIn(buffs);
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
                if (buffValues.Count > 22)
	                MessageBox.Show("Adding more than 22 persistent buffs is not possible without additional game modifications (tModLoader, etc).", Program.AssemblyName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                var details = new TerrariaDetails()
                {
                    InfiniteCloudJumps = infiniteCloudJumps.Checked,
                    FunctionalSocialSlots = functionalSocialSlots.Checked,
                    VampiricHealing = (float)vampiricKnivesHealingRate.Value,
                    SpectreHealing = (float)spectreHealingRate.Value,
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
                    PylonEverywhere = pylonEverywhere.Checked,
                    SpawnRateVoodoo = (int)spawnRateVoodoo.Value,
                    BossBagsDropAllLoot = treasureBagsDropAll.Checked,
                    SteamFix = steamFixEnabled.Enabled && steamFixEnabled.Checked,
                    Plugins = plugins.Checked,
                };
                try
                {
                    Terraria.Patch(original, saveFileDialog.FileName, details);

                    if (details.Plugins)
                    {
                        var targetFolder = Path.GetDirectoryName(saveFileDialog.FileName);
                        foreach (var source in new[] {"PluginLoader.XNA.dll"})
                        {
	                        var sourceFileInfo = new FileInfo(source);
                            var target = $"{targetFolder}\\{source}";

                            if (!sourceFileInfo.Exists)
                            {
                                MessageBox.Show(source + " is missing.", Program.AssemblyName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                continue;
                            }

                            var targetFileInfo = new FileInfo(target);
                            while (Utils.IsFileLocked(targetFileInfo))
                            {
                                var result = MessageBox.Show(target + " is in use. Please close Terraria then hit OK.", Program.AssemblyName, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                                if (result == DialogResult.Cancel)
                                    return;
                            }

                            File.Copy(source, target, true);
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
