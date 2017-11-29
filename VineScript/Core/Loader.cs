using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VineScript.Compiler;
using System.Collections;

namespace VineScript.Core
{
    public class Loader
    {
        public Dictionary<string, PassageScript> passageScripts = new Dictionary<string, PassageScript>();

        private const string CURRENT_DIR = "./";

        /// <summary>
        /// Base directory containing the scripts. By default it uses the
        /// directory in which the program is launched. Calling LoadFromDir()
        /// will change its value.
        /// </summary>
        public string BaseDir { get ; set; } = CURRENT_DIR;

        public IEnumerable<string> LoadedScripts
        {
            get {
                foreach (var item in this.passageScripts) {
                    yield return item.Key;
                }
                yield break;
            }
        }

        public Loader(string basedir="")
        {
            if (!string.IsNullOrWhiteSpace(basedir)) {
                BaseDir = basedir;
            }
        }

        public PassageScript Get(string scriptname)
        {
            PassageScript passage;
            if (!passageScripts.TryGetValue(scriptname, out passage)) {
                Console.WriteLine(string.Format("'{0}' doesn't exist!", scriptname));
            }
            return passage;
        }

        /// <summary>
        /// Add files from a given directory. That directory will become the
        /// root directory for the scripts.
        /// </summary>
        /// <param name="dirname">Directory name</param>
        /// <param name="ext">Files extensions (not case sensitive).
        /// Can add more than one extension with a pipe. E.g: "*.vine|*.txt".
        /// Files can also be searched by pattern. Eg: all files starting with 'foo':
        /// "foo*"</param>
        /// <param name="recursive">Include the current `dirname` directory 
        /// and all its subdirectories in the search.</param>
        /// <returns></returns>
        private bool AddFilesFromDir(string dirname, string ext, bool recursive=true)
        {
            bool added = false;
            try {
                if (!string.IsNullOrWhiteSpace(dirname)) {
                    BaseDir = Path.GetFullPath(dirname);
                }
                string[] filenames = GetFiles(BaseDir, ext,
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
                 );

                foreach (string fn in filenames) {
                    AddFile(fn);
                }
                added = filenames.Length > 0;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            return added;
        }

        private bool AddFile(string filename, string scriptname="")
        {
            string filefullpath = Path.GetFullPath(filename);
            string dirfullpath = Path.GetFullPath(BaseDir);
            return AddFile(filefullpath, ref scriptname, dirfullpath);
        }

        private bool AddFile(string filename, ref string scriptname, string dir_root)
        {
            if (string.IsNullOrWhiteSpace(scriptname)) {
                // Give the script a name based on the filename
                scriptname = GetScriptNameFromFile(filename, dir_root);
            }

            if (passageScripts.ContainsKey(scriptname)) {
                throw new Exception(string.Format(
                    "'{0}' can't be added because another script has the same name!",
                    scriptname
                ));
            }

            string fullpath = Path.GetFullPath(filename);
            Console.WriteLine(string.Format(
                "Added '{0}':{1} {2}", scriptname, Environment.NewLine, fullpath
            ));
            StreamReader istream = File.OpenText(fullpath);
            string code = istream.ReadToEnd();
            PassageScript passage = new PassageScript(scriptname, code, fullpath);
            passageScripts.Add(scriptname, passage);
            return true;
        }

        private bool AddCode(string sourceCode, string scriptname)
        {
            if (string.IsNullOrWhiteSpace(scriptname)) {
                throw new Exception("Can't add code with no script name associated!");
            }

            if (passageScripts.ContainsKey(scriptname)) {
                throw new Exception(string.Format(
                    "'{0}' can't be added because another script has the same name!",
                    scriptname
                ));
            }
            
            Console.WriteLine(string.Format(
                "Added '{0}':{1} {2}", scriptname, Environment.NewLine, PassageScript.STDIN
            ));
            PassageScript passage = new PassageScript(scriptname, sourceCode);
            passageScripts.Add(scriptname, passage);
            return true;
        }

        private bool LoadPassage(string scriptname)
        {
            PassageScript passage;
            if (passageScripts.TryGetValue(scriptname, out passage))
            {
                if (passage.Loaded) {
                    throw new Exception(string.Format(
                        "'{0}' can't be loaded because it's already loaded!", scriptname
                    ));
                }
                passage.Load();
                return true;
            }

            throw new Exception(string.Format("'{0}' doesn't exist!", scriptname));
        }

        public bool LoadFile(string filename, string scriptname="")
        {
            string filefullpath = Path.GetFullPath(filename);
            string dirfullpath = Path.GetFullPath(BaseDir);
            if (AddFile(filefullpath, ref scriptname, dirfullpath)) {
                return LoadPassage(scriptname);
            }
            return false;
        }

        public bool LoadCode(string sourceCode, string scriptname)
        {
            if (AddCode(sourceCode, scriptname)) {
                return LoadPassage(scriptname);
            }
            return false;
        }
        
        /// <summary>
        /// Load files from a given directory. That directory will become the
        /// implicit root directory, and all scripts path will be relative to
        /// it, meaning that you won't need to specify it when calling scripts.
        /// E.g:
        /// You have the directory "./scripts/foo" containing the files:
        ///     * hello.vine
        ///     * bar.vine
        ///     * sub/lorem.vine
        /// You can call:
        /// loader.LoadFromDir("./scripts/foo/bar");
        /// But you don't need to specify the path again, it's now the base
        /// directory for the scripts.
        /// loader.Get("hello");
        /// loader.Get("sub/lorem");
        /// </summary>
        /// <param name="dirname"></param>
        /// <param name="ext"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public bool LoadFromDir(string dirname="", string ext="*.vine", bool recursive=true)
        {
            if (AddFilesFromDir(dirname, ext, recursive)) {
                return LoadAll();
            }
            return false;
        }

        /// <summary>
        /// Load all added scripts.
        /// </summary>
        /// <returns>true if all the added scripts were loaded.</returns>
        private bool LoadAll()
        {
            bool parseError = false;
            if (passageScripts.Count == 0) {
                return false;
            }
            foreach (var el in passageScripts) {
                try {
                    if (el.Value.Loaded || !LoadPassage(el.Key)) {
                        return false;
                    }
                } catch (VineParseException e) {
                    // Print the parse error but doesn't stop the loop here.
                    // Continue loading more files to print more potential
                    // parse errors
                    Console.Write(e.Message);
                    // we mark the function to return an error though
                    parseError = true;
                }
            }
            return !parseError;
        }

        private bool Unload(string scriptname)
        {
            PassageScript passage = Get(scriptname);
            if (passage != null) {
                passage.Unload();
                return true;
            }
            return false;
        }

        private bool UnloadAll()
        {
            foreach (var el in passageScripts) {
                if (!Unload(el.Key)) {
                    return false;
                }
            }
            return passageScripts.Count == 0;
        }

        public bool Remove(string scriptname)
        {
            bool unloaded = true;
            if (passageScripts.ContainsKey(scriptname)) {
                unloaded = Unload(scriptname);
            }
            return unloaded && passageScripts.Remove(scriptname);
        }

        public bool RemoveAll()
        {
            for (int i = passageScripts.Count - 1; i >= 0; i--) {
                if (!Remove(passageScripts.ElementAt(i).Key)) {
                    return false;
                }
            }
            return passageScripts.Count == 0;
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
            // Parts of the script's name can be directories. It is needed to
            // change the directory separator char in order to be portable
            // on every OS. Otherwise the name would be different:
            //  * Windows => "scripts\foo\bar"
            //  * Unix => "scripts/foo/bar"
            // I chose the Unix one "/" as the standard directory separator 
            // char when calling a script
            var scriptPart = PathParts(filename);
            var basePart = PathParts(dir_root);
            string scriptname = "";
            for (int i = basePart.Count; i < scriptPart.Count; i++) {
                scriptname += scriptPart[i].Name;
                if (i < scriptPart.Count - 1) {
                    scriptname += "/";
                }
            }
            string extension = Path.GetExtension(scriptname);
            // remove the extension
            return scriptname.Substring(0, scriptname.Length - extension.Length);
        }

        /// <summary>
        /// Get a list of each parts of a path. Can contain a filename.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static List<DirectoryInfo> PathParts(string path)
        {
            DirectoryInfo current = new DirectoryInfo(path);
            var parts = new List<DirectoryInfo>();
            while (current != null) {
                parts.Add(current);
                current = current.Parent;
            }
            parts.Reverse();
            return parts;
        }

        /// <summary>
        /// Get a list of files for a given path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="patterns"></param>
        /// <param name="opt"></param>
        /// <returns></returns>
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
