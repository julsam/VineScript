using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VineScript.Compiler
{
    internal class ParserOutputBuilder
    {
        private enum ChunkType {
            Default,
            Verbatim,
            Collapsed,
            VerbatimSurroundedByCollapsed
        }

        private class Chunk
        {
            public readonly ChunkType type;
            public readonly string text;

            public Chunk(string text, ChunkType type)
            {
                this.text = text;
                this.type = type;
            }
        }

        public bool IsCollapsed {
            get {
                return collapseMgr.IsCollapsed;
            }
        }

        private List<Chunk> chunks = new List<Chunk>();
        private CollapseManager collapseMgr;

        public ParserOutputBuilder()
        {
            collapseMgr = new CollapseManager(PushCollapsed);
        }

        public void EnterCollapse()
        {
            collapseMgr.Depth++;
        }

        public void ExitCollapse()
        {
            collapseMgr.Depth--;
        }
        
        /// <summary>
        /// Called back by CollapseManager when its buffer is flushed.
        /// </summary>
        /// <param name="text"></param>
        private void PushCollapsed(string text)
        {
            chunks.Add(new Chunk(text, ChunkType.Collapsed));
        }

        public void PushText(string text)
        {
            if (collapseMgr.IsCollapsed) {
                collapseMgr.AddText(text);
            } else {
                chunks.Add(new Chunk(text, ChunkType.Default));
            }
        }

        public void PushVerbatim(string verbatim)
        {
            if (collapseMgr.IsCollapsed) {
                // Verbatim in a collapsed block can't be collapsed.
                // 1. So we need first to disable the collapse and flush the text
                collapseMgr.Pause();
                // 2. Then add the verbatim. There's some whitespace processing
                // to do before and after inserting it
                chunks.Add(new Chunk(verbatim, ChunkType.VerbatimSurroundedByCollapsed));
                // 3. Go back to collapsed mode
                collapseMgr.Resume();
            } else {
                // Add the verbatim, nothing special to do
                chunks.Add(new Chunk(verbatim, ChunkType.Verbatim));
            }
        }

        public void PushStmt(string stmt)
        {
            // When it's not collapsed, we need to keep track of the position
            // and type of stmt in order to be able to format in a specific
            // way the text preceding or following that stmt.
            // But when it's collapsed, the formatting is disabled, so we
            // don't need to add that stmt to the result.
            if (!collapseMgr.IsCollapsed) {
                chunks.Add(new Chunk(stmt, ChunkType.Default));
            }
        }

        public string Build()
        {
            if (collapseMgr.IsCollapsed) {
                collapseMgr.Stop();
            }

            if (chunks.Count <= 0) {
                return "";
            }

            string str = "";

            // First PassageText
            str += chunks[0].text;
            Chunk prev = chunks[0];

            for (int i = 1; i < chunks.Count; i++)
            {
                Chunk ptext = chunks[i];
                switch (ptext.type)
                {
                    case ChunkType.Default:
                    case ChunkType.Verbatim:
                        // Add it as it is
                        str += ptext.text;
                        break;
                    case ChunkType.Collapsed:
                        // remove extra space between words and remove line returns
                        str += WhiteSpace.CollapseWordsSpacingLn(ptext.text);
                        break;
                    case ChunkType.VerbatimSurroundedByCollapsed:
                        // 1. First need to check if there is a white space at the end of
                        // the previous PassageText. If it's a collapsed type, that white
                        // space will be removed, so we need to add it back here
                        if (prev.type == ChunkType.Collapsed)
                        {
                            if (prev.text.Length > 0) {
                                char lastChar = prev.text[prev.text.Length - 1];
                                if (char.IsWhiteSpace(lastChar)) {
                                    str += lastChar.ToString();
                                }
                            }
                        }

                        // 2. Add the verbatim
                        str += ptext.text;

                        // 3. Same as 1., but this time we check white space at
                        // the start of the next PassageText.
                        Chunk next = i + 1 < chunks.Count ? chunks[i + 1] : null;
                        if (next != null && next.type == ChunkType.Collapsed)
                        {
                            if (next.text.Length > 0 && !WhiteSpace.IsOnlyWhiteSpace(next.text))
                            {
                                char firstChar = next.text[0];
                                if (char.IsWhiteSpace(firstChar)) {
                                    str += firstChar.ToString();
                                }
                            }
                        }
                        break;
                }
                
                prev = chunks[i];
            }

            return str;
        }
    }
}
