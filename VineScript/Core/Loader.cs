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
        private Dictionary<string, PassageScript> passageScripts = new Dictionary<string, PassageScript>();

        private Compiler.VineCompiler compiler;
        
        public string BaseDir { get ; set; } = "./";

        public Loader()
        {
            compiler = new Compiler.VineCompiler(null);
        }

        public PassageScript Get(string scriptname)
        {
            PassageScript passage;
            passageScripts.TryGetValue(scriptname, out passage);
            return passage;
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
                    AddFile(fn, ref scriptname, dirfullpath);
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
            string filefullpath = Path.GetFullPath(filename);
            string dirfullpath = Path.GetFullPath(BaseDir);
            return AddFile(filefullpath, ref scriptname, dirfullpath);
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
                "Added '{0}':{1} {2}", scriptname, System.Environment.NewLine, fullpath
            ));
            passageScripts.Add(scriptname, new PassageScript(scriptname, fullpath));
            return true;
        }

        public bool LoadPassage(string scriptname)
        {
            PassageScript passage;
            if (passageScripts.TryGetValue(scriptname, out passage))
            {
                if (passage.Loaded) {
                    throw new Exception(string.Format(
                        "'{0}' can't be loaded because it's already loaded!", scriptname
                    ));
                }

                StreamReader istream = File.OpenText(passage.Filename);
                string code = istream.ReadToEnd();
                compiler.Init(code, passage.Filename);
                var tree = compiler.BuildTree();
                Get(scriptname).Load(tree);
                return true;
            }

            throw new Exception(string.Format("'{0}' doesn't exist!", scriptname));
        }

        public bool LoadFilesFromDir(string dirname, string ext="*.vine|*", bool recursive=true)
        {
            if (AddFilesFromDir(dirname, ext, recursive)) {
                return LoadAll();
            }
            return false;

        }

        public bool LoadAll()
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
                } catch (Compiler.VineParseException e) {
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

        public bool Unload(string scriptname)
        {
            PassageScript passage = Get(scriptname);
            if (passage != null) {
                passage.Unload();
                return true;
            }
            return false;
        }

        public bool UnloadAll()
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
            // TODO replace folder separator '\' on windows by '/'
            // so it's the same name on every os.
            int overflow = dir_root.Last() == Path.DirectorySeparatorChar ? 0 : 1;
            string without_path = filename.Substring(dir_root.Length + overflow);
            string with_ext = Path.GetExtension(filename);
            // remove the extension
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
