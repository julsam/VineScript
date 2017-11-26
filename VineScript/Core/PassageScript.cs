using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;

namespace VineScript.Core
{
    public class PassageScript
    {
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
        
        private ParserRuleContext _tree;
        public ParserRuleContext Tree {
            get {
                return _tree;
            }
        }

        public bool RecordStats { get; set; }

        public PassageScript(string scriptname, string filename, ParserRuleContext tree)
        {
            _name = scriptname;
            _filename = filename;
            _tree = tree;
        }

        public bool Loaded { get; private set; }

        public void Load()
        {

        }

        public void Unload()
        {

        }

        public void Run()
        {

        }
    }
}
