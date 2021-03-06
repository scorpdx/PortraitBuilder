﻿using System;

namespace PortraitBuilder.UI {
	partial class PortraitBuilderForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
				previewImage.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PortraitBuilderForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label24 = new System.Windows.Forms.Label();
            this.cbGovernment = new System.Windows.Forms.ComboBox();
            this.label22 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCopy = new System.Windows.Forms.Button();
            this.tbDNA = new System.Windows.Forms.TextBox();
            this.pbPortrait = new System.Windows.Forms.PictureBox();
            this.pbPortraitOnline = new System.Windows.Forms.PictureBox();
            this.cbRank = new System.Windows.Forms.ComboBox();
            this.label20 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabDNA = new System.Windows.Forms.TabPage();
            this.panelDNA = new System.Windows.Forms.FlowLayoutPanel();
            this.tabProperties = new System.Windows.Forms.TabPage();
            this.panelProperties = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cbCulturePortraitTypes = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.btnRandom = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.cbPortraitTypes = new System.Windows.Forms.ComboBox();
            this.tabContent = new System.Windows.Forms.TabControl();
            this.tabDLCs = new System.Windows.Forms.TabPage();
            this.panelDLCs = new System.Windows.Forms.FlowLayoutPanel();
            this.tabMods = new System.Windows.Forms.TabPage();
            this.panelMods = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnToogleAll = new System.Windows.Forms.Button();
            this.btnReload = new System.Windows.Forms.Button();
            this.lnkLblRemotePortrait = new System.Windows.Forms.LinkLabel();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPortrait)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbPortraitOnline)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabDNA.SuspendLayout();
            this.tabProperties.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabContent.SuspendLayout();
            this.tabDLCs.SuspendLayout();
            this.tabMods.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label24);
            this.groupBox1.Controls.Add(this.cbGovernment);
            this.groupBox1.Controls.Add(this.label22);
            this.groupBox1.Controls.Add(this.btnSave);
            this.groupBox1.Controls.Add(this.btnCopy);
            this.groupBox1.Controls.Add(this.tbDNA);
            this.groupBox1.Controls.Add(this.pbPortrait);
            this.groupBox1.Controls.Add(this.lnkLblRemotePortrait);
            this.groupBox1.Controls.Add(this.pbPortraitOnline);
            this.groupBox1.Controls.Add(this.cbRank);
            this.groupBox1.Controls.Add(this.label20);
            this.groupBox1.Location = new System.Drawing.Point(598, 12);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Size = new System.Drawing.Size(440, 543);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Output";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(10, 25);
            this.label24.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(35, 15);
            this.label24.TabIndex = 26;
            this.label24.Text = "Govt:";
            // 
            // cbGovernment
            // 
            this.cbGovernment.FormattingEnabled = true;
            this.cbGovernment.Items.AddRange(new object[] {
            "Feudal",
            "Iqta",
            "Theocracy",
            "Republic",
            "Merchant Republic",
            "Tribal",
            "Nomadic",
            "Monastic Feudal",
            "Chinese Imperial",
            "Confucian Bureaucracy"});
            this.cbGovernment.Location = new System.Drawing.Point(52, 22);
            this.cbGovernment.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbGovernment.Name = "cbGovernment";
            this.cbGovernment.Size = new System.Drawing.Size(158, 23);
            this.cbGovernment.TabIndex = 25;
            this.cbGovernment.SelectedIndexChanged += new System.EventHandler(this.onChangeGovernment);
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(4, 428);
            this.label22.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(205, 30);
            this.label22.TabIndex = 24;
            this.label22.Text = "*Warning: properties are dynamically \r\noverridden by portrait_properties.txt";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(7, 510);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(88, 27);
            this.btnSave.TabIndex = 22;
            this.btnSave.Text = "Save Image";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.onClickSave);
            // 
            // btnCopy
            // 
            this.btnCopy.Location = new System.Drawing.Point(124, 510);
            this.btnCopy.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(88, 27);
            this.btnCopy.TabIndex = 23;
            this.btnCopy.Text = "Copy";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.onClickCopy);
            // 
            // tbDNA
            // 
            this.tbDNA.Location = new System.Drawing.Point(7, 350);
            this.tbDNA.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbDNA.Multiline = true;
            this.tbDNA.Name = "tbDNA";
            this.tbDNA.ReadOnly = true;
            this.tbDNA.Size = new System.Drawing.Size(180, 75);
            this.tbDNA.TabIndex = 23;
            this.tbDNA.TabStop = false;
            this.tbDNA.Text = "dna\r\nproperties";
            // 
            // pbPortrait
            // 
            this.pbPortrait.Location = new System.Drawing.Point(7, 118);
            this.pbPortrait.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pbPortrait.Name = "pbPortrait";
            this.pbPortrait.Size = new System.Drawing.Size(205, 203);
            this.pbPortrait.TabIndex = 0;
            this.pbPortrait.TabStop = false;
            // 
            // lnkLblRemotePortrait
            // 
            this.lnkLblRemotePortrait.Location = new System.Drawing.Point(212, 350);
            this.lnkLblRemotePortrait.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lnkLblRemotePortrait.Name = "lnkLblRemotePortrait";
            this.lnkLblRemotePortrait.Size = new System.Drawing.Size(180, 120);
            this.lnkLblRemotePortrait.TabIndex = 0;
            this.lnkLblRemotePortrait.TabStop = true;
            this.lnkLblRemotePortrait.UseMnemonic = false;
            this.lnkLblRemotePortrait.Text = "";
            this.lnkLblRemotePortrait.LinkClicked += this.OnRemotePortraitLinkClicked;
            // 
            // pbPortraitOnline
            // 
            this.pbPortraitOnline.Location = new System.Drawing.Point(212, 118);
            this.pbPortraitOnline.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pbPortraitOnline.Name = "pbPortraitOnline";
            this.pbPortraitOnline.Size = new System.Drawing.Size(205, 203);
            this.pbPortraitOnline.TabIndex = 0;
            this.pbPortraitOnline.TabStop = false;
            // 
            // cbRank
            // 
            this.cbRank.FormattingEnabled = true;
            this.cbRank.Items.AddRange(new object[] {
            "None",
            "Baron",
            "Count",
            "Duke",
            "King",
            "Emperor"});
            this.cbRank.Location = new System.Drawing.Point(52, 53);
            this.cbRank.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbRank.Name = "cbRank";
            this.cbRank.Size = new System.Drawing.Size(158, 23);
            this.cbRank.TabIndex = 21;
            this.cbRank.SelectedIndexChanged += new System.EventHandler(this.onChangeRank);
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(7, 57);
            this.label20.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(36, 15);
            this.label20.TabIndex = 2;
            this.label20.Text = "Rank:";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabDNA);
            this.tabControl1.Controls.Add(this.tabProperties);
            this.tabControl1.ItemSize = new System.Drawing.Size(96, 20);
            this.tabControl1.Location = new System.Drawing.Point(7, 85);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(292, 418);
            this.tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl1.TabIndex = 3;
            // 
            // tabDNA
            // 
            this.tabDNA.Controls.Add(this.panelDNA);
            this.tabDNA.Location = new System.Drawing.Point(4, 24);
            this.tabDNA.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabDNA.Name = "tabDNA";
            this.tabDNA.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabDNA.Size = new System.Drawing.Size(284, 390);
            this.tabDNA.TabIndex = 0;
            this.tabDNA.Text = "DNA";
            this.tabDNA.UseVisualStyleBackColor = true;
            // 
            // panelDNA
            // 
            this.panelDNA.AutoScroll = true;
            this.panelDNA.Location = new System.Drawing.Point(4, 7);
            this.panelDNA.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panelDNA.Name = "panelDNA";
            this.panelDNA.Size = new System.Drawing.Size(275, 377);
            this.panelDNA.TabIndex = 1;
            // 
            // tabProperties
            // 
            this.tabProperties.Controls.Add(this.panelProperties);
            this.tabProperties.Location = new System.Drawing.Point(4, 24);
            this.tabProperties.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabProperties.Name = "tabProperties";
            this.tabProperties.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabProperties.Size = new System.Drawing.Size(284, 390);
            this.tabProperties.TabIndex = 1;
            this.tabProperties.Text = "Properties*";
            this.tabProperties.UseVisualStyleBackColor = true;
            // 
            // panelProperties
            // 
            this.panelProperties.AutoScroll = true;
            this.panelProperties.Location = new System.Drawing.Point(5, 7);
            this.panelProperties.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panelProperties.Name = "panelProperties";
            this.panelProperties.Size = new System.Drawing.Size(274, 375);
            this.panelProperties.TabIndex = 2;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cbCulturePortraitTypes);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label21);
            this.groupBox2.Controls.Add(this.btnRandom);
            this.groupBox2.Controls.Add(this.btnImport);
            this.groupBox2.Controls.Add(this.cbPortraitTypes);
            this.groupBox2.Controls.Add(this.tabControl1);
            this.groupBox2.Location = new System.Drawing.Point(286, 12);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Size = new System.Drawing.Size(306, 543);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Input";
            // 
            // cbCulturePortraitTypes
            // 
            this.cbCulturePortraitTypes.DropDownHeight = 400;
            this.cbCulturePortraitTypes.FormattingEnabled = true;
            this.cbCulturePortraitTypes.IntegralHeight = false;
            this.cbCulturePortraitTypes.Location = new System.Drawing.Point(64, 53);
            this.cbCulturePortraitTypes.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbCulturePortraitTypes.Name = "cbCulturePortraitTypes";
            this.cbCulturePortraitTypes.Size = new System.Drawing.Size(234, 23);
            this.cbCulturePortraitTypes.TabIndex = 26;
            this.cbCulturePortraitTypes.SelectedIndexChanged += new System.EventHandler(this.onChangePortraitType);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 57);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 15);
            this.label1.TabIndex = 25;
            this.label1.Text = "Clothing:";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(8, 25);
            this.label21.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(49, 15);
            this.label21.TabIndex = 24;
            this.label21.Text = "Portrait:";
            // 
            // btnRandom
            // 
            this.btnRandom.Location = new System.Drawing.Point(211, 510);
            this.btnRandom.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnRandom.Name = "btnRandom";
            this.btnRandom.Size = new System.Drawing.Size(88, 27);
            this.btnRandom.TabIndex = 11;
            this.btnRandom.Text = "Randomize";
            this.btnRandom.UseVisualStyleBackColor = true;
            this.btnRandom.Click += new System.EventHandler(this.onClickRandomize);
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(7, 510);
            this.btnImport.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(88, 27);
            this.btnImport.TabIndex = 11;
            this.btnImport.Text = "Import";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.onClickImport);
            // 
            // cbPortraitTypes
            // 
            this.cbPortraitTypes.DropDownHeight = 400;
            this.cbPortraitTypes.FormattingEnabled = true;
            this.cbPortraitTypes.IntegralHeight = false;
            this.cbPortraitTypes.Location = new System.Drawing.Point(64, 22);
            this.cbPortraitTypes.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbPortraitTypes.Name = "cbPortraitTypes";
            this.cbPortraitTypes.Size = new System.Drawing.Size(234, 23);
            this.cbPortraitTypes.TabIndex = 2;
            this.cbPortraitTypes.SelectedIndexChanged += new System.EventHandler(this.onChangePortraitType);
            // 
            // tabContent
            // 
            this.tabContent.Controls.Add(this.tabDLCs);
            this.tabContent.Controls.Add(this.tabMods);
            this.tabContent.ItemSize = new System.Drawing.Size(96, 20);
            this.tabContent.Location = new System.Drawing.Point(7, 20);
            this.tabContent.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabContent.Name = "tabContent";
            this.tabContent.SelectedIndex = 0;
            this.tabContent.Size = new System.Drawing.Size(261, 483);
            this.tabContent.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabContent.TabIndex = 4;
            // 
            // tabDLCs
            // 
            this.tabDLCs.AutoScroll = true;
            this.tabDLCs.Controls.Add(this.panelDLCs);
            this.tabDLCs.Location = new System.Drawing.Point(4, 24);
            this.tabDLCs.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabDLCs.Name = "tabDLCs";
            this.tabDLCs.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabDLCs.Size = new System.Drawing.Size(253, 455);
            this.tabDLCs.TabIndex = 1;
            this.tabDLCs.Text = "DLCs";
            this.tabDLCs.UseVisualStyleBackColor = true;
            // 
            // panelDLCs
            // 
            this.panelDLCs.AutoScroll = true;
            this.panelDLCs.Location = new System.Drawing.Point(5, 3);
            this.panelDLCs.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panelDLCs.Name = "panelDLCs";
            this.panelDLCs.Size = new System.Drawing.Size(241, 444);
            this.panelDLCs.TabIndex = 0;
            // 
            // tabMods
            // 
            this.tabMods.AutoScroll = true;
            this.tabMods.Controls.Add(this.panelMods);
            this.tabMods.Location = new System.Drawing.Point(4, 24);
            this.tabMods.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabMods.Name = "tabMods";
            this.tabMods.Size = new System.Drawing.Size(253, 455);
            this.tabMods.TabIndex = 2;
            this.tabMods.Text = "Mods";
            this.tabMods.UseVisualStyleBackColor = true;
            // 
            // panelMods
            // 
            this.panelMods.AutoScroll = true;
            this.panelMods.Location = new System.Drawing.Point(6, 3);
            this.panelMods.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panelMods.Name = "panelMods";
            this.panelMods.Size = new System.Drawing.Size(243, 447);
            this.panelMods.TabIndex = 1;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnToogleAll);
            this.groupBox3.Controls.Add(this.btnReload);
            this.groupBox3.Controls.Add(this.tabContent);
            this.groupBox3.Location = new System.Drawing.Point(5, 12);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox3.Size = new System.Drawing.Size(276, 543);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Content";
            // 
            // btnToogleAll
            // 
            this.btnToogleAll.Location = new System.Drawing.Point(7, 510);
            this.btnToogleAll.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnToogleAll.Name = "btnToogleAll";
            this.btnToogleAll.Size = new System.Drawing.Size(88, 27);
            this.btnToogleAll.TabIndex = 6;
            this.btnToogleAll.Text = "Toggle All";
            this.btnToogleAll.UseVisualStyleBackColor = true;
            this.btnToogleAll.Click += new System.EventHandler(this.onClickToogleAll);
            // 
            // btnReload
            // 
            this.btnReload.Location = new System.Drawing.Point(176, 510);
            this.btnReload.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(88, 27);
            this.btnReload.TabIndex = 5;
            this.btnReload.Text = "Reload";
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.onClickReload);
            // 
            // PortraitBuilderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1045, 562);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.Name = "PortraitBuilderForm";
            this.Text = "Portrait Builder";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPortrait)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbPortraitOnline)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabDNA.ResumeLayout(false);
            this.tabProperties.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabContent.ResumeLayout(false);
            this.tabDLCs.ResumeLayout(false);
            this.tabMods.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.PictureBox pbPortrait;
        private System.Windows.Forms.PictureBox pbPortraitOnline;
        private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabProperties;
		private System.Windows.Forms.TextBox tbDNA;
		private System.Windows.Forms.ComboBox cbRank;
		private System.Windows.Forms.Label label20;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Button btnCopy;
		private System.Windows.Forms.Button btnImport;
		private System.Windows.Forms.Button btnRandom;
		private System.Windows.Forms.ComboBox cbPortraitTypes;
		private System.Windows.Forms.TabControl tabContent;
		private System.Windows.Forms.TabPage tabMods;
		private System.Windows.Forms.TabPage tabDLCs;
		private System.Windows.Forms.FlowLayoutPanel panelDLCs;
		private System.Windows.Forms.Label label21;
		private System.Windows.Forms.FlowLayoutPanel panelMods;
		private System.Windows.Forms.Label label22;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Button btnToogleAll;
		private System.Windows.Forms.Button btnReload;
		private System.Windows.Forms.Label label24;
		private System.Windows.Forms.ComboBox cbGovernment;
		private System.Windows.Forms.TabPage tabDNA;
		private System.Windows.Forms.FlowLayoutPanel panelDNA;
		private System.Windows.Forms.FlowLayoutPanel panelProperties;
		private System.Windows.Forms.ComboBox cbCulturePortraitTypes;
		private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel lnkLblRemotePortrait;
    }
}

