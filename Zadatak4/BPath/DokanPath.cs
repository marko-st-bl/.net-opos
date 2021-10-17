using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Zadatak4.BPath
{
    [Serializable]
    public class DokanPath : IComparable<DokanPath>
    {
        public string FilePath { get; set;}
        public bool IsDirectory { get; set; }

        public int Depth => FilePath.Split("\\").Length - 1;

        public int CompareTo([AllowNull] DokanPath other)
        {
            if (this.Depth < other.Depth)
                return -1;
            else if (this.Depth < other.Depth)
                return 1;
            else
                return FilePath.CompareTo(other.FilePath);
        }
    }
}
