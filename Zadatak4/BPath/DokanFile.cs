using System;
using System.Collections.Generic;
using System.Text;

namespace Zadatak4.BPath
{
    [Serializable]
    class DokanFile : DokanItem
    {
        public DokanFile(DokanDirectory parent, string name) : base(parent, name)
        {
            Attributes = System.IO.FileAttributes.Normal;
        }

        public DokanFile() : base(null, "")
        {
            Attributes = System.IO.FileAttributes.Normal;
        }

        public byte[] Bytes { get; set; } = new byte[0];

    }
}
