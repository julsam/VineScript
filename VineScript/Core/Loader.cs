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
        private Dictionary<string, PassageScript> passageScripts = new Dictionary<string, PassageScript>();

        private const string CURRENT_DIR = "./";

        /// <summary>
        /// Base directory containing the scripts. By default it uses the
        /// directory in which the program is launched. Calling LoadFromDir()
        /// will change its value.
        /// </summary>
        public string BaseDir {
            get {
                return _basedir;
            }
            set {
                if (string.IsNullOrWhiteSpace(value)) {
                    throw new Exception("BaseDir can't be empty!");
                }
                _basedir = Path.GetFullPath(value);
            }
        }
        private string _basedir;

        /// <summary>
        /// List the name of the loaded scripts
        /// </summary>
        public IEnumerable<string> LoadedScripts
        {
            get {
                foreach (var item in this.passageScripts) {
                    yield return item.Key;
                }
                yield break;
            }
        }

        /// <summary>
        /// Vine Loader to add scripts.
        /// </summary>
        /// <param name="basedir">
        /// The root directory when calling scripts. The scripts' path become
        /// relative to BaseDir, meaning that you won't need
        /// to specify it when calling scripts.
        /// E.g:
        /// You have the directory "./scripts/foo" containing the files:
        ///     hello.vine, bar.vine, sub/lorem.vine
        /// You can load the whole directory:
        ///     Loader loader = new Loader("./scripts/foo");
        /// But you don't need to specify that path again, when you need
        /// to call a script:
        ///     loader.Get("hello");
        ///     story.RunPassage("sub/lorem");
        /// 
        /// By default, it'll use the directory in which the program is
        /// launched as the root directory.
        /// </param>
        public Loader(string basedir="")
        {
            if (!string.IsNullOrWhiteSpace(basedir)) {
                BaseDir = basedir;
            } else {
                BaseDir = CURRENT_DIR;
            }
        }

        /// <summary>
        /// Get a loaded script.
        /// </summary>
        /// <param name="scriptname">The name of the script to load.</param>
        /// <returns>Returns the script if it is found, otherwise null.</returns>
        public PassageScript Get(string scriptname)
        {
            PassageScript passage;
            if (!passageScripts.TryGetValue(scriptname, out passage)) {
                Console.WriteLine(string.Format("'{0}' doesn't exist!", scriptname));
            }
            return passage;
        }

        /// <summary>
        /// Add files from a given directory.
        /// </summary>
        /// <param name="dirname">Relative directory name</param>
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
                string combinedDir = CombinePaths(BaseDir, dirname);
                
                if (!Directory.Exists(combinedDir)) {
                    throw new Exception(string.Format(
                        "'{0}' is not a valid directory path!", combinedDir
                    ));
                }

                string[] filenames = GetFiles(combinedDir, ext,
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
            return AddFile(filename, ref scriptname, BaseDir);
        }

        private bool AddFile(string filename, ref string scriptname, string dir_root)
        {
            string filefullpath = Path.GetFullPath(filename);
            if (string.IsNullOrWhiteSpace(scriptname)) {
                // Give the script a name based on the filename
                scriptname = GetScriptNameFromFile(filefullpath, dir_root);
            }

            if (passageScripts.ContainsKey(scriptname)) {
                throw new Exception(string.Format(
                    "'{0}' can't be added because another script has the same name!",
                    scriptname
                ));
            }
            
            Console.WriteLine(string.Format(
                "Added '{0}':{1} {2}", scriptname, Environment.NewLine, filefullpath
            ));
            StreamReader istream = File.OpenText(filefullpath);
            string code = istream.ReadToEnd();
            PassageScript passage = new PassageScript(scriptname, code, filefullpath);
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

        /// <summary>
        /// Load a VineScript file.
        /// </summary>
        /// <param name="filename">The file path, relative to the BaseDir.</param>
        /// <param name="scriptname">Rename the script. By default it'll use the filename
        /// (without the extension) and the path (relative to BaseDir).</param>
        /// <returns>True if the file was successfully loaded.</returns>
        public bool LoadFile(string filename, string scriptname="")
        {
            //string filefullpath = Path.GetFullPath(filename);
            string combined = CombinePaths(BaseDir, filename);
            string dirfullpath = Path.GetFullPath(BaseDir);
            if (AddFile(combined, ref scriptname, dirfullpath)) {
                return LoadPassage(scriptname);
            }
            return false;
        }

        /// <summary>
        /// Load a VineScript source code.
        /// </summary>
        /// <param name="filename">The source code to load.</param>
        /// <param name="scriptname">Name the source code.</param>
        /// <returns>True if the code was successfully loaded.</returns>
        public bool LoadCode(string sourceCode, string scriptname)
        {
            if (AddCode(sourceCode, scriptname)) {
                return LoadPassage(scriptname);
            }
            return false;
        }
        
        /// <summary>
        /// Load files from a given directory. 
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

            // TODO "there is n errors on n scripts" message
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

        /// <summary>
        /// Unload and remove a script <paramref name="scriptname"/>.
        /// </summary>
        /// <param name="scriptname">Script to remove.</param>
        /// <returns>True if successfully unloaded and removed.</returns>
        public bool Remove(string scriptname)
        {
            bool unloaded = true;
            if (passageScripts.ContainsKey(scriptname)) {
                unloaded = Unload(scriptname);
            }
            return unloaded && passageScripts.Remove(scriptname);
        }

        /// <summary>
        /// Unload and remove all scripts.
        /// </summary>
        /// <returns>True if successfully unloaded and removed.</returns>
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
            // First, check that the given file is not out of BaseDir or its subdirectories
            StringComparison comparison = StringComparison.Ordinal;
            if (Environment.OSVersion.Platform != PlatformID.Unix
                && Environment.OSVersion.Platform != PlatformID.MacOSX) {
                // ignore case on windows
                comparison = StringComparison.OrdinalIgnoreCase;
            }
            if (!filename.StartsWith(dir_root, comparison)) {
                throw new Exception(string.Format(
                    "'{0}' can't be loaded because it's out of the defined base directory!",
                    filename
                ));
            }
            
            // Parts of the script's name can be directories. It is needed to
            // change the directory separator char in order to be portable
            // on every OS. Otherwise the name would be different:
            //  * Windows => "scripts\foo\bar"
            //  * Unix => "scripts/foo/bar"
            // I chose the Unix one "/" as the standard directory separator 
            // char when calling a script.
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

        /// <summary>
        /// Combine 2 paths
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        private static string CombinePaths(string path1, string path2)
        {
            string combined = path1.TrimEnd(Path.DirectorySeparatorChar)
                + Path.DirectorySeparatorChar
                + path2.TrimStart(Path.DirectorySeparatorChar);
            return combined;
        }
    }
}
