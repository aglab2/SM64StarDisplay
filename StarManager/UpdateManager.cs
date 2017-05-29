using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    class UpdateManager
    {
        GitHubClient client;
        Task<GitHubCommit> task;

        public UpdateManager()
        {
            client = new GitHubClient(new ProductHeaderValue("star-display"));
            task = client.Repository.Commit.Get("aglab2", "SM64StarManager", "HEAD");
        }

        public bool IsCompleted()
        {
            return task.IsCompleted;
        }

        public bool IsUpdated()
        {
            var commit = task.Result;
            return commit.Commit.Message == "Minor updates to engine, added updates checker";
        }
    }
}
