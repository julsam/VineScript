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
        private Dictionary<string, PassageScript> loadedScripts = new Dictionary<string, PassageScript>();
        private Dictionary<string, string> unloadedScripts = new Dictionary<string, string>();

        private Compiler.VineCompiler compiler;

        public Loader()
        {
            compiler = new Compiler.VineCompiler(null);
        }

        public PassageScript Get(string scriptname)
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
                string dirfullpath = Path.GetFullPath(dirname);
                string[] filenames = GetFiles(dirfullpath, ext,
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
                 );

                foreach (string fn in filenames) {
                    string scriptname = GetScriptNameFromFile(fn, dirfullpath);
                    AddFile(fn, scriptname, dirfullpath);
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
            string dirfullpath = Path.GetFullPath(filename);
            return AddFile(filename, scriptname, dirfullpath);
        }

        private bool AddFile(string filename, string scriptname, string dir_root)
        {
            if (string.IsNullOrWhiteSpace(scriptname)) {
                // Give the script a name based on the filename
                scriptname = GetScriptNameFromFile(filename, dir_root);
            }

            if (unloadedScripts.ContainsKey(scriptname)) {
                throw new Exception(string.Format(
                    "'{0}' can't be added because another script has the same name!",
                    scriptname
                ));
            }

            string fullpath = Path.GetFullPath(filename);
            Console.WriteLine(string.Format(
                "Added '{0}':{1} {2}", scriptname, System.Environment.NewLine, fullpath
            ));
            unloadedScripts.Add(scriptname, fullpath);
            return true;
        }

        public bool Load(string scriptname)
        {
            if (loadedScripts.ContainsKey(scriptname)) {
                throw new Exception(string.Format(
                    "'{0}' can't be loaded because it's already loaded!", scriptname
                ));
            }

            string filename;
            if (unloadedScripts.TryGetValue(scriptname, out filename)) {
                StreamReader istream = File.OpenText(filename);
                string code = istream.ReadToEnd();
                compiler.Init(code, filename);
                var tree = compiler.BuildTree();
                PassageScript loadedScript = new PassageScript(scriptname, filename, tree);
                loadedScripts.Add(scriptname, loadedScript);
                return true;
            }

            throw new Exception(string.Format("'{0}' doesn't exist!", scriptname));
        }

        public bool LoadAll()
        {
            bool parseError = false;
            if (unloadedScripts.Count == 0) {
                return false;
            }
            foreach (var el in unloadedScripts) {
                try {
                    if (!Load(el.Key)) {
                        return false;
                    }
                } catch (Compiler.VineParseException e) {
                    Console.Write(e.Message);
                    parseError = true;
                }
            }
            // TODO if parse errors throw exception?
            return !parseError;
        }

        public bool Unload(string scriptname)
        {
            return loadedScripts.Remove(scriptname);
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
            bool unloaded = true;
            if (loadedScripts.ContainsKey(scriptname)) {
                unloaded = Unload(scriptname);
            }
            return unloaded && unloadedScripts.Remove(scriptname);
        }

        public bool RemoveAll()
        {
            for (int i = unloadedScripts.Count - 1; i >= 0; i--) {
                if (!Remove(unloadedScripts.ElementAt(i).Key)) {
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

        private static string GetScriptNameFromFile(string filename, string dir_root)
        {
            // TODO replace folder separator '\' on windows by '/'
            // so it's the same name on every os.
            string without_path = filename.Substring(dir_root.Length + 1);
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
