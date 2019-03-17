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
    partial class Program {
        public struct Photo
        {
            public int[] Tags;
            public bool IsVertical;
            public int Id;

            public Photo(int id, int[] tags, bool isVertical)
            {
                Id = id;
                Tags = tags;
                IsVertical = isVertical;
            }
        }

        public struct Slide
        {
            public int[] Tags;
            public ulong[] TagsB;
            public int[] Ids;
            public long Impact;

            public Slide(int[] tags, params int[] ids)
            {
                Tags = tags;
                Ids = ids;
                Impact = 0;
                TagsB = new ulong[6];
                for (int i = 0; i < tags.Length; i++)
                {
                    int shiftCount = tags[i] % 64;
                    TagsB[tags[i] / 64] |= (ulong)1 << shiftCount;
                }
            }

            public int GetScore(int id)
            {
                int a = 0;
                for (int i = 0; i < 4; i++)
                {
                    a += numberOfSetBits(TagsB[i] & Slides[id].TagsB[i]);
                }
                return
                    Tags.Length < Slides[id].Tags.Length
                    ? Tags.Length - a < a
                    ? Tags.Length - a
                    : a
                    : Slides[id].Tags.Length - a < a
                    ? Slides[id].Tags.Length - a
                    : a;

                int numberOfSetBits(ulong num)
                {
                    num = num - ((num >> 1) & 0x5555555555555555UL);
                    num = (num & 0x3333333333333333UL) + ((num >> 2) & 0x3333333333333333UL);
                    return (int)(unchecked(((num + (num >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
                }
            }
        }
    }
}
