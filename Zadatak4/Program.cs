using DokanNet;
using System;
using System.Collections.Generic;
using Zadatak4.BTreeDictionary;
using Zadatak4.BPath;
//using Zadatak4.BTree;

namespace Zadatak4
{
    class Program
    {
        static void Main(string[] args)
        {
            const string path = @"Y:\";
             new MSFS(path).Mount(path, DokanOptions.DebugMode | DokanOptions.StderrOutput);
            /*BTreeDictionary<Path, File> files = new BTreeDictionary<Path, File>(3);
            files.Add(new Path() { FilePath = "C:\\Users\\Marko\\Downloads\\b-tree\\Collections\\Generics" }, new File());
            files.Add(new Path() { FilePath = "C:\\Asers\\Marko\\Downloads\\b-tree\\Collections\\Generics" }, new File());
            files.Add(new Path() { FilePath = "C:\\Bsers\\Marko\\Downloads\\b-tree\\Collections\\Generics" }, new File());
            files.Add(new Path() { FilePath = "C:\\Xsers\\Marko\\Downloads\\b-tree\\Collections" }, new File());*/


        }
    }
}
