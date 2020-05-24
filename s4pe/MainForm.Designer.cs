﻿/***************************************************************************
 *  Copyright (C) 2009, 2010 by Peter L Jones                              *
 *  pljones@users.sf.net                                                   *
 *                                                                         *
 *  This file is part of the Sims 3 Package Interface (s3pi)               *
 *                                                                         *
 *  s3pi is free software: you can redistribute it and/or modify           *
 *  it under the terms of the GNU General Public License as published by   *
 *  the Free Software Foundation, either version 3 of the License, or      *
 *  (at your option) any later version.                                    *
 *                                                                         *
 *  s3pi is distributed in the hope that it will be useful,                *
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *  GNU General Public License for more details.                           *
 *                                                                         *
 *  You should have received a copy of the GNU General Public License      *
 *  along with s3pi.  If not, see <http://www.gnu.org/licenses/>.          *
 ***************************************************************************/

namespace S4PIDemoFE
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lbProgress = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.button3 = new System.Windows.Forms.Button();
            this.btn_translate = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.cbo_exp_cloth = new System.Windows.Forms.CheckBox();
            this.cbo_r_rpmod = new System.Windows.Forms.CheckBox();
            this.lable1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.cbo_rm_ablity = new System.Windows.Forms.CheckBox();
            this.pnAuto = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.saveAsFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.exportFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.exportBatchTarget = new System.Windows.Forms.FolderBrowserDialog();
            this.importResourcesDialog = new System.Windows.Forms.OpenFileDialog();
            this.importPackagesDialog = new System.Windows.Forms.OpenFileDialog();
            this.exportToPackageDialog = new System.Windows.Forms.OpenFileDialog();
            this.replaceResourceDialog = new System.Windows.Forms.OpenFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.browserWidget1 = new S4PIDemoFE.BrowserWidget();
            this.packageInfoWidget1 = new S4PIDemoFE.PackageInfo.PackageInfoWidget();
            this.controlPanel1 = new S4PIDemoFE.ControlPanel();
            this.resourceFilterWidget1 = new S4PIDemoFE.Filter.ResourceFilterWidget();
            this.menuBarWidget = new S4PIDemoFE.MenuBarWidget();
            this.packageInfoFields1 = new S4PIDemoFE.PackageInfo.PackageInfoFields();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.pnAuto.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 23);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.packageInfoWidget1);
            this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Panel2MinSize = 131;
            this.splitContainer1.Size = new System.Drawing.Size(923, 654);
            this.splitContainer1.SplitterDistance = 514;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.browserWidget1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.panel3);
            this.splitContainer2.Panel2.Controls.Add(this.pnAuto);
            this.splitContainer2.Size = new System.Drawing.Size(923, 514);
            this.splitContainer2.SplitterDistance = 629;
            this.splitContainer2.TabIndex = 0;
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBar1.Location = new System.Drawing.Point(83, 0);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(0);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(836, 27);
            this.progressBar1.TabIndex = 2;
            // 
            // lbProgress
            // 
            this.lbProgress.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbProgress.AutoSize = true;
            this.lbProgress.Location = new System.Drawing.Point(0, 7);
            this.lbProgress.Margin = new System.Windows.Forms.Padding(0);
            this.lbProgress.Name = "lbProgress";
            this.lbProgress.Size = new System.Drawing.Size(83, 12);
            this.lbProgress.TabIndex = 3;
            this.lbProgress.Text = "Progress text";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.button3);
            this.panel3.Controls.Add(this.btn_translate);
            this.panel3.Controls.Add(this.button2);
            this.panel3.Controls.Add(this.cbo_exp_cloth);
            this.panel3.Controls.Add(this.cbo_r_rpmod);
            this.panel3.Controls.Add(this.lable1);
            this.panel3.Controls.Add(this.textBox1);
            this.panel3.Controls.Add(this.button1);
            this.panel3.Controls.Add(this.cbo_rm_ablity);
            this.panel3.Location = new System.Drawing.Point(0, 227);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(285, 258);
            this.panel3.TabIndex = 5;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(126, 3);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(65, 28);
            this.button3.TabIndex = 8;
            this.button3.Text = "选择";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // btn_translate
            // 
            this.btn_translate.Location = new System.Drawing.Point(6, 162);
            this.btn_translate.Name = "btn_translate";
            this.btn_translate.Size = new System.Drawing.Size(75, 23);
            this.btn_translate.TabIndex = 7;
            this.btn_translate.Text = "翻译";
            this.btn_translate.UseVisualStyleBackColor = true;
            this.btn_translate.Click += new System.EventHandler(this.btn_translate_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(155, 118);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(99, 28);
            this.button2.TabIndex = 6;
            this.button2.Text = "mod文件去重";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Button2_Click_1);
            // 
            // cbo_exp_cloth
            // 
            this.cbo_exp_cloth.AutoSize = true;
            this.cbo_exp_cloth.Location = new System.Drawing.Point(6, 96);
            this.cbo_exp_cloth.Name = "cbo_exp_cloth";
            this.cbo_exp_cloth.Size = new System.Drawing.Size(96, 16);
            this.cbo_exp_cloth.TabIndex = 5;
            this.cbo_exp_cloth.Text = "导出衣服信息";
            this.cbo_exp_cloth.UseVisualStyleBackColor = true;
            // 
            // cbo_r_rpmod
            // 
            this.cbo_r_rpmod.AutoSize = true;
            this.cbo_r_rpmod.Location = new System.Drawing.Point(126, 74);
            this.cbo_r_rpmod.Name = "cbo_r_rpmod";
            this.cbo_r_rpmod.Size = new System.Drawing.Size(90, 16);
            this.cbo_r_rpmod.TabIndex = 4;
            this.cbo_r_rpmod.Text = "移除替换mod";
            this.cbo_r_rpmod.UseVisualStyleBackColor = true;
            // 
            // lable1
            // 
            this.lable1.AutoSize = true;
            this.lable1.Location = new System.Drawing.Point(3, 11);
            this.lable1.Name = "lable1";
            this.lable1.Size = new System.Drawing.Size(89, 12);
            this.lable1.TabIndex = 3;
            this.lable1.Text = "模组目录路径：";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(6, 35);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(248, 21);
            this.textBox1.TabIndex = 2;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 118);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 28);
            this.button1.TabIndex = 0;
            this.button1.Text = "生成";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // cbo_rm_ablity
            // 
            this.cbo_rm_ablity.AutoSize = true;
            this.cbo_rm_ablity.Location = new System.Drawing.Point(6, 74);
            this.cbo_rm_ablity.Name = "cbo_rm_ablity";
            this.cbo_rm_ablity.Size = new System.Drawing.Size(90, 16);
            this.cbo_rm_ablity.TabIndex = 1;
            this.cbo_rm_ablity.Text = "移除功能mod";
            this.cbo_rm_ablity.UseVisualStyleBackColor = true;
            // 
            // pnAuto
            // 
            this.pnAuto.Controls.Add(this.panel2);
            this.pnAuto.Location = new System.Drawing.Point(0, 0);
            this.pnAuto.Margin = new System.Windows.Forms.Padding(0);
            this.pnAuto.Name = "pnAuto";
            this.pnAuto.Size = new System.Drawing.Size(288, 224);
            this.pnAuto.TabIndex = 4;
            // 
            // panel2
            // 
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(200, 100);
            this.panel2.TabIndex = 4;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.progressBar1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lbProgress, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 105);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(919, 27);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.controlPanel1);
            this.panel1.Controls.Add(this.resourceFilterWidget1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(919, 103);
            this.panel1.TabIndex = 0;
            // 
            // saveAsFileDialog
            // 
            this.saveAsFileDialog.FileName = "*.package";
            this.saveAsFileDialog.Filter = global::S4PIDemoFE.Properties.Settings.Default.DBPFFilesAndAll;
            this.saveAsFileDialog.Title = "Save As";
            // 
            // exportFileDialog
            // 
            this.exportFileDialog.AddExtension = false;
            this.exportFileDialog.Filter = "Exported files (S4_*.*)|S4_*.*|All files (*.*)|*.*";
            this.exportFileDialog.Title = "Export File";
            // 
            // exportBatchTarget
            // 
            this.exportBatchTarget.Description = "Choose the folder to receive the exported resources";
            // 
            // importResourcesDialog
            // 
            this.importResourcesDialog.Filter = "Exported files (S4_*.*)|S4_*.*|All files (*.*)|*.*";
            this.importResourcesDialog.Multiselect = true;
            this.importResourcesDialog.Title = "Import Resources";
            // 
            // importPackagesDialog
            // 
            this.importPackagesDialog.Filter = global::S4PIDemoFE.Properties.Settings.Default.DBPFFilesAndAll;
            this.importPackagesDialog.Multiselect = true;
            this.importPackagesDialog.Title = "Import Packages";
            // 
            // exportToPackageDialog
            // 
            this.exportToPackageDialog.CheckFileExists = false;
            this.exportToPackageDialog.DefaultExt = "package";
            this.exportToPackageDialog.Filter = global::S4PIDemoFE.Properties.Settings.Default.DBPFFilesAndAll;
            this.exportToPackageDialog.Title = "Export to package";
            // 
            // replaceResourceDialog
            // 
            this.replaceResourceDialog.AddExtension = false;
            this.replaceResourceDialog.Title = "Replace Resource";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "*.package";
            this.openFileDialog1.Filter = global::S4PIDemoFE.Properties.Settings.Default.DBPFFilesAndAll;
            this.openFileDialog1.SupportMultiDottedExtensions = true;
            this.openFileDialog1.Title = "Open package";
            // 
            // browserWidget1
            // 
            this.browserWidget1.AllowDrop = true;
            this.browserWidget1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browserWidget1.Fields = null;
            this.browserWidget1.Filter = null;
            this.browserWidget1.Location = new System.Drawing.Point(0, 0);
            this.browserWidget1.Name = "browserWidget1";
            this.browserWidget1.Package = null;
            this.browserWidget1.ProgressBar = this.progressBar1;
            this.browserWidget1.ProgressLabel = this.lbProgress;
            this.browserWidget1.ResourceKey = null;
            this.browserWidget1.SelectedResource = null;
            this.browserWidget1.Size = new System.Drawing.Size(625, 510);
            this.browserWidget1.Sortable = false;
            this.browserWidget1.TabIndex = 0;
            this.browserWidget1.ItemActivate += new System.EventHandler(this.browserWidget1_ItemActivate);
            this.browserWidget1.SelectedResourceChanging += new System.EventHandler<S4PIDemoFE.BrowserWidget.ResourceChangingEventArgs>(this.browserWidget1_SelectedResourceChanging);
            this.browserWidget1.SelectedResourceChanged += new System.EventHandler<S4PIDemoFE.BrowserWidget.ResourceChangedEventArgs>(this.browserWidget1_SelectedResourceChanged);
            this.browserWidget1.DeletePressed += new System.EventHandler(this.browserWidget1_DeletePressed);
            this.browserWidget1.DragDrop += new System.Windows.Forms.DragEventHandler(this.browserWidget1_DragDrop);
            this.browserWidget1.DragOver += new System.Windows.Forms.DragEventHandler(this.browserWidget1_DragOver);
            // 
            // packageInfoWidget1
            // 
            this.packageInfoWidget1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packageInfoWidget1.Fields = null;
            this.packageInfoWidget1.Location = new System.Drawing.Point(0, 103);
            this.packageInfoWidget1.Name = "packageInfoWidget1";
            this.packageInfoWidget1.Package = null;
            this.packageInfoWidget1.Size = new System.Drawing.Size(919, 2);
            this.packageInfoWidget1.TabIndex = 0;
            // 
            // controlPanel1
            // 
            this.controlPanel1.AutoOff = false;
            this.controlPanel1.AutoPreview = true;
            this.controlPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.controlPanel1.Location = new System.Drawing.Point(0, 74);
            this.controlPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.controlPanel1.Name = "controlPanel1";
            this.controlPanel1.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.controlPanel1.Size = new System.Drawing.Size(919, 29);
            this.controlPanel1.Sort = true;
            this.controlPanel1.TabIndex = 1;
            this.controlPanel1.UseNames = true;
            this.controlPanel1.UseTags = true;
            this.controlPanel1.SortChanged += new System.EventHandler(this.controlPanel1_SortChanged);
            this.controlPanel1.HexClick += new System.EventHandler(this.controlPanel1_HexClick);
            this.controlPanel1.AutoChanged += new System.EventHandler(this.controlPanel1_AutoChanged);
            this.controlPanel1.HexOnlyChanged += new System.EventHandler(this.controlPanel1_HexOnlyChanged);
            this.controlPanel1.ValueClick += new System.EventHandler(this.controlPanel1_PreviewClick);
            this.controlPanel1.GridClick += new System.EventHandler(this.controlPanel1_GridClick);
            this.controlPanel1.UseNamesChanged += new System.EventHandler(this.controlPanel1_UseNamesChanged);
            this.controlPanel1.UseTagsChanged += new System.EventHandler(this.controlPanel1_UseTagsChanged);
            this.controlPanel1.Helper1Click += new System.EventHandler(this.controlPanel1_Helper1Click);
            this.controlPanel1.Helper2Click += new System.EventHandler(this.controlPanel1_Helper2Click);
            this.controlPanel1.HexEditClick += new System.EventHandler(this.controlPanel1_HexEditClick);
            this.controlPanel1.CommitClick += new System.EventHandler(this.controlPanel1_CommitClick);
            // 
            // resourceFilterWidget1
            // 
            this.resourceFilterWidget1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resourceFilterWidget1.Location = new System.Drawing.Point(0, 0);
            this.resourceFilterWidget1.Name = "resourceFilterWidget1";
            this.resourceFilterWidget1.PasteButtonEnabled = true;
            this.resourceFilterWidget1.Size = new System.Drawing.Size(919, 103);
            this.resourceFilterWidget1.TabIndex = 0;
            this.resourceFilterWidget1.FilterChanged += new System.EventHandler(this.resourceFilterWidget1_FilterChanged);
            this.resourceFilterWidget1.PasteClicked += new System.EventHandler(this.resourceFilterWidget1_PasteClicked);
            // 
            // menuBarWidget
            // 
            this.menuBarWidget.Dock = System.Windows.Forms.DockStyle.Top;
            this.menuBarWidget.Location = new System.Drawing.Point(0, 0);
            this.menuBarWidget.Margin = new System.Windows.Forms.Padding(0);
            this.menuBarWidget.Name = "menuBarWidget";
            this.menuBarWidget.Size = new System.Drawing.Size(923, 23);
            this.menuBarWidget.TabIndex = 0;
            this.menuBarWidget.MBDropDownOpening += new S4PIDemoFE.MenuBarWidget.MBDropDownOpeningEventHandler(this.menuBarWidget1_MBDropDownOpening);
            this.menuBarWidget.MBFile_Click += new S4PIDemoFE.MenuBarWidget.MBClickEventHandler(this.menuBarWidget1_MBFile_Click);
            this.menuBarWidget.MBEdit_Click += new S4PIDemoFE.MenuBarWidget.MBClickEventHandler(this.menuBarWidget1_MBEdit_Click);
            this.menuBarWidget.MBResource_Click += new S4PIDemoFE.MenuBarWidget.MBClickEventHandler(this.menuBarWidget1_MBResource_Click);
            this.menuBarWidget.MBTools_Click += new S4PIDemoFE.MenuBarWidget.MBClickEventHandler(this.menuBarWidget1_MBTools_Click);
            this.menuBarWidget.MBSettings_Click += new S4PIDemoFE.MenuBarWidget.MBClickEventHandler(this.menuBarWidget1_MBSettings_Click);
            this.menuBarWidget.MBHelp_Click += new S4PIDemoFE.MenuBarWidget.MBClickEventHandler(this.menuBarWidget1_MBHelp_Click);
            this.menuBarWidget.HelperClick += new S4PIDemoFE.MenuBarWidget.HelperClickEventHandler(this.menuBarWidget1_HelperClick);
            this.menuBarWidget.MRUClick += new S4PIDemoFE.MenuBarWidget.MRUClickEventHandler(this.menuBarWidget1_MRUClick);
            this.menuBarWidget.BookmarkClick += new S4PIDemoFE.MenuBarWidget.BookmarkClickEventHandler(this.menuBarWidget1_BookmarkClick);
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(923, 677);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuBarWidget);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "s3pe";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.pnAuto.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveAsFileDialog;
        private System.Windows.Forms.SaveFileDialog exportFileDialog;
        private MenuBarWidget menuBarWidget;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private BrowserWidget browserWidget1;
        private PackageInfo.PackageInfoWidget packageInfoWidget1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private PackageInfo.PackageInfoFields packageInfoFields1;
        private System.Windows.Forms.Panel panel1;
        private Filter.ResourceFilterWidget resourceFilterWidget1;
        private ControlPanel controlPanel1;
        private System.Windows.Forms.FolderBrowserDialog exportBatchTarget;
        private System.Windows.Forms.OpenFileDialog importResourcesDialog;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lbProgress;
        private System.Windows.Forms.OpenFileDialog importPackagesDialog;
        private System.Windows.Forms.OpenFileDialog exportToPackageDialog;
        private System.Windows.Forms.Panel pnAuto;
        private System.Windows.Forms.OpenFileDialog replaceResourceDialog;
        private System.Windows.Forms.Label lable1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.CheckBox cbo_rm_ablity;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.CheckBox cbo_r_rpmod;
        private System.Windows.Forms.CheckBox cbo_exp_cloth;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btn_translate;
        private System.Windows.Forms.Button button3;
    }
}