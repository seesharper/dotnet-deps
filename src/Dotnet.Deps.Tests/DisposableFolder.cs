using System;
using System.IO;

namespace Dotnet.Deps.Tests
{
    public class DisposableFolder : IDisposable
    {
        public DisposableFolder()
        {
            var tempFolder = System.IO.Path.GetTempPath();
            this.Path = System.IO.Path.Combine(tempFolder, System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetTempFileName()));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            RemoveDirectory(Path);

            void RemoveDirectory(string path)
            {
                if (!Directory.Exists(path))
                {
                    return;
                }
                NormalizeAttributes(path);

                foreach (string directory in Directory.GetDirectories(path))
                {
                    RemoveDirectory(directory);
                }

                try
                {
                    Directory.Delete(path, true);
                }
                catch (IOException)
                {
                    Directory.Delete(path, true);
                }
                catch (UnauthorizedAccessException)
                {
                    Directory.Delete(path, true);
                }

                void NormalizeAttributes(string directoryPath)
                {
                    string[] filePaths = Directory.GetFiles(directoryPath);
                    string[] subdirectoryPaths = Directory.GetDirectories(directoryPath);

                    foreach (string filePath in filePaths)
                    {
                        File.SetAttributes(filePath, FileAttributes.Normal);
                    }
                    foreach (string subdirectoryPath in subdirectoryPaths)
                    {
                        NormalizeAttributes(subdirectoryPath);
                    }
                    File.SetAttributes(directoryPath, FileAttributes.Normal);
                }
            }
        }
    }
}