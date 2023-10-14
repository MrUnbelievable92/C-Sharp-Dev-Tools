using System;
using System.IO;
using System.Collections.Generic;

namespace DevTools
{
    public static class DirectoryExtensions
    {
        /// <summary>    Appends <see cref="Path.DirectorySeparatorChar"/> to '<paramref name="directory"/>' and returns the result.    </summary>
        public static string AsDirectory(this string directory)
        {
            return directory + Path.DirectorySeparatorChar;
        }

        /// <summary>    Recursively searches for a file '<paramref name="fileName"/>' in the '<paramref name="folder"/>' directory in a depth-first manner, including subdirectories.    </summary>
        public static string FindInFolder(string folder, string fileName)
        {
            string result = folder.AsDirectory() + fileName;
            if (File.Exists(result))
            {
                return result;
            }

            foreach (string subDirectory in Directory.GetDirectories(folder))
            {
                result = FindInFolder(subDirectory, fileName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>    Deletes a directory '<paramref name="folder"/>' and all of its contents, including subdirectories.     </summary>
        public static void DeleteFolder(string folder)
        {
            ForEachDirectory(folder,
            (__folder) =>
            {
                foreach (string file in Directory.GetFiles(__folder))
                {
                    File.Delete(file);
                }

                Directory.Delete(__folder);
            });
        }

        /// <summary>    Recursively performs an <see cref="Action{string}"/> '<paramref name="action"/>' on each subdirectory within the directory '<paramref name="folder"/>' in a depth-first manner.     </summary>
        public static void ForEachDirectory(string folder, Action<string> action)
        {
            foreach (string subFolder in Directory.GetDirectories(folder))
            {
                ForEachDirectory(subFolder, action);
                action(subFolder);
            }
        }

        /// <summary>    Recursively performs an <see cref="Action{string}"/> '<paramref name="action"/>' on each file within the directory '<paramref name="folder"/>' in a depth-first manner.     </summary>
        public static void ForEachFile(string folder, Action<string> action)
        {
            ForEachDirectory(folder,
            (__folder) =>
            {
                foreach (string file in Directory.GetFiles(__folder))
                {
                    action(file);
                }
            });

            foreach (string file in Directory.GetFiles(folder))
            {
                action(file);
            }
        }

        /// <summary>    Recursively performs a <see cref="Func{string, T}"/> '<paramref name="func"/>' on each subdirectory within the directory '<paramref name="folder"/>' in a depth-first manner.     </summary>
        public static IEnumerable<T> ForEachDirectory<T>(string folder, Func<string, T> func)
        {
            foreach (string subFolder in Directory.GetDirectories(folder))
            {
                ForEachDirectory(subFolder, func);

                yield return func(subFolder);
            }
        }

        /// <summary>    Recursively performs a <see cref="Func{string, T}"/> '<paramref name="func"/>' on each file within the directory '<paramref name="folder"/>' in a depth-first manner.     </summary>
        public static IEnumerable<T> ForEachFile<T>(string folder, Func<string, T> func)
        {
            List<T> result = new List<T>();

            ForEachFile(folder, (file) => result.Add(func(file)));

            return result;
        }
    }
}
