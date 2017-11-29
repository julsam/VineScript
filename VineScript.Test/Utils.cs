using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VineScript.Core;

namespace VineScript.Test
{
    class Utils
    {
        public static void CompareFile(string input)
        {
            StreamReader cmp = File.OpenText(input + ".cmp");
            
            Loader loader = new Loader("");
            loader.LoadFile(input + ".vine");
            VineStory story = new VineStory(loader);
            string output = story.RunPassage(input).text;

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }
    }
}
