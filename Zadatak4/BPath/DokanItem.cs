using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Zadatak4.BPath
{
    [Serializable]
    class DokanItem
    {
        public DateTime LastAccessTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime CreationTime { get; set; }
        public FileAttributes Attributes { get; set; }
        public string Name { get; set; }

        public DokanItem(DokanDirectory parent, string name)
        {
            Name = name;
            CreationTime = DateTime.Now;
            LastAccessTime = DateTime.Now;
            LastWriteTime = DateTime.Now;
        }
    }
}
