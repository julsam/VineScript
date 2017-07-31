using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VineScript.Core;

namespace VineScript.Test
{
    class Utils
    {
        public static void CompareFile(string vinefile)
        {
            string cmpfile = Path.ChangeExtension(vinefile, "cmp");

            StreamReader input = File.OpenText(vinefile);
            StreamReader cmp = File.OpenText(cmpfile);

            VineStory story = new VineStory();
            string output = story.RunPassage(input).text;

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }
    }
}
