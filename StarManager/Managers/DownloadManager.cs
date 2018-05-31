using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    class DownloadManager : CachedManager
    {
        GitHubClient client;
        Task<IReadOnlyList<RepositoryContent>> task;
        string path;
        bool isDataAcquired;

        public DownloadManager(string path)
        {
            client = new GitHubClient(new ProductHeaderValue("star-display"));
            task = client.Repository.Content.GetAllContents("StarDisplayLayouts", "layouts", path);
            this.path = path;
            isDataAcquired = false;
        }

        public bool IsCompleted()
        {
            return task.IsCompleted && !task.IsFaulted;
        }

        public override bool CheckInvalidated()
        {
            return !isDataAcquired && IsCompleted();
        }


        public bool GetData()
        {
            try
            {
                RepositoryContent content = task.Result.First();
                byte[] data = Convert.FromBase64String(content.EncodedContent);

                File.WriteAllBytes("layout/" + path, data);
            }
            catch (Exception)
            {
                isDataAcquired = true;
                return false;
            }
            isDataAcquired = true;
            return true;
        }
    }
}
