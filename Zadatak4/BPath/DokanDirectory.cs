using System;
using System.Collections.Generic;
using System.Text;

namespace Zadatak4.BPath
{
    [Serializable]
    class DokanDirectory : DokanItem
    {
        List<DokanItem> children = new List<DokanItem>();

        public DokanDirectory(DokanDirectory parent, string name) : base(parent, name)
        {
            Attributes = System.IO.FileAttributes.Directory;
        }

        public DokanDirectory() : base(null, "") 
        {
            Attributes = System.IO.FileAttributes.Directory;
        }

        public List<string> Content { get; set; } = new List<string>();
    }
}
