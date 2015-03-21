using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace tinyPnger
{
    class Smallifyer
    {
        /// <summary>
        /// An array of file size suffixes.
        /// </summary>
        static private readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        /// <summary>
        /// Shrinks the selected files using tinyPng, storing the shrunk files in the output location.
        /// </summary>
        /// <param name="key">The the tinyPng API key to use.</param>
        /// <param name="input">The input file, files or directory to upload.</param>
        /// <param name="output">The directory to output processed files to.</param>
        /// <param name="logger">A LogHandler with which to perform logging.</param>
        /// <param name="progress">A ProgressHandler with which to update the progress.</param>
        /// <param name="running">A StateToggleable to enable the caller to be informed when the process starts and stops.</param>
        public void smallifyFiles(string key, string input, string output, LogHandler logger, ProgressHandler progress, StateToggleable running)
        {
            running.ToggleState();
            WebClient client = new WebClient();
            string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes("api:" + key));
            client.Headers.Add(HttpRequestHeader.Authorization, "Basic " + auth);

            List<string> Errors = new List<string>();
            long totalSaved = 0;
            double totalSavedPercent = 0;
            int filesProcessed = 0;

            if (key != "" && input != "" && output != "")
            {
                try
                {
                    string[] files = getFilesFromInputString(input);

                    progress.ResetProgress();
                    progress.SetProgressMax(files.Length);

                    for (int i = 0; i < files.Length; i++)
                    {
                        string file = files[i];
                        if (File.Exists(file))
                        {
                            string fileExt = Path.GetExtension(file).ToLower();
                            if (fileExt == ".png" || fileExt == ".jpg" || fileExt == ".jpeg")
                            {
                                string inputFileName = Path.GetFileName(file);
                                logger.LogLine("Processing file: " + inputFileName);
                                string outputFilePath = Path.Combine(output, inputFileName);

                                if ((outputFilePath = downloadFile(client, file, outputFilePath)) == "")
                                {
                                    logger.LogLine("Error processing file: " + inputFileName);
                                    Errors.Add(inputFileName);
                                }
                                else
                                {
                                    filesProcessed++;
                                    long sizeBefore = new FileInfo(file).Length;
                                    long sizeAfter = new FileInfo(outputFilePath).Length;
                                    long saved = sizeBefore - sizeAfter;
                                    double savedPercent = ((double)saved / (double)sizeBefore) * 100f;
                                    totalSavedPercent += savedPercent;
                                    totalSaved += saved;
                                    logger.LogLine(string.Format("\tSize before: {0}", SizeSuffix(sizeBefore)));
                                    logger.LogLine(string.Format("\tSize After: {0}", SizeSuffix(sizeAfter)));
                                    logger.LogLine(string.Format("\tSaved: {0} or {1}%", SizeSuffix(saved), savedPercent.ToString("0.00")));
                                }
                            }
                        }

                        progress.SetProgress(i + 1);
                    }
                    if (Errors.Count == 0)
                    {
                        logger.LogLine("All done!");
                    }
                    else
                    {
                        string errorMessage = "Done, but there were errors processing the following files:";
                        foreach (string error in Errors)
                        {
                            errorMessage += System.Environment.NewLine + error;
                        }

                        logger.LogLine(errorMessage);
                    }

                    logger.LogLine(string.Format("Total Saved: {0} or {1}%",
                                                    SizeSuffix(totalSaved),
                                                    (totalSavedPercent / (double)filesProcessed).ToString("0.00")));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    running.ToggleState();
                }
            }
            else
            {
                MessageBox.Show("Please fill in all of the fields");
            }
        }

        /// <summary>
        /// Splits a ";" seperated string into an array of file paths.
        /// If there was only one path and it was a directory then it returns a list of the files in that directory.
        /// </summary>
        /// <param name="input">A ";" seperated string of paths</param>
        /// <returns>An array containing file paths</returns>
        private string[] getFilesFromInputString(string input)
        {
            string[] files = input.Split(';');

            if (files.Length == 1)
            {
                FileAttributes attr = File.GetAttributes(input);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    files = Directory.GetFiles(input);
                }
            }

            return files;
        }

        /// <summary>
        /// Uploads a file to tinyPng and downloads the processed file.
        /// </summary>
        /// <param name="client">The web client to use for the upload/download.</param>
        /// <param name="inputFile">The file to upload.</param>
        /// <param name="outputFile">The full path to download the processed file to.</param>
        /// <returns></returns>
        private string downloadFile(WebClient client, string inputFile, string outputFile)
        {
            string url = "https://api.tinypng.com/shrink";
            try
            {
                if (File.Exists(outputFile))
                {
                    string outputFileNameWithoutExt = Path.GetFileNameWithoutExtension(outputFile);
                    string outputFileName = outputFileNameWithoutExt + "1" + Path.GetExtension(outputFile);
                    outputFile = Path.Combine(outputFile.Replace(Path.GetFileName(outputFile), ""), outputFileName);
                }
                client.UploadData(url, File.ReadAllBytes(inputFile));
                //Compression was successful, retrieve output from Location header.
                client.DownloadFile(client.ResponseHeaders["Location"], outputFile);
            }
            catch (WebException)
            {
                //Something went wrong! You can parse the JSON body for details.
                return "";
            }
            return outputFile;
        }

        /// <summary>
        /// Converts sizes in bytes into a human readable format.
        /// </summary>
        /// <param name="value">A size in bytes</param>
        /// <returns>The size in a human readable format.</returns>
        static string SizeSuffix(Int64 value)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return "0.0 bytes"; }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
        }
    }
}
