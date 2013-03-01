using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using FolderSelect;

namespace PPF2DataEditor
{
    public partial class MainForm : Form
    {
        ISO gameISO;

        public MainForm()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select a PPF2 ISO";
            ofd.Filter = "ISO files (*.iso)|*.iso|All files (*.*)|*.*";
            ofd.CheckFileExists = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                // Check to see if this is a valid ISO
                if (new FileInfo(ofd.FileName).Length == 0x4AA58000)
                {
                    gameISO = new ISO(ofd.FileName);

                    // Add the inital directory
                    filesystem.SuspendLayout();
                    filesystem.Nodes.Clear();
                    AddDirectoryToTreeView(gameISO.FileSystem.Root, filesystem.Nodes);
                    filesystem.ResumeLayout();
                    filesystem.Enabled = true;
                    executableToolStripMenuItem.Enabled = true;
                    exportAllToolStripMenuItem.Enabled = true;
                    rebuildDATABINQuickToolStripMenuItem.Enabled = true;
                    rebuildDATABINFullToolStripMenuItem.Enabled = true;
                }
                else
                {
                    MessageBox.Show("This is not a Puyo Puyo Fever 2 PS2 ISO.",
                        "Not a valid ISO");
                }
            }
        }

        private void AddDirectoryToTreeView(FileSystem.DirectoryEntry directory, TreeNodeCollection nodes)
        {
            // Add the directories
            for (int i = 0; i < directory.Directories.Count; i++)
            {
                TreeNode node = new TreeNode();
                node.Tag = directory.Directories[i];
                node.Text = directory.Directories[i].Name;
                nodes.Add(node);

                AddDirectoryToTreeView(directory.Directories[i], node.Nodes);
            }

            // Add the files
            foreach (FileSystem.FileEntry file in directory.Files)
            {
                TreeNode node = new TreeNode();
                node.Tag = file;
                node.Text = file.Name;
                nodes.Add(node);
            }
        }

        private void filesystem_MouseUp(object sender, MouseEventArgs e)
        {
            ContextMenuStrip cm = new ContextMenuStrip();

            if (e.Button == MouseButtons.Right)
            {
                // Select the clicked node
                filesystem.SelectedNode = filesystem.GetNodeAt(e.X, e.Y);

                if (filesystem.SelectedNode != null)
                {
                    // Is a directory
                    if (filesystem.SelectedNode.Nodes.Count != 0)
                    {
                        FileSystem.DirectoryEntry entry = (FileSystem.DirectoryEntry)filesystem.SelectedNode.Tag;

                        ToolStripItem extract = new ToolStripMenuItem();
                        extract.Text = "Export directory";
                        extract.Click += delegate(object sender2, EventArgs e2)
                        {
                            FolderSelectDialog fsd = new FolderSelectDialog();
                            fsd.Title = "Export a directory";

                            if (fsd.ShowDialog())
                            {
                                ProgressDialog progress = new ProgressDialog();
                                progress.title.Text = "Exporting directory";
                                progress.action.Text = String.Empty;

                                BackgroundWorker bw = new BackgroundWorker();
                                bw.WorkerReportsProgress = true;
                                bw.DoWork += delegate(object sender3, DoWorkEventArgs e3)
                                {
                                    gameISO.ExportDirectory(entry, fsd.FileName, sender3 as BackgroundWorker);
                                };
                                bw.RunWorkerCompleted += delegate(object sender3, RunWorkerCompletedEventArgs e3)
                                {
                                    progress.Close();
                                    this.Enabled = true;
                                    this.Focus();
                                };
                                bw.ProgressChanged += delegate(object sender3, ProgressChangedEventArgs e3)
                                {
                                    progress.action.Text = (string)e3.UserState;
                                    progress.progressBar.Value = e3.ProgressPercentage;
                                };

                                this.Enabled = false;
                                progress.Show();
                                bw.RunWorkerAsync();
                            }
                        };

                        cm.Items.Add(extract);
                    }
                    else // Is a file
                    {
                        FileSystem.FileEntry entry = (FileSystem.FileEntry)filesystem.SelectedNode.Tag;
                        bool isArchive = entry.Name.ToLower().EndsWith(".mrg") || entry.Name.ToLower().EndsWith(".mrz") ||
                            entry.Name.ToLower().EndsWith(".tex") || entry.Name.ToLower().EndsWith(".tez") ||
                            entry.Name.ToLower().EndsWith(".spk");

                        ToolStripItem extract = new ToolStripMenuItem();
                        extract.Text = "Export";
                        extract.Click += delegate(object sender2, EventArgs e2)
                        {
                            SaveFileDialog sfd = new SaveFileDialog();
                            sfd.Title = "Export file";
                            sfd.FileName = entry.Name;
                            sfd.Filter = "All files (*.*)|*.*";
                            sfd.OverwritePrompt = true;
                            
                            if (sfd.ShowDialog() == DialogResult.OK)
                                gameISO.ExportFile(entry, Path.GetDirectoryName(sfd.FileName));
                        };
                        cm.Items.Add(extract);

                        if (isArchive)
                        {
                            ToolStripItem exportAndExtract = new ToolStripMenuItem();
                            exportAndExtract.Text = "Export and extract archive";
                            exportAndExtract.Click += delegate(object sender2, EventArgs e2)
                            {
                                SaveFileDialog sfd = new SaveFileDialog();
                                sfd.Title = "Export and extract archive";
                                sfd.FileName = entry.Name;
                                sfd.Filter = "All files (*.*)|*.*";
                                sfd.OverwritePrompt = true;

                                if (sfd.ShowDialog() == DialogResult.OK)
                                    gameISO.ExportAndExtractFile(entry, Path.GetDirectoryName(sfd.FileName));
                            };
                            cm.Items.Add(exportAndExtract);

                            cm.Items.Add(new ToolStripSeparator());
                        }

                        ToolStripItem import = new ToolStripMenuItem();
                        import.Text = "Import";
                        import.Click += delegate(object sender2, EventArgs e2)
                        {
                            OpenFileDialog ofd = new OpenFileDialog();
                            ofd.Title = "Import file";
                            ofd.Filter = "All files (*.*)|*.*";
                            ofd.CheckFileExists = true;

                            if (ofd.ShowDialog() == DialogResult.OK)
                                gameISO.ImportFile(entry, ofd.FileName);
                        };
                        cm.Items.Add(import);

                        if (isArchive)
                        {
                            ToolStripItem importFromDirectory = new ToolStripMenuItem();
                            importFromDirectory.Text = "Import from loose files";
                            importFromDirectory.Click += delegate(object sender2, EventArgs e2)
                            {
                                FolderSelectDialog fsd = new FolderSelectDialog();
                                fsd.Title = "Select the directory containing the files for the archive.";

                                if (fsd.ShowDialog())
                                    gameISO.ImportFileFromDirectory(entry, fsd.FileName);
                            };
                            cm.Items.Add(importFromDirectory);
                        }
                    }

                    cm.Show(filesystem, e.Location);
                }
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Export the PPF2 executable";
            sfd.FileName = "SLPM_661.04";
            sfd.Filter = "All files (*.*)|*.*";
            sfd.OverwritePrompt = true;

            if (sfd.ShowDialog() == DialogResult.OK)
                gameISO.ExportExecutable(sfd.FileName);
        }

        private void exportAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderSelectDialog fsd = new FolderSelectDialog();
            fsd.Title = "Export all files";

            if (fsd.ShowDialog())
            {
                ProgressDialog progress = new ProgressDialog();
                progress.title.Text = "Exporting all files";
                progress.action.Text = String.Empty;

                BackgroundWorker bw = new BackgroundWorker();
                bw.WorkerReportsProgress = true;
                bw.DoWork += delegate(object sender2, DoWorkEventArgs e2)
                {
                    gameISO.ExportDirectory(gameISO.FileSystem.Root, fsd.FileName, sender2 as BackgroundWorker);
                };
                bw.RunWorkerCompleted += delegate(object sender2, RunWorkerCompletedEventArgs e2)
                {
                    progress.Close();
                    this.Enabled = true;
                    this.Focus();
                };
                bw.ProgressChanged += delegate(object sender2, ProgressChangedEventArgs e2)
                {
                    progress.action.Text = (string)e2.UserState;
                    progress.progressBar.Value = e2.ProgressPercentage;
                };

                this.Enabled = false;
                progress.Show();
                bw.RunWorkerAsync();
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Import the PPF2 executable (usually SLPM_661.04)";
            ofd.Filter = "All files (*.*)|*.*";
            ofd.CheckFileExists = true;

            if (ofd.ShowDialog() == DialogResult.OK)
                gameISO.ImportExecutable(ofd.FileName);
        }

        private void rebuildDATABINQuickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderSelectDialog fsd = new FolderSelectDialog();
            fsd.Title = "Rebuild ISO (Quick)";

            if (fsd.ShowDialog())
            {
                ProgressDialog progress = new ProgressDialog();
                progress.title.Text = "Rebuilding ISO";
                progress.action.Text = String.Empty;

                BackgroundWorker bw = new BackgroundWorker();
                bw.WorkerReportsProgress = true;
                bw.DoWork += delegate(object sender2, DoWorkEventArgs e2)
                {
                    gameISO.Rebuild(fsd.FileName, true, sender2 as BackgroundWorker);
                };
                bw.RunWorkerCompleted += delegate(object sender2, RunWorkerCompletedEventArgs e2)
                {
                    progress.Close();
                    this.Enabled = true;
                    this.Focus();
                };
                bw.ProgressChanged += delegate(object sender2, ProgressChangedEventArgs e2)
                {
                    progress.action.Text = (string)e2.UserState;
                    progress.progressBar.Value = e2.ProgressPercentage;
                };

                this.Enabled = false;
                progress.Show();
                bw.RunWorkerAsync();
            }
        }

        private void rebuildDATABINFullToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderSelectDialog fsd = new FolderSelectDialog();
            fsd.Title = "Rebuild ISO (Full)";

            if (fsd.ShowDialog())
            {
                ProgressDialog progress = new ProgressDialog();
                progress.title.Text = "Rebuilding ISO";
                progress.action.Text = String.Empty;

                BackgroundWorker bw = new BackgroundWorker();
                bw.WorkerReportsProgress = true;
                bw.DoWork += delegate(object sender2, DoWorkEventArgs e2)
                {
                    gameISO.Rebuild(fsd.FileName, false, sender2 as BackgroundWorker);
                };
                bw.RunWorkerCompleted += delegate(object sender2, RunWorkerCompletedEventArgs e2)
                {
                    progress.Close();
                    this.Enabled = true;
                    this.Focus();
                };
                bw.ProgressChanged += delegate(object sender2, ProgressChangedEventArgs e2)
                {
                    progress.action.Text = (string)e2.UserState;
                    progress.progressBar.Value = e2.ProgressPercentage;
                };

                this.Enabled = false;
                progress.Show();
                bw.RunWorkerAsync();
            }
        }
    }
}