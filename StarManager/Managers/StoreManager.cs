using System;
using System.IO;
using System.Reflection;

namespace StarDisplay
{
    class StoreManager
    {
        public static string AppDataPath
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); }
        }

        public static string ExePath
        {
            get { return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location); }
        }

        public static string StarDisplayPath
        {
            get { return Path.Combine(AppDataPath, "StarDisplay"); }
        }

        public static string LayoutFolder
        {
            get { Directory.CreateDirectory(StarDisplayPath); return Path.Combine(StarDisplayPath, "layout"); }
        }

        public static string LegacyConfigPath
        {
            get { return Path.Combine(ExePath, "settings.cfg"); }
        }

        public static string ConfigPath
        {
            get { return Path.Combine(ExePath, "settings.jcfg"); }
        }

        public static string GetPathForLayoutName(string name)
        {
            Directory.CreateDirectory(LayoutFolder);
            return Path.Combine(LayoutFolder, name + ".jsml");
        }
    }
}
