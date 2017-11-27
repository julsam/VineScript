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
        
        public PassageScript(string scriptname, string filename)
        {
            _name = scriptname;
            _filename = filename;
        }

        public bool Loaded {
            get {
                return _tree != null;
            }
        }

        public bool Load(ParserRuleContext tree)
        {
            _tree = tree;
            return true;
        }

        public void Unload()
        {
            _tree = null;
        }

        public void Run()
        {
            throw new NotImplementedException();
        }
    }
}
