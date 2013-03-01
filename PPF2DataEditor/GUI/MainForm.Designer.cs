namespace PPF2DataEditor
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
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("The file system will appear here once you open an ISO.");
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.rebuildDATABINQuickToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rebuildDATABINFullToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.executableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.archiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createMRGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createMRZToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createSPKToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createTEXToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createTEZToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.texturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sVRPNGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.encodeToSVRToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filesystem = new System.Windows.Forms.TreeView();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.executableToolStripMenuItem,
            this.archiveToolStripMenuItem,
            this.texturesToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(584, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.exportAllToolStripMenuItem,
            this.toolStripSeparator1,
            this.rebuildDATABINQuickToolStripMenuItem,
            this.rebuildDATABINFullToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.openToolStripMenuItem.Text = "Open ISO";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // exportAllToolStripMenuItem
            // 
            this.exportAllToolStripMenuItem.Enabled = false;
            this.exportAllToolStripMenuItem.Name = "exportAllToolStripMenuItem";
            this.exportAllToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.exportAllToolStripMenuItem.Text = "Export All";
            this.exportAllToolStripMenuItem.Click += new System.EventHandler(this.exportAllToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(153, 6);
            // 
            // rebuildDATABINQuickToolStripMenuItem
            // 
            this.rebuildDATABINQuickToolStripMenuItem.Enabled = false;
            this.rebuildDATABINQuickToolStripMenuItem.Name = "rebuildDATABINQuickToolStripMenuItem";
            this.rebuildDATABINQuickToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.rebuildDATABINQuickToolStripMenuItem.Text = "Rebuild (Quick)";
            this.rebuildDATABINQuickToolStripMenuItem.Click += new System.EventHandler(this.rebuildDATABINQuickToolStripMenuItem_Click);
            // 
            // rebuildDATABINFullToolStripMenuItem
            // 
            this.rebuildDATABINFullToolStripMenuItem.Enabled = false;
            this.rebuildDATABINFullToolStripMenuItem.Name = "rebuildDATABINFullToolStripMenuItem";
            this.rebuildDATABINFullToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.rebuildDATABINFullToolStripMenuItem.Text = "Rebuild (Full)";
            this.rebuildDATABINFullToolStripMenuItem.Click += new System.EventHandler(this.rebuildDATABINFullToolStripMenuItem_Click);
            // 
            // executableToolStripMenuItem
            // 
            this.executableToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importToolStripMenuItem,
            this.exportToolStripMenuItem});
            this.executableToolStripMenuItem.Enabled = false;
            this.executableToolStripMenuItem.Name = "executableToolStripMenuItem";
            this.executableToolStripMenuItem.Size = new System.Drawing.Size(75, 20);
            this.executableToolStripMenuItem.Text = "Executable";
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.importToolStripMenuItem.Text = "Import";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.importToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exportToolStripMenuItem.Text = "Export";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // archiveToolStripMenuItem
            // 
            this.archiveToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractToolStripMenuItem,
            this.createMRGToolStripMenuItem,
            this.createMRZToolStripMenuItem,
            this.createSPKToolStripMenuItem,
            this.createTEXToolStripMenuItem,
            this.createTEZToolStripMenuItem});
            this.archiveToolStripMenuItem.Enabled = false;
            this.archiveToolStripMenuItem.Name = "archiveToolStripMenuItem";
            this.archiveToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
            this.archiveToolStripMenuItem.Text = "Archive";
            // 
            // extractToolStripMenuItem
            // 
            this.extractToolStripMenuItem.Name = "extractToolStripMenuItem";
            this.extractToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.extractToolStripMenuItem.Text = "Extract";
            // 
            // createMRGToolStripMenuItem
            // 
            this.createMRGToolStripMenuItem.Name = "createMRGToolStripMenuItem";
            this.createMRGToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.createMRGToolStripMenuItem.Text = "Create MRG";
            // 
            // createMRZToolStripMenuItem
            // 
            this.createMRZToolStripMenuItem.Name = "createMRZToolStripMenuItem";
            this.createMRZToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.createMRZToolStripMenuItem.Text = "Create MRZ";
            // 
            // createSPKToolStripMenuItem
            // 
            this.createSPKToolStripMenuItem.Name = "createSPKToolStripMenuItem";
            this.createSPKToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.createSPKToolStripMenuItem.Text = "Create SPK";
            // 
            // createTEXToolStripMenuItem
            // 
            this.createTEXToolStripMenuItem.Name = "createTEXToolStripMenuItem";
            this.createTEXToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.createTEXToolStripMenuItem.Text = "Create TEX";
            // 
            // createTEZToolStripMenuItem
            // 
            this.createTEZToolStripMenuItem.Name = "createTEZToolStripMenuItem";
            this.createTEZToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.createTEZToolStripMenuItem.Text = "Create TEZ";
            // 
            // texturesToolStripMenuItem
            // 
            this.texturesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sVRPNGToolStripMenuItem,
            this.encodeToSVRToolStripMenuItem});
            this.texturesToolStripMenuItem.Enabled = false;
            this.texturesToolStripMenuItem.Name = "texturesToolStripMenuItem";
            this.texturesToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
            this.texturesToolStripMenuItem.Text = "Textures";
            // 
            // sVRPNGToolStripMenuItem
            // 
            this.sVRPNGToolStripMenuItem.Name = "sVRPNGToolStripMenuItem";
            this.sVRPNGToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.sVRPNGToolStripMenuItem.Text = "Decode to PNG";
            // 
            // encodeToSVRToolStripMenuItem
            // 
            this.encodeToSVRToolStripMenuItem.Name = "encodeToSVRToolStripMenuItem";
            this.encodeToSVRToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.encodeToSVRToolStripMenuItem.Text = "Encode to SVR";
            // 
            // filesystem
            // 
            this.filesystem.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filesystem.Enabled = false;
            this.filesystem.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.filesystem.Location = new System.Drawing.Point(13, 28);
            this.filesystem.Name = "filesystem";
            treeNode3.Name = "Node0";
            treeNode3.Text = "The file system will appear here once you open an ISO.";
            this.filesystem.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode3});
            this.filesystem.Size = new System.Drawing.Size(559, 322);
            this.filesystem.TabIndex = 1;
            this.filesystem.MouseUp += new System.Windows.Forms.MouseEventHandler(this.filesystem_MouseUp);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 362);
            this.Controls.Add(this.filesystem);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "PPF2 Data Editor";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem executableToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem archiveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createMRZToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createTEZToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createMRGToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createTEXToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createSPKToolStripMenuItem;
        private System.Windows.Forms.TreeView filesystem;
        private System.Windows.Forms.ToolStripMenuItem texturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sVRPNGToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem encodeToSVRToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem rebuildDATABINQuickToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rebuildDATABINFullToolStripMenuItem;
    }
}