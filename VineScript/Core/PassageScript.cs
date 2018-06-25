using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using VineScript.Compiler;

namespace VineScript.Core
{
    /// <summary>Type of the script's source: either from stdin or from a file.</summary>
    public enum PassageScriptSource {
        File,
        Stdin
    }

    public class PassageScript
    {
        public const string STDIN = "<stdin>";
        
        /// <summary>Source of the script.</summary>
        public readonly PassageScriptSource Source;

        /// <summary>Name of the script.</summary>
        public string Name {
            get {
                return _name;
            }
        }
        private string _name;
        
        /// <summary>Filename of the script.</summary>
        public string Filename {
            get {
                return _filename;
            }
        }
        private string _filename;
        
        /// <summary>Source code of the script.</summary>
        public string SourceCode {
            get {
                return _sourceCode;
            }
        }
        private string _sourceCode;
        
        /// <summary>Tree for the parser.</summary>
        public ParserRuleContext Tree {
            get {
                return _tree;
            }
        }
        private ParserRuleContext _tree;

        // TODO Should keep track of some sort of CompilationInstance
        // instead of the whole compiler. CompilationInstance would
        // be produced when VineCompiler load the script and passed
        // back to VineCompiler when we want to execute it.
        /// <summary>Compiler for this script.</summary>
        private VineCompiler compiler;
        
        public PassageScript(string scriptname, string sourceCode)
        {
            _name = scriptname;
            _filename = scriptname;
            Source = PassageScriptSource.Stdin;
            _sourceCode = sourceCode;
            compiler = new VineCompiler(null);
        }
        
        public PassageScript(string scriptname, string sourceCode, string filename)
        {
            _name = scriptname;
            _filename = filename;
            Source = PassageScriptSource.File;
            _sourceCode = sourceCode;
            compiler = new VineCompiler(null);
        }

        public bool Loaded {
            get {
                return _tree != null;
            }
        }

        public bool Load()
        {
            string preprocessed = compiler.PreProcessing(SourceCode);
            compiler.Init(preprocessed, Filename);
            _tree = compiler.BuildTree();
            return true;
        }

        public void Unload()
        {
            //compiler.ClearCache();
            _tree = null;
            compiler = null;
        }

        public PassageResult Run(VineStory story)
        {
            return compiler.CompileTree(_tree, story);
        }
    }
}
