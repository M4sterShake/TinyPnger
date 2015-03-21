using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using dnGREP;
using System.Windows.Shell;

namespace tinyPnger
{
    public partial class MainForm : Form, LogHandler, ProgressHandler, StateToggleable
    {
        /// <summary>
        /// MainForm constructor.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// MainForm Load event handler.
        /// </summary>
        /// <param name="sender">The sending object.</param>
        /// <param name="e">Load event arguments.</param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            txtApiKey.Text = ConfigurationManager.AppSettings.Get("ApiKey");
            txtInput.Text = ConfigurationManager.AppSettings.Get("InputPath");
            txtOutput.Text = ConfigurationManager.AppSettings.Get("OutputPath");
        }

        /// <summary>
        /// Start button clicked event handler.
        /// </summary>
        /// <param name="sender">The sending object.</param>
        /// <param name="e">Load event arguments.</param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            string key = txtApiKey.Text;
            string input = txtInput.Text;
            string output = txtOutput.Text;
            txtLog.Clear();
            Smallifyer smallifyer = new Smallifyer();
            Thread smallifyThread = new Thread(() => smallifyer.smallifyFiles(key, input, output, (LogHandler)this, (ProgressHandler)this, (StateToggleable)this));
            smallifyThread.IsBackground = true;
            smallifyThread.Start();
        }

        /// <summary>
        /// Input browse button clicked event handler.
        /// </summary>
        /// <param name="sender">The sending object.</param>
        /// <param name="e">Load event arguments.</param>
        private void btnInputBrowse_Click(object sender, EventArgs e)
        {
            string newPath = browseForFileOrFolder();
            if (newPath != "")
            {
                txtInput.Text = newPath;
            }
        }

        /// <summary>
        /// Output browse button event handler.
        /// </summary>
        /// <param name="sender">The sending object.</param>
        /// <param name="e">Load event arguments.</param>
        private void btnOutputBrowse_Click(object sender, EventArgs e)
        {
            string newPath = browseForFolder();
            if (Directory.Exists(newPath))
            {
                txtOutput.Text = newPath;
            }
        }

        /// <summary>
        /// Creates a browse dialog that allows selecting of files or folders.
        /// </summary>
        /// <returns>The selected path from the browse dialog or an empty string if there was no valid selection</returns>
        private string browseForFileOrFolder()
        {
            String selectedPath = string.Empty;
            FileFolderDialog browseDialog = new FileFolderDialog();
            browseDialog.Dialog.Multiselect = true;

            DialogResult dialogResult = browseDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                if (browseDialog.SelectedPaths != null)
                {
                    selectedPath = browseDialog.SelectedPaths;
                }
                else
                {
                    if (Directory.Exists(browseDialog.SelectedPath) || File.Exists(browseDialog.SelectedPath))
                    {
                        selectedPath = browseDialog.SelectedPath;
                    }
                }
            }

            return selectedPath;
        }

        /// <summary>
        /// Creates a browse dialog that allows selecting of folders.
        /// </summary>
        /// <returns>The selected path from the browse dialog or an empty string if there was no valid selection</returns>
        private string browseForFolder()
        {
            String selectedPath = string.Empty;
            FolderBrowserDialog browseDialog = new FolderBrowserDialog();

            DialogResult dialogResult = browseDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                if (Directory.Exists(browseDialog.SelectedPath))
                {
                    selectedPath = browseDialog.SelectedPath;
                }
            }

            return selectedPath;
        }

        // LogHandler Implementation //

        /// <summary>
        /// Logs the specified line of text.
        /// </summary>
        /// <param name="line">The line of text to log.</param>
        public void LogLine(string line)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => txtLog.AppendText(line + System.Environment.NewLine)));
            }
        }

        /// <summary>
        /// Toggles the state between running and ready.
        /// </summary>
        public void ToggleState()
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => btnStart.Enabled = !btnStart.Enabled));
            }
        }


        // ProgressHandler Implimentation //

        /// <summary>
        /// Resets the progress to 0.
        /// </summary>
        public void ResetProgress()
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => progressBar.Minimum = 0));
                this.Invoke(new Action(() => progressBar.Maximum = 100));
                this.Invoke(new Action(() => progressBar.Value = 0));
            }
        }

        /// <summary>
        /// Sets the maximum possible progress value.
        /// </summary>
        /// <param name="max">The highest progress value possible.</param>
        public void SetProgressMax(int max)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => progressBar.Maximum = max));
            }
        }

        /// <summary>
        /// Sets the current progress.
        /// </summary>
        /// <param name="progress">The current progress.</param>
        public void SetProgress(int progress)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => progressBar.Value = progress));
            }
        }
    }
}
