using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using Android.App;

using PCLStorage;

namespace PokeD.Server.Android.WrapperInstances
{
    public class FileSystemWrapperInstance : Aragas.Core.Wrappers.IFileSystem
    {
        private IFolder BaseFolder { get; }

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


            BaseFolder = new FileSystemFolder(baseDirectory);
            SettingsFolder = new FileSystemFolder(settingsPath);
            LogFolder = new FileSystemFolder(logPath);
            CrashLogFolder = new FileSystemFolder(crashLogPath);
            LuaFolder = new FileSystemFolder(luaPath);
            DatabaseFolder = new FileSystemFolder(databasePath);
            ContentFolder = new FileSystemFolder(contentPath);

            CopyEmbeddedResources();
        }

        private void CopyEmbeddedResources()
        {
            var assembly = typeof(FileSystemWrapperInstance).Assembly;
            foreach (var resourceName in assembly.GetManifestResourceNames())
                SaveEmbeddedResource(assembly, resourceName);
        }
        private void SaveEmbeddedResource(Assembly assembly, string resourcePath)
        {
            var path = GetManifestResourcePath(assembly, resourcePath);

            var folder = BaseFolder;
            var dirs = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Reverse().Skip(1).Reverse();
            folder = dirs.Aggregate(folder, (current, dir) => current.CreateFolderAsync(dir, CreationCollisionOption.OpenIfExists).Result);


            var file = folder.CreateFileAsync(Path.GetFileName(path), CreationCollisionOption.OpenIfExists).Result;
            using (var stream = assembly.GetManifestResourceStream(resourcePath))
            using (var fileStream = file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite, CancellationToken.None).Result)
                stream?.CopyTo(fileStream);
            
        }
        private static string GetManifestResourcePath(Assembly assembly, string path)
        {
            var array = path.Replace($"{assembly.GetName().Name}.", "").Split('.');
            var fileExtention = array.Skip(Math.Max(0, array.Length - 1)).FirstOrDefault();
            var path1 = array.Take(Math.Max(0, array.Length - 1));

            var path2 = string.Join(Path.DirectorySeparatorChar.ToString(), path1);
            return $"{path2}.{fileExtention}";
        }
    }
}
