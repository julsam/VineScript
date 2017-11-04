using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VineScript.Core
{
    public class Loader
    {
        private Dictionary<string, LoadedScript> loadedScripts = new Dictionary<string, LoadedScript>();
        private Dictionary<string, string> unloadedScripts = new Dictionary<string, string>();

        public void Get(string scriptname)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Add files from a given directory.
        /// </summary>
        /// <param name="dirname">Directory name</param>
        /// <param name="ext">Files extensions (not case sensitive).
        /// Can add more than one extension with a pipe. E.g: "*.vine|*.txt".
        /// Files can also be searched by pattern. Eg: all files starting with 'foo':
        /// "foo*"</param>
        /// <param name="recursive">Include the current `dirname` directory 
        /// and all its subdirectories in the search.</param>
        /// <returns></returns>
        public bool AddFilesFromDir(string dirname, string ext="*.vine|*", bool recursive=true)
        {
            bool added = false;
            try {
                string fullpath = Path.GetFullPath(dirname);
                string[] filenames = GetFiles(fullpath, ext,
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
                 );

                foreach (string fn in filenames) {
                    string scriptname = GetScriptNameFromFile(fn);
                    AddFile(fn, scriptname);
                }
                added = filenames.Length > 0;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            return added;
        }

        public bool AddFile(string filename, string scriptname="")
        {
            if (string.IsNullOrWhiteSpace(scriptname)) {
                // Give the script a name based on the filename
                scriptname = GetScriptNameFromFile(filename);
            }

            if (!unloadedScripts.ContainsKey(scriptname)) {
                string fullpath = Path.GetFullPath(filename);
                Console.WriteLine(string.Format(
                    "Added '{0}':{1} {2}", scriptname, System.Environment.NewLine, fullpath
                ));
                unloadedScripts.Add(scriptname, fullpath);
                return true;
            } else {
                throw new Exception(string.Format(
                    "'{0}' can't be added because another script has the same name!",
                    scriptname
                ));
            }
        }

        public bool Load(string scriptname)
        {
            throw new NotImplementedException();
        }

        public bool LoadAll()
        {
            if (unloadedScripts.Count == 0) {
                return false;
            }
            foreach (var el in unloadedScripts) {
                if (!Load(el.Key)) {
                    return false;
                }
            }
            return true;
        }

        public bool Unload(string scriptname)
        {
            throw new NotImplementedException();
        }

        public bool UnloadAll()
        {
            foreach (var el in loadedScripts) {
                if (!Unload(el.Key)) {
                    return false;
                }
            }
            return loadedScripts.Count == 0;
        }

        public bool Remove(string scriptname)
        {
            if (loadedScripts.ContainsKey(scriptname)) {
                Unload(scriptname);
            }
            return unloadedScripts.Remove(scriptname);
        }

        public bool RemoveAll()
        {
            foreach (var el in unloadedScripts) {
                if (!Remove(el.Key)) {
                    return false;
                }
            }
            return unloadedScripts.Count == 0 && loadedScripts.Count == 0;
        }

        public void ClearMemory()
        {
            throw new NotImplementedException();
        }

        //
        // UTILS
        //

        private static string GetScriptNameFromFile(string filename)
        {
            string fullpath = Path.GetFullPath(filename);
            string without_path = filename.Substring(fullpath.Length + 1);
            string with_ext = Path.GetExtension(filename);
            return without_path.Substring(0, without_path.Length - with_ext.Length);
        }

        private static string[] GetFiles(string path, string patterns, SearchOption opt)
        {
            // Escape the dot between the filename and extension
            patterns = patterns.Replace(".", "\\.");
            // Replace the "all" pattern to the regex equivalent
            patterns = patterns.Replace('*', '.');

            Regex reSearchPattern = new Regex(patterns, RegexOptions.IgnoreCase);

            // Search all files in the given dir (and subdirs)
            var files = Directory.EnumerateFiles(path, "*", opt)
                // Get the ones that match the patterns
                .Where(file => reSearchPattern.IsMatch(Path.GetFileName(file)));
            return files.ToArray();
        }
    }
}
