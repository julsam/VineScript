using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using VineScript.Compiler;

namespace VineScript.Core
{
    public enum PassageScriptSource {
        File,
        Stdin
    }

    public class PassageScript
    {
        public const string STDIN = "<stdin>";

        public readonly PassageScriptSource Source;

        private string _name;
        public string Name {
            get {
                return _name;
            }
        }

        private string _filename;
        public string Filename {
            get {
                return _filename;
            }
        }
        
        private string _sourceCode;
        public string SourceCode {
            get {
                return _sourceCode;
            }
        }
        
        private ParserRuleContext _tree;
        public ParserRuleContext Tree {
            get {
                return _tree;
            }
        }

        public bool RecordStats { get; set; }

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
