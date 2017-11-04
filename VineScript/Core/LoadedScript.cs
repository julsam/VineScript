using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;

namespace VineScript.Core
{
    class LoadedScript
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
    }
}
