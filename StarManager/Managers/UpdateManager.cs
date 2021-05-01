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
        Task<IReadOnlyList<Release>> versionTask;
        Version version;
        bool isUpdated;

        Task<IReadOnlyList<ReleaseAsset>> assetTask;

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
            versionTask = client.Repository.Release.GetAll("aglab2", "SM64StarDisplay", new ApiOptions() { PageSize = 5, PageCount = 1 });
            isUpdated = false;
        }

        static bool IsCompleted<T>(Task<T> task)
        {
            return task != null && task.IsCompleted && !task.IsFaulted;
        }

        public bool IsCompleted()
        {
            if (isUpdated)
                return true;

            if (IsCompleted(assetTask))
                return true;

            if (IsCompleted(versionTask))
            {
                if (assetTask == null)
                {
                    isUpdated = CalcIsUpdated();
                    if (!isUpdated)
                        assetTask = client.Repository.Release.GetAllAssets("aglab2", "SM64StarManager", versionTask.Result[0].Id);
                }
            }

            return false;
        }

        bool CalcIsUpdated()
        {
            Version updVersion = UpdateVersion();
            return updVersion <= version;
        }

        public bool IsUpdated()
        {
            return isUpdated;
        }

        public string UpdateName()
        {
            return versionTask.Result[0].Body;
        }

        public string DownloadPath()
        {
            return assetTask.Result[0].BrowserDownloadUrl;
        }

        public Version UpdateVersion()
        {
            return new Version(versionTask.Result[0].TagName);
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
