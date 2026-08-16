namespace TerrariaPatcher
{
    partial class CopyPlugins
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
            this.checkedListBox = new System.Windows.Forms.CheckedListBox();
            this.copyButton = new System.Windows.Forms.Button();
            this.clearExisting = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // checkedListBox
            // 
            this.checkedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedListBox.CheckOnClick = true;
            this.checkedListBox.FormattingEnabled = true;
            this.checkedListBox.Location = new System.Drawing.Point(0, 30);
            this.checkedListBox.Name = "checkedListBox";
            this.checkedListBox.Size = new System.Drawing.Size(284, 199);
            this.checkedListBox.TabIndex = 0;
            this.checkedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBox_ItemCheck);
            // 
            // copyButton
            // 
            this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.copyButton.Location = new System.Drawing.Point(0, 229);
            this.copyButton.Name = "copyButton";
            this.copyButton.Size = new System.Drawing.Size(284, 23);
            this.copyButton.TabIndex = 1;
            this.copyButton.Text = "Sync";
            this.copyButton.UseVisualStyleBackColor = true;
            this.copyButton.Click += new System.EventHandler(this.copyButton_Click);
            // 
            // clearExisting
            // 
            this.clearExisting.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clearExisting.AutoSize = true;
            this.clearExisting.Checked = true;
            this.clearExisting.CheckState = System.Windows.Forms.CheckState.Checked;
            this.clearExisting.Location = new System.Drawing.Point(3, 7);
            this.clearExisting.Name = "clearExisting";
            this.clearExisting.Size = new System.Drawing.Size(238, 17);
            this.clearExisting.TabIndex = 2;
            this.clearExisting.Text = "Remove Unchecked Plugins (recommended)";
            this.clearExisting.UseVisualStyleBackColor = true;
            this.clearExisting.CheckedChanged += new System.EventHandler(this.clearExisting_CheckedChanged);
            // 
            // CopyPlugins
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 252);
            this.Controls.Add(this.clearExisting);
            this.Controls.Add(this.copyButton);
            this.Controls.Add(this.checkedListBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "CopyPlugins";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Plugins";
            this.Shown += new System.EventHandler(this.CopyPlugins_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox checkedListBox;
        private System.Windows.Forms.Button copyButton;
        private System.Windows.Forms.CheckBox clearExisting;
    }
}