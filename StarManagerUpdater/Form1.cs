using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Octokit;

namespace StarManagerUpdater
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            client = new GitHubClient(new ProductHeaderValue("star-display"));
            versionTask = client.Repository.Release.GetAll("aglab2", "SM64StarManager", new ApiOptions() { PageSize = 5, PageCount = 1 });
            while(true)
            {
                Thread.Sleep(100);
                if (IsCompleted())
                    break;
            }

            Process.Start(DownloadPath());
        }

        GitHubClient client;
        Task<IReadOnlyList<Release>> versionTask;
        Task<IReadOnlyList<ReleaseAsset>> assetTask;

        static bool IsCompleted<T>(Task<T> task)
        {
            return task != null && task.IsCompleted && !task.IsFaulted;
        }

        public bool IsCompleted()
        {
            if (IsCompleted(assetTask))
                return true;

            if (IsCompleted(versionTask))
            {
                if (assetTask == null)
                {
                    assetTask = client.Repository.Release.GetAllAssets("aglab2", "SM64StarManager", versionTask.Result[0].Id);
                }
            }

            return false;
        }

        public string UpdateName()
        {
            return versionTask.Result[0].Body;
        }

        public string DownloadPath()
        {
            return assetTask.Result[0].BrowserDownloadUrl;
        }
    }
}
