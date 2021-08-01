using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Ookii.Dialogs.Wpf;

namespace MeltyInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WebClient webClient;
        const string mbaaccUrl = "https://1g4i.short.gy/mbaacc";
        const string cccasterUrl = "https://1g4i.short.gy/cccaster";
        const string concertoUrl = "https://github.com/shiburizu/concerto-mbaacc/releases/latest/download/Concerto.exe";
        string path;

        public MainWindow()
        {
            InitializeComponent();
            SetPath("C:\\Games\\MBAACC");
        }

        private void selectPath_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder.";
            dialog.UseDescriptionForTitle = true;
            if ((bool)dialog.ShowDialog(this))
                SetPath(dialog.SelectedPath);
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
            } else
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


            var filepath = Path.Join(path, "mbaacc.zip");

            await DownloadFile(mbaaccUrl, filepath);

            PrintLog("Extracting MBAACC...");
            ZipFile.ExtractToDirectory(filepath, path);

            PrintLog("Cleaning up MBAACC Archive...");
            try
            {
                File.Delete(filepath);
            }
            catch {
                PrintLog("Cleaning up MBAACC Archive failed...");
            }


            if (installCCCaster.IsChecked.Value)
            {
                filepath = Path.Join(path, "cccaster.zip");

                await DownloadFile(cccasterUrl, filepath);

                PrintLog("Extracting CCCaster...");
                ZipFile.ExtractToDirectory(filepath, path);

                PrintLog("Cleaning up CCCaster Archive...");
                try
                {
                    File.Delete(filepath);
                }
                catch
                {
                    PrintLog("Cleaning up CCCaster Archive failed...");
                }
            }


            if (installConcerto.IsChecked.Value)
            {
                filepath = Path.Join(path, "Concerto.exe");

                await DownloadFile(concertoUrl, filepath);
            }

            PrintLog("DONE!");
        }

        private async Task DownloadFile(string urlAddress, string filepath)
        {
            using (webClient = new WebClient())
            {
                webClient.DownloadFileCompleted += (s, e) => PrintLog("Download file completed.");
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                Uri URL = new Uri(urlAddress);

                try
                {
                    PrintLog("Starting Download of: " + Path.GetFileName(filepath));
                    await webClient.DownloadFileTaskAsync(URL, filepath);
                }
                catch (Exception ex)
                {
                    PrintLog("Download failed: " + ex.Message);
                }
            }
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;

            PrintLog(string.Format("{0} MB's / {1} MB's",
                (e.BytesReceived / 1024d / 1024d).ToString("0.00"),
                (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00")));
        }
    }
}
