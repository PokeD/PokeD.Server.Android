using System;
using System.IO;
using Android.App;
using PCLStorage;

namespace PokeD.Server.Android.WrapperInstances
{
    public class FileSystemWrapperInstance : Aragas.Core.Wrappers.IFileSystem
    {
        public IFolder SettingsFolder { get; }
        public IFolder LogFolder { get; }
        public IFolder CrashLogFolder { get; }
        public IFolder LuaFolder { get; }
        public IFolder AssemblyFolder { get; }
        public IFolder DatabaseFolder { get; }
        public IFolder ContentFolder { get; }
        public IFolder OutputFolder { get; }

        public FileSystemWrapperInstance()
        {
            var baseDirectory = Application.Context.GetExternalFilesDir(null).ParentFile.AbsolutePath;

            var settingsPath = Path.Combine(baseDirectory, "Settings");
            var logPath = Path.Combine(baseDirectory, "Logs");
            var crashLogPath = Path.Combine(baseDirectory, "Crash");
            var luaPath = Path.Combine(baseDirectory, "Lua");
            var databasePath = Path.Combine(baseDirectory, "Database");
            var contentPath = Path.Combine(baseDirectory, "Content");

            if (!Directory.Exists(settingsPath))
                Directory.CreateDirectory(settingsPath);

            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            if (!Directory.Exists(crashLogPath))
                Directory.CreateDirectory(crashLogPath);

            if (!Directory.Exists(luaPath))
                Directory.CreateDirectory(luaPath);

            if (!Directory.Exists(databasePath))
                Directory.CreateDirectory(databasePath);

            if (!Directory.Exists(contentPath))
                Directory.CreateDirectory(contentPath);


            SettingsFolder = new FileSystemFolder(settingsPath);
            LogFolder = new FileSystemFolder(logPath);
            CrashLogFolder = new FileSystemFolder(crashLogPath);
            LuaFolder = new FileSystemFolder(luaPath);
            DatabaseFolder = new FileSystemFolder(databasePath);
            ContentFolder = new FileSystemFolder(contentPath);



            //var baseDirectory = FileSystem.Current.LocalStorage;

            //var vars = Environment.GetEnvironmentVariable("EMULATED_STORAGE_TARGET");
            //var baseDirectory = FileSystem.Current.GetFolderFromPathAsync(vars).Result;
            //var dar = baseDirectory.CreateFolderAsync("PokeD.Server.Android", CreationCollisionOption.OpenIfExists).Result;

            //SettingsFolder  = baseDirectory.CreateFolderAsync("Settings", CreationCollisionOption.OpenIfExists).Result;
            //LogFolder       = baseDirectory.CreateFolderAsync("Logs", CreationCollisionOption.OpenIfExists).Result;
            //CrashLogFolder  = LogFolder.CreateFolderAsync("Crash", CreationCollisionOption.OpenIfExists).Result;
            //LuaFolder       = baseDirectory.CreateFolderAsync("Lua", CreationCollisionOption.OpenIfExists).Result;
            //DatabaseFolder  = baseDirectory.CreateFolderAsync("Database", CreationCollisionOption.OpenIfExists).Result;
            //ContentFolder = baseDirectory.CreateFolderAsync("Content", CreationCollisionOption.OpenIfExists).Result;
        }
    }
}
