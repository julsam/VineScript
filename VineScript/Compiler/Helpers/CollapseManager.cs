using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VineScript.Compiler
{
    /// <summary>
    /// Manage collapsing text when using the Collapse Statement (with '{' and '}').
    /// </summary>
    internal class CollapseManager
    {
        private string textBuffer = "";
        private int pausedDepth;
        private int _depth = 0;
        // Depth is used in case of nested collapse
        public int Depth {
            get {
                return _depth;
            }
            set {
                _depth = value < 0 ? 0 : value;
                if (_depth > 0) {
                    IsCollapsed = true;
                } else {
                    IsCollapsed = false;
                }
            }
        }

        private bool _collapsed = false;
        public bool IsCollapsed {
            get {
                return _collapsed;
            }
            private set {
                if (value != _collapsed)
                {
                    _collapsed = value;
                    if (_collapsed == false) {
                        // flush buffer if it's not collapsed anymore
                        if (flush != null) {
                            flush(textBuffer);
                            textBuffer = "";
                        }
                    }
                }
            }
        }

        private Action<string> flush;

        public CollapseManager(Action<string> flush)
        {
            this.flush = flush;
        }

        public void Pause()
        {
            pausedDepth = Depth;
            Depth = 0;
        }

        public void Resume()
        {
            Depth = pausedDepth;
        }

        public void Stop()
        {
            Depth = 0;
        }

        public void AddText(string text)
        {
            if (Depth <= 0) {
                throw new Exception("CollapseManager: adding text while not in collapse mode");
            }
            textBuffer += text;
        }
    }
}
