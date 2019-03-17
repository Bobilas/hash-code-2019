using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HashCode
{
    public struct Photo
    {
        public string[] Tags;
        public bool IsVertical;
        public int Id;

        public Photo(int id, string[] tags, bool isVertical)
        {
            Id = id;
            Tags = tags;
            IsVertical = isVertical;
        }
    }

    public struct Slide
    {
        public string[] Tags;
        public int[] Ids;
        public long Impact;

        public Slide(string[] tags, params int[] ids)
        {
            Tags = tags;
            Ids = ids;
            Impact = 0;
        }
    }
}
