using System.IO;

namespace DevTools
{
    public static class DirectoryExtensions
    {
        /// <summary>    Appends <see cref="Path.DirectorySeparatorChar"/> to '<paramref name="directory"/>' and returns the result.    </summary>
        public static string AsDirectory(this string directory)
        {
            return directory + Path.DirectorySeparatorChar;
        }

        /// <summary>    Recursively searches for a file '<paramref name="fileName"/>' in the '<paramref name="folder"/>' directory, including subdirectories.    </summary>
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
            foreach (string subFolder in Directory.GetDirectories(folder))
            {
                DeleteFolder(subFolder);
            }

            foreach (string file in Directory.GetFiles(folder))
            {
                File.Delete(file);
            }

            Directory.Delete(folder);
        }
    }
}
