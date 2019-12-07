using Microsoft.Win32;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StarDisplay
{
    class UpdateManager
    {
        GitHubClient client;
        Task<IReadOnlyList<Release>> task;
        Version version;

        public UpdateManager()
        {
            version = Assembly.GetEntryAssembly().GetName().Version;
            using (RegistryKey softwareKey = Registry.CurrentUser.CreateSubKey("Software"),
                   sdKey = softwareKey.CreateSubKey("StarDisplay"))
            {
                try
                {
                    Version registryVersion = new Version((string)sdKey.GetValue("updatecfg"));
                    if (registryVersion != null && (registryVersion > version))
                        version = registryVersion;
                }
                catch(Exception) { }
            }

            client = new GitHubClient(new ProductHeaderValue("star-display"));
            task = client.Repository.Release.GetAll("aglab2", "SM64StarManager");
        }

        public bool IsCompleted()
        {
            return task.IsCompleted && !task.IsFaulted;
        }

        public bool IsUpdated()
        {
            Version updVersion = UpdateVersion();
            return updVersion <= version;
        }

        public string UpdateName()
        {
            return task.Result[0].Body;
        }

        public Version UpdateVersion()
        {
            return new Version(task.Result[0].TagName);
        }

        public void WritebackUpdate()
        {
            using (RegistryKey softwareKey = Registry.CurrentUser.CreateSubKey("Software"),
                   sdKey = softwareKey.CreateSubKey("StarDisplay"))
            {
                sdKey.SetValue("updatecfg", UpdateVersion());
            }
        }
    }
}
