using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VineScript.Core;

namespace VineScriptTest
{
    class LoaderTest
    {
        static void FileLoad1()
        {
            Loader loader = new Loader("./scripts");
            loader.LoadFromDir("loader", "*.vine|*");
            
            // file2.vine
            Assert.IsNotNull(loader.Get("file2"));
            // file3
            Assert.IsNotNull(loader.Get("file3"));
            // file4.txt
            Assert.IsNotNull(loader.Get("file4"));
            
            // sub1/file1.vine
            Assert.IsNotNull(loader.Get(@"sub1/file1"));
            Assert.IsNull(loader.Get(@"sub1\file1"));

            loader.RemoveAll();
        }

        static void FileLoad2()
        {
            Loader loader = new Loader("./scripts/loader");
            loader.LoadFile("file1.vine");
            
            loader.RemoveAll();
        }

        static void FileLoad3()
        {
            Loader loader = new Loader("./scripts/loader");
            loader.LoadFile("sub1/file1.vine", "My Renamed File");
            
            Assert.IsNotNull(loader.Get("My Renamed File"));
            
            loader.RemoveAll();
        }

        static void FileLoad4()
        {
            // Loader is automatically created in Story if not provided.
            // Default base directory is ./
            VineStory story = new VineStory();
            story.RunPassage("scripts/loader_tests/file1");
        }

        static void FileLoad5()
        {
            // Loader is automatically created in Story if not provided.
            VineStory story = new VineStory("scripts/loader_tests");
            story.RunPassage("file1");
        }

        static void CodeLoad1()
        {
            StreamReader istream = File.OpenText(@"./scripts/loader/file1.vine");
            Loader loader = new Loader();
            loader.LoadCode(istream.ReadToEnd(), "foobar");

            loader.RemoveAll();
        }
    }
}
