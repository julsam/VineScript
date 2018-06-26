using System;
using System.Collections.Generic;
using System.Linq;
using VineScript.Binding;
using VineScript.Core;

namespace VineScript.Lib
{
    public class StoryState
    {
        private VineStory storyRef;

        public StoryState(VineStory story)
        {
            storyRef = story;
        }

        /// <summary>
        /// Returns an array of the previous passages.
        /// </summary>
        [VineBinding]
        public VineVar History()
        {
            return storyRef.history.Select(p => p.PassageName).ToList();
        }

        /// <summary>
        /// Returns the name of the current passage / script.
        /// </summary>
        [VineBinding]
        public VineVar CurrentPassage()
        {
            return storyRef.currentScript.Name;
        }
    }
}
