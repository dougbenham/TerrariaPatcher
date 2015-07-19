namespace TerrariaPatcher
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.spawnRateVoodoo = new System.Windows.Forms.NumericUpDown();
            this.timeEnabled = new System.Windows.Forms.CheckBox();
            this.steamFixEnabled = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.plugins = new System.Windows.Forms.CheckBox();
            this.maxCraftingRange = new System.Windows.Forms.CheckBox();
            this.infiniteCloudJumps = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.accessoryPrefix = new System.Windows.Forms.ComboBox();
            this.fixedPrefixes = new System.Windows.Forms.CheckBox();
            this.infiniteAmmo = new System.Windows.Forms.CheckBox();
            this.oneHitKill = new System.Windows.Forms.CheckBox();
            this.demigodMode = new System.Windows.Forms.CheckBox();
            this.removeDrowning = new System.Windows.Forms.CheckBox();
            this.removeAnglerQuestLimit = new System.Windows.Forms.CheckBox();
            this.permanentWings = new System.Windows.Forms.CheckBox();
            this.removeManaCosts = new System.Windows.Forms.CheckBox();
            this.removePotionSickness = new System.Windows.Forms.CheckBox();
            this.removeRodBuffEnabled = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.allOut = new System.Windows.Forms.Button();
            this.buffsInCount = new System.Windows.Forms.Label();
            this.buffsOutCount = new System.Windows.Forms.Label();
            this.moveOut = new System.Windows.Forms.Button();
            this.buffsIn = new System.Windows.Forms.ListBox();
            this.buffsOut = new System.Windows.Forms.ListBox();
            this.moveIn = new System.Windows.Forms.Button();
            this.save = new System.Windows.Forms.Button();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.terrariaPath = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.browse = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.vampiricKnivesHealingRate = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.spectreHealingRate = new System.Windows.Forms.NumericUpDown();
            this.linkLabel = new System.Windows.Forms.LinkLabel();
            this.versionWarning = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spawnRateVoodoo)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.vampiricKnivesHealingRate)).BeginInit();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spectreHealingRate)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.spawnRateVoodoo);
            this.groupBox1.Location = new System.Drawing.Point(640, 96);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(308, 70);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Spawning:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(3, 16);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(220, 13);
            this.label10.TabIndex = 17;
            this.label10.Text = "Spawn rate for Voodoo Demon (0% to 100%):";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(24, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(191, 24);
            this.label1.TabIndex = 2;
            this.label1.Text = "50% means for every regular Demon you see,\r\nyou\'ll see 1 Voodoo Demon.";
            // 
            // spawnRateVoodoo
            // 
            this.spawnRateVoodoo.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.spawnRateVoodoo.Location = new System.Drawing.Point(241, 14);
            this.spawnRateVoodoo.Name = "spawnRateVoodoo";
            this.spawnRateVoodoo.Size = new System.Drawing.Size(63, 20);
            this.spawnRateVoodoo.TabIndex = 4;
            this.spawnRateVoodoo.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.spawnRateVoodoo.ValueChanged += new System.EventHandler(this.config_Changed);
            // 
            // timeEnabled
            // 
            this.timeEnabled.AutoSize = true;
            this.timeEnabled.Checked = true;
            this.timeEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.timeEnabled.Location = new System.Drawing.Point(6, 19);
            this.timeEnabled.Name = "timeEnabled";
            this.timeEnabled.Size = new System.Drawing.Size(86, 17);
            this.timeEnabled.TabIndex = 2;
            this.timeEnabled.Text = "Display Time";
            this.timeEnabled.UseVisualStyleBackColor = true;
            this.timeEnabled.CheckedChanged += new System.EventHandler(this.config_Changed);
            // 
            // steamFixEnabled
            // 
            this.steamFixEnabled.AutoSize = true;
            this.steamFixEnabled.Checked = true;
            this.steamFixEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.steamFixEnabled.Location = new System.Drawing.Point(5, 317);
            this.steamFixEnabled.Name = "steamFixEnabled";
            this.steamFixEnabled.Size = new System.Drawing.Size(72, 17);
            this.steamFixEnabled.TabIndex = 4;
            this.steamFixEnabled.Text = "Steam Fix";
            this.steamFixEnabled.UseVisualStyleBackColor = true;
            this.steamFixEnabled.CheckedChanged += new System.EventHandler(this.config_Changed);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.plugins);
            this.groupBox2.Controls.Add(this.maxCraftingRange);
            this.groupBox2.Controls.Add(this.infiniteCloudJumps);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.accessoryPrefix);
            this.groupBox2.Controls.Add(this.fixedPrefixes);
            this.groupBox2.Controls.Add(this.infiniteAmmo);
            this.groupBox2.Controls.Add(this.oneHitKill);
            this.groupBox2.Controls.Add(this.demigodMode);
            this.groupBox2.Controls.Add(this.removeDrowning);
            this.groupBox2.Controls.Add(this.removeAnglerQuestLimit);
            this.groupBox2.Controls.Add(this.permanentWings);
            this.groupBox2.Controls.Add(this.removeManaCosts);
            this.groupBox2.Controls.Add(this.removePotionSickness);
            this.groupBox2.Controls.Add(this.removeRodBuffEnabled);
            this.groupBox2.Controls.Add(this.timeEnabled);
            this.groupBox2.Controls.Add(this.steamFixEnabled);
            this.groupBox2.Location = new System.Drawing.Point(12, 63);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(308, 362);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "General:";
            // 
            // plugins
            // 
            this.plugins.AutoSize = true;
            this.plugins.Checked = true;
            this.plugins.CheckState = System.Windows.Forms.CheckState.Checked;
            this.plugins.Location = new System.Drawing.Point(5, 340);
            this.plugins.Name = "plugins";
            this.plugins.Size = new System.Drawing.Size(215, 17);
            this.plugins.TabIndex = 32;
            this.plugins.Text = "Plugin support (loads from \\Plugins\\*.cs)";
            this.plugins.UseVisualStyleBackColor = true;
            this.plugins.CheckedChanged += new System.EventHandler(this.config_Changed);
            // 
            // maxCraftingRange
            // 
            this.maxCraftingRange.AutoSize = true;
            this.maxCraftingRange.Checked = true;
            this.maxCraftingRange.CheckState = System.Windows.Forms.CheckState.Checked;
            this.maxCraftingRange.Location = new System.Drawing.Point(5, 294);
            this.maxCraftingRange.Name = "maxCraftingRange";
            this.maxCraftingRange.Size = new System.Drawing.Size(120, 17);
            this.maxCraftingRange.TabIndex = 30;
            this.maxCraftingRange.Text = "Max Crafting Range";
            this.maxCraftingRange.UseVisualStyleBackColor = true;
            this.maxCraftingRange.CheckedChanged += new System.EventHandler(this.config_Changed);
            // 
            // infiniteCloudJumps
            // 
            this.infiniteCloudJumps.AutoSize = true;
            this.infiniteCloudJumps.Location = new System.Drawing.Point(5, 271);
            this.infiniteCloudJumps.Name = "infiniteCloudJumps";
            this.infiniteCloudJumps.Size = new System.Drawing.Size(120, 17);
            this.infiniteCloudJumps.TabIndex = 24;
            this.infiniteCloudJumps.Text = "Infinite Cloud Jumps";
            this.infiniteCloudJumps.UseVisualStyleBackColor = true;
            this.infiniteCloudJumps.CheckedChanged += new System.EventHandler(this.wings_CheckedChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(177, 208);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(88, 13);
            this.label11.TabIndex = 23;
            this.label11.Text = "Accessory Prefix:";
            // 
            // accessoryPrefix
            // 
            this.accessoryPrefix.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.accessoryPrefix.FormattingEnabled = true;
            this.accessoryPrefix.Items.AddRange(new object[] {
            "Arcane",
            "Lucky",
            "Menacing",
            "Quick",
            "Violent",
            "Warding"});
            this.accessoryPrefix.Location = new System.Drawing.Point(177, 225);
            this.accessoryPrefix.Margin = new System.Windows.Forms.Padding(2);
            this.accessoryPrefix.Name = "accessoryPrefix";
            this.accessoryPrefix.Size = new System.Drawing.Size(122, 21);
            this.accessoryPrefix.TabIndex = 22;
            this.accessoryPrefix.SelectedIndexChanged += new System.EventHandler(this.config_Changed);
            // 
            // fixedPrefixes
            // 
            this.fixedPrefixes.AutoSize = true;
            this.fixedPrefixes.Checked = true;
            this.fixedPrefixes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.fixedPrefixes.Location = new System.Drawing.Point(5, 226);
            this.fixedPrefixes.Margin = new System.Windows.Forms.Padding(2);
            this.fixedPrefixes.Name = "fixedPrefixes";
            this.fixedPrefixes.Size = new System.Drawing.Size(144, 17);
            this.fixedPrefixes.TabIndex = 21;
            this.fixedPrefixes.Text = "Always Roll Best Prefixes";
            this.fixedPrefixes.UseVisualStyleBackColor = true;
            this.fixedPrefixes.CheckedChanged += new System.EventHandler(this.config_Changed);
            // 
            // infiniteAmmo
            // 
            this.infiniteAmmo.AutoSize = true;
            this.infiniteAmmo.Checked = true;
            this.infiniteAmmo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.infiniteAmmo.Location = new System.Drawing.Point(5, 202);
            this.infiniteAmmo.Margin = new System.Windows.Forms.Padding(2);
            this.infiniteAmmo.Name = "infiniteAmmo";
            this.infiniteAmmo.Size = new System.Drawing.Size(147, 17);
            this.infiniteAmmo.TabIndex = 20;
            this.infiniteAmmo.Text = "Infinite Ammo (by Ryan S)";
            this.infiniteAmmo.UseVisualStyleBackColor = true;
            this.infiniteAmmo.CheckedChanged += new System.EventHandler(this.config_Changed);
            // 
            // oneHitKill
            // 
            this.oneHitKill.AutoSize = true;
            this.oneHitKill.Location = new System.Drawing.Point(6, 180);
            this.oneHitKill.Name = "oneHitKill";
            this.oneHitKill.Size = new System.Drawing.Size(78, 17);
            this.oneHitKill.TabIndex = 18;
            this.oneHitKill.Text = "One Hit Kill";
            this.oneHitKill.UseVisualStyleBackColor = true;
            this.oneHitKill.CheckedChanged += new System.EventHandler(this.config_Changed);
            // 
            // demigodMode
            // 
            this.demigodMode.AutoSize = true;
            this.demigodMode.Checked = true;
            this.demigodMode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.demigodMode.Location = new System.Drawing.Point(6, 157);
            this.demigodMode.Name = "demigodMode";
            this.demigodMode.Size = new System.Drawing.Size(147, 17);
            this.demigodMode.TabIndex = 17;
            this.demigodMode.Text = "Demigod Mode (can\'t die)";
            this.demigodMode.UseVisualStyleBackColor = true;
            this.demigodMode.CheckedChanged += new System.EventHandler(this.config_Changed);
            // 
            // removeDrowning
            // 
            this.removeDrowning.AutoSize = true;
            this.removeDrowning.Checked = true;
            this.removeDrowning.CheckState = System.Windows.Forms.CheckState.Checked;
            this.removeDrowning.Location = new System.Drawing.Point(6, 134);
            this.removeDrowning.Name = "removeDrowning";
            this.removeDrowning.Size = new System.Drawing.Size(114, 17);
            this.removeDrowning.TabIndex = 16;
            this.removeDrowning.Text = "Remove Drowning";
            this.removeDrowning.UseVisualStyleBackColor = true;
            this.removeDrowning.CheckedChanged += new System.EventHandler(this.config_Changed);
            // 
            // removeAnglerQuestLimit
            // 
            this.removeAnglerQuestLimit.AutoSize = true;
            this.removeAnglerQuestLimit.Location = new System.Drawing.Point(6, 111);
            this.removeAnglerQuestLimit.Name = "removeAnglerQuestLimit";
            this.removeAnglerQuestLimit.Size = new System.Drawing.Size(195, 17);
            this.removeAnglerQuestLimit.TabIndex = 15;
            this.removeAnglerQuestLimit.Text = "Remove Angler Quest Per Day Limit";
            this.removeAnglerQuestLimit.UseVisualStyleBackColor = true;
            this.removeAnglerQuestLimit.CheckedChanged += new System.EventHandler(this.removeAnglerQuestLimit_CheckedChanged);
            // 
            // permanentWings
            // 
            this.permanentWings.AutoSize = true;
            this.permanentWings.Checked = true;
            this.permanentWings.CheckState = System.Windows.Forms.CheckState.Checked;
            this.permanentWings.Location = new System.Drawing.Point(5, 248);
            this.permanentWings.Name = "permanentWings";
            this.permanentWings.Size = new System.Drawing.Size(238, 17);
            this.permanentWings.TabIndex = 12;
            this.permanentWings.Text = "Permanent Stardust Wings + Infinite Up Time";
            this.permanentWings.UseVisualStyleBackColor = true;
            this.permanentWings.CheckedChanged += new System.EventHandler(this.wings_CheckedChanged);
            // 
            // removeManaCosts
            // 
            this.removeManaCosts.AutoSize = true;
            this.removeManaCosts.Checked = true;
            this.removeManaCosts.CheckState = System.Windows.Forms.CheckState.Checked;
            this.removeManaCosts.Location = new System.Drawing.Point(6, 88);
            this.removeManaCosts.Name = "removeManaCosts";
            this.removeManaCosts.Size = new System.Drawing.Size(125, 17);
            this.removeManaCosts.TabIndex = 11;
            this.removeManaCosts.Text = "Remove Mana Costs";
            this.removeManaCosts.UseVisualStyleBackColor = true;
            this.removeManaCosts.CheckedChanged += new System.EventHandler(this.config_Changed);
            // 
            // removePotionSickness
            // 
            this.removePotionSickness.AutoSize = true;
            this.removePotionSickness.Checked = true;
            this.removePotionSickness.CheckState = System.Windows.Forms.CheckState.Checked;
            this.removePotionSickness.Location = new System.Drawing.Point(6, 65);
            this.removePotionSickness.Name = "removePotionSickness";
            this.removePotionSickness.Size = new System.Drawing.Size(180, 17);
            this.removePotionSickness.TabIndex = 10;
            this.removePotionSickness.Text = "Remove Potion Sickness Debuff";
            this.removePotionSickness.UseVisualStyleBackColor = true;
            this.removePotionSickness.CheckedChanged += new System.EventHandler(this.config_Changed);
            // 
            // removeRodBuffEnabled
            // 
            this.removeRodBuffEnabled.AutoSize = true;
            this.removeRodBuffEnabled.Checked = true;
            this.removeRodBuffEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.removeRodBuffEnabled.Location = new System.Drawing.Point(6, 42);
            this.removeRodBuffEnabled.Name = "removeRodBuffEnabled";
            this.removeRodBuffEnabled.Size = new System.Drawing.Size(175, 17);
            this.removeRodBuffEnabled.TabIndex = 5;
            this.removeRodBuffEnabled.Text = "Remove Rod of Discord Debuff";
            this.removeRodBuffEnabled.UseVisualStyleBackColor = true;
            this.removeRodBuffEnabled.CheckedChanged += new System.EventHandler(this.config_Changed);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.allOut);
            this.groupBox3.Controls.Add(this.buffsInCount);
            this.groupBox3.Controls.Add(this.buffsOutCount);
            this.groupBox3.Controls.Add(this.moveOut);
            this.groupBox3.Controls.Add(this.buffsIn);
            this.groupBox3.Controls.Add(this.buffsOut);
            this.groupBox3.Controls.Add(this.moveIn);
            this.groupBox3.Location = new System.Drawing.Point(326, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(308, 413);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Persistent Buffs:";
            // 
            // allOut
            // 
            this.allOut.Enabled = false;
            this.allOut.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.allOut.Location = new System.Drawing.Point(143, 226);
            this.allOut.Name = "allOut";
            this.allOut.Size = new System.Drawing.Size(23, 23);
            this.allOut.TabIndex = 6;
            this.allOut.Text = "<<";
            this.allOut.UseVisualStyleBackColor = true;
            this.allOut.Click += new System.EventHandler(this.allOut_Click);
            // 
            // buffsInCount
            // 
            this.buffsInCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buffsInCount.Location = new System.Drawing.Point(163, 393);
            this.buffsInCount.Name = "buffsInCount";
            this.buffsInCount.Size = new System.Drawing.Size(139, 12);
            this.buffsInCount.TabIndex = 5;
            this.buffsInCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buffsOutCount
            // 
            this.buffsOutCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buffsOutCount.Location = new System.Drawing.Point(6, 393);
            this.buffsOutCount.Name = "buffsOutCount";
            this.buffsOutCount.Size = new System.Drawing.Size(130, 12);
            this.buffsOutCount.TabIndex = 4;
            this.buffsOutCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // moveOut
            // 
            this.moveOut.Enabled = false;
            this.moveOut.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.moveOut.Location = new System.Drawing.Point(143, 198);
            this.moveOut.Name = "moveOut";
            this.moveOut.Size = new System.Drawing.Size(23, 23);
            this.moveOut.TabIndex = 3;
            this.moveOut.Text = "<";
            this.moveOut.UseVisualStyleBackColor = true;
            this.moveOut.Click += new System.EventHandler(this.moveOut_Click);
            // 
            // buffsIn
            // 
            this.buffsIn.FormattingEnabled = true;
            this.buffsIn.HorizontalScrollbar = true;
            this.buffsIn.Location = new System.Drawing.Point(172, 19);
            this.buffsIn.Name = "buffsIn";
            this.buffsIn.Size = new System.Drawing.Size(130, 355);
            this.buffsIn.TabIndex = 1;
            this.buffsIn.SelectedIndexChanged += new System.EventHandler(this.buffs_Update);
            // 
            // buffsOut
            // 
            this.buffsOut.FormattingEnabled = true;
            this.buffsOut.HorizontalScrollbar = true;
            this.buffsOut.Location = new System.Drawing.Point(6, 19);
            this.buffsOut.Name = "buffsOut";
            this.buffsOut.Size = new System.Drawing.Size(130, 355);
            this.buffsOut.TabIndex = 0;
            this.buffsOut.SelectedIndexChanged += new System.EventHandler(this.buffs_Update);
            // 
            // moveIn
            // 
            this.moveIn.Enabled = false;
            this.moveIn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.moveIn.Location = new System.Drawing.Point(143, 169);
            this.moveIn.Name = "moveIn";
            this.moveIn.Size = new System.Drawing.Size(23, 23);
            this.moveIn.TabIndex = 2;
            this.moveIn.Text = ">";
            this.moveIn.UseVisualStyleBackColor = true;
            this.moveIn.Click += new System.EventHandler(this.moveIn_Click);
            // 
            // save
            // 
            this.save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.save.Location = new System.Drawing.Point(865, 404);
            this.save.Name = "save";
            this.save.Size = new System.Drawing.Size(83, 23);
            this.save.TabIndex = 0;
            this.save.Text = "Patch && Save";
            this.save.UseVisualStyleBackColor = true;
            this.save.Click += new System.EventHandler(this.save_Click);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.Filter = "Executable files (*.exe)|*.exe";
            this.saveFileDialog.OverwritePrompt = false;
            // 
            // terrariaPath
            // 
            this.terrariaPath.Location = new System.Drawing.Point(6, 19);
            this.terrariaPath.Name = "terrariaPath";
            this.terrariaPath.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.terrariaPath.Size = new System.Drawing.Size(266, 20);
            this.terrariaPath.TabIndex = 8;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.browse);
            this.groupBox4.Controls.Add(this.terrariaPath);
            this.groupBox4.Location = new System.Drawing.Point(12, 12);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(308, 45);
            this.groupBox4.TabIndex = 0;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Terraria\'s location:";
            // 
            // browse
            // 
            this.browse.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.browse.Image = ((System.Drawing.Image)(resources.GetObject("browse.Image")));
            this.browse.Location = new System.Drawing.Point(278, 18);
            this.browse.Name = "browse";
            this.browse.Size = new System.Drawing.Size(24, 24);
            this.browse.TabIndex = 9;
            this.browse.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.browse.UseVisualStyleBackColor = true;
            this.browse.Click += new System.EventHandler(this.browse_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "Terraria.exe";
            this.openFileDialog.Filter = "Executable files (*.exe)|*.exe";
            // 
            // vampiricKnivesHealingRate
            // 
            this.vampiricKnivesHealingRate.DecimalPlaces = 1;
            this.vampiricKnivesHealingRate.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.vampiricKnivesHealingRate.Location = new System.Drawing.Point(236, 18);
            this.vampiricKnivesHealingRate.Name = "vampiricKnivesHealingRate";
            this.vampiricKnivesHealingRate.Size = new System.Drawing.Size(63, 20);
            this.vampiricKnivesHealingRate.TabIndex = 24;
            this.vampiricKnivesHealingRate.Value = new decimal(new int[] {
            75,
            0,
            0,
            65536});
            this.vampiricKnivesHealingRate.ValueChanged += new System.EventHandler(this.config_Changed);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 46);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(206, 13);
            this.label12.TabIndex = 24;
            this.label12.Text = "Spectre Armor Healing Rate (default 20%):";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 20);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(217, 13);
            this.label13.TabIndex = 26;
            this.label13.Text = "Vampiric Knives Healing Rate (default 7.5%):";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.spectreHealingRate);
            this.groupBox6.Controls.Add(this.label12);
            this.groupBox6.Controls.Add(this.label13);
            this.groupBox6.Controls.Add(this.vampiricKnivesHealingRate);
            this.groupBox6.Location = new System.Drawing.Point(640, 12);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(308, 78);
            this.groupBox6.TabIndex = 27;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Healing:";
            // 
            // spectreHealingRate
            // 
            this.spectreHealingRate.DecimalPlaces = 1;
            this.spectreHealingRate.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.spectreHealingRate.Location = new System.Drawing.Point(237, 44);
            this.spectreHealingRate.Name = "spectreHealingRate";
            this.spectreHealingRate.Size = new System.Drawing.Size(63, 20);
            this.spectreHealingRate.TabIndex = 27;
            this.spectreHealingRate.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.spectreHealingRate.ValueChanged += new System.EventHandler(this.config_Changed);
            // 
            // linkLabel
            // 
            this.linkLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.linkLabel.AutoSize = true;
            this.linkLabel.Location = new System.Drawing.Point(714, 388);
            this.linkLabel.Name = "linkLabel";
            this.linkLabel.Size = new System.Drawing.Size(234, 13);
            this.linkLabel.TabIndex = 28;
            this.linkLabel.TabStop = true;
            this.linkLabel.Text = "http://www.terrariaonline.com/threads/116592/";
            this.linkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            // 
            // versionWarning
            // 
            this.versionWarning.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.versionWarning.AutoSize = true;
            this.versionWarning.Location = new System.Drawing.Point(948, 375);
            this.versionWarning.Name = "versionWarning";
            this.versionWarning.Size = new System.Drawing.Size(0, 13);
            this.versionWarning.TabIndex = 29;
            this.versionWarning.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(956, 435);
            this.Controls.Add(this.versionWarning);
            this.Controls.Add(this.linkLabel);
            this.Controls.Add(this.save);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spawnRateVoodoo)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.vampiricKnivesHealingRate)).EndInit();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spectreHealingRate)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown spawnRateVoodoo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox timeEnabled;
        private System.Windows.Forms.CheckBox steamFixEnabled;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button moveOut;
        private System.Windows.Forms.ListBox buffsIn;
        private System.Windows.Forms.ListBox buffsOut;
        private System.Windows.Forms.Button moveIn;
        private System.Windows.Forms.Label buffsInCount;
        private System.Windows.Forms.Label buffsOutCount;
        private System.Windows.Forms.Button save;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.TextBox terrariaPath;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button browse;
        private System.Windows.Forms.Button allOut;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.CheckBox removeRodBuffEnabled;
        private System.Windows.Forms.CheckBox removePotionSickness;
        private System.Windows.Forms.CheckBox removeManaCosts;
        private System.Windows.Forms.CheckBox permanentWings;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox removeAnglerQuestLimit;
        private System.Windows.Forms.CheckBox oneHitKill;
        private System.Windows.Forms.CheckBox demigodMode;
        private System.Windows.Forms.CheckBox removeDrowning;
		private System.Windows.Forms.CheckBox infiniteAmmo;
        private System.Windows.Forms.CheckBox fixedPrefixes;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.ComboBox accessoryPrefix;
        private System.Windows.Forms.CheckBox infiniteCloudJumps;
        private System.Windows.Forms.NumericUpDown vampiricKnivesHealingRate;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.NumericUpDown spectreHealingRate;
        private System.Windows.Forms.CheckBox maxCraftingRange;
        private System.Windows.Forms.CheckBox plugins;
        private System.Windows.Forms.LinkLabel linkLabel;
        private System.Windows.Forms.Label versionWarning;
    }
}

