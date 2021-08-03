using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Ookii.Dialogs.Wpf;
using System.Collections.Generic;

namespace MeltyInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string path;
        HttpClient client;

        const int bufferSize = 8192;

        Tuple<string, string> mbaaccInstall = new Tuple<string, string>("https://1g4i.short.gy/mbaacc", "mbaacc.zip");
        Tuple<string, string> cccasterInstall = new Tuple<string, string>("https://1g4i.short.gy/cccaster", "cccaster.zip");
        Tuple<string, string> concertoInstall = new Tuple<string, string>("https://github.com/shiburizu/concerto-mbaacc/releases/latest/download/Concerto.exe", "Concerto.exe");

        delegate void ProgressChangedHandler(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage);

        event ProgressChangedHandler ProgressChanged;

        public MainWindow()
        {
            InitializeComponent();
            SetPath("C:\\Games\\MBAACC");
        }

        private void selectPath_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog
            {
                UseDescriptionForTitle = true,
                Description = "Please select a folder." 
            };

            if ((bool) dialog.ShowDialog(this))
            {
                SetPath(dialog.SelectedPath);
            }
        }

        private void installPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetPath(installPath.Text);
        }

        private void installConcerto_Click(object sender, RoutedEventArgs e)
        {
            if (installConcerto.IsChecked.Value) 
            {
                installCCCaster.IsChecked = true;
                installCCCaster.IsEnabled = false;
            } 
            else
            {
                installCCCaster.IsEnabled = true;
            }
        }

        private void install_Click(object sender, RoutedEventArgs e)
        {
            Install();
        }

        private void SetPath(string newPath)
        {
            path = newPath;
            installPath.Text = newPath;
        }

        public void PrintLog(string log)
        {
            installLog.Text += log + "\n";
            scrollLog.ScrollToEnd();
        }

        private async void Install()
        {
            install.IsEnabled = false;
            installCCCaster.IsEnabled = false;
            installConcerto.IsEnabled = false;

            PrintLog("Creating Directory...");
            Directory.CreateDirectory(path);

            progressBar.Value += 5;

            client = new HttpClient();

            List<Tuple<string, string>> installInformation = new List<Tuple<string, string>> { mbaaccInstall };

            if(installCCCaster.IsChecked.Value)
            {
                installInformation.Add(cccasterInstall);
            };

            if(installConcerto.IsChecked.Value)
            {
                installInformation.Add(concertoInstall);
            };

            PrintLog("Downloading files... (This might take a while)");

            await Task.WhenAll(installInformation.Select(info => DownloadFile(info.Item1, info.Item2)));

            PrintLog("Finished downloading files!");

            client.Dispose();

            PrintLog("Unzipping archives...");

            await Task.WhenAll(installInformation.Select(info => UnzipFile(info.Item2)));

            PrintLog("Finished unzipping archives!");

            progressBar.Value = 100;

            PrintLog("---------------------");
            PrintLog("DONE!");
            close.Content = "Done";
        }

        // Modified code from https://www.tugberkugurlu.com/archive/efficiently-streaming-large-http-responses-with-httpclient
        // @abosma - TODO: Add progress bar updating with download reporting
        //                 Possible way maybe by using totalReads with a smaller modulo to add to progress bar, not yet sure.
        //                 Further changes needed is extra error checking if connection to server can't be made.
        private async Task DownloadFile(string urlAddress, string fileName)
        {
            Uri uri = new Uri(urlAddress);
            string completePath = Path.Join(path, fileName);

            bool fileExists = File.Exists(completePath);

            if(!fileExists)
            {
                using (HttpResponseMessage response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        PrintLog($"Downloading: {fileName}");

                        using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync(),
                            fileStream = new FileStream(completePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true))
                        {
                            // Progress report implementation modified from: https://github.com/dotnet/runtime/issues/16681#issuecomment-195980023
                            var totalRead = 0L;
                            var totalReads = 0L;
                            var totalSize = response.Content.Headers.ContentLength;
                            var buffer = new byte[bufferSize];
                            var isMoreToRead = true;

                            do
                            {
                                var read = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length);
                                if (read == 0)
                                {
                                    isMoreToRead = false;
                                }
                                else
                                {
                                    await fileStream.WriteAsync(buffer, 0, read);

                                    totalRead += read;
                                    totalReads += 1;

                                    if (totalReads % 512 == 0)
                                    {
                                        PrintLog($"{fileName} download progress: {totalRead / 1048576}mb of {totalSize / 1048576}mb");
                                    }
                                }
                            }
                            while (isMoreToRead);
                        }

                        progressBar.Value += 20;
                        PrintLog($"Finished Download of: {fileName}");
                    }
                    else
                    {
                        PrintLog($"Download failed for: {fileName}");
                    }
                }
            }
        }

        private Task UnzipFile(string fileName)
        {
            string completePath = Path.Join(path, fileName);

            if (!fileName.Contains(".zip"))
            {
                return Task.CompletedTask;
            }

            PrintLog($"Starting Unzip of: {fileName}");

            ZipFile.ExtractToDirectory(completePath, path, true);

            progressBar.Value += 10;

            PrintLog($"Cleaning up {fileName} archive...");

            File.Delete(completePath);

            progressBar.Value += 5;

            return Task.CompletedTask;
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }
    }
}
