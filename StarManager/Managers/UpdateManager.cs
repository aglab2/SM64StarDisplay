using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    class UpdateManager
    {
        GitHubClient client;
        Task<GitHubCommit> task;
        string lastUpdate;

        public UpdateManager()
        {
            client = new GitHubClient(new ProductHeaderValue("star-display"));
            task = client.Repository.Commit.Get("aglab2", "SM64StarManager", "HEAD");
            lastUpdate = File.ReadLines("updateinfo.cfg").First();
        }

        public bool IsCompleted()
        {
            return task.IsCompleted && !task.IsFaulted;
        }

        public bool IsUpdated()
        {
            var commit = task.Result;
            return commit.Commit.Message == lastUpdate;
        }

        public string UpdateName()
        {
            return task.Result.Commit.Message;
        }

        public void WritebackUpdate()
        {
            File.WriteAllText("updateinfo.cfg", task.Result.Commit.Message);
        }
    }
}
