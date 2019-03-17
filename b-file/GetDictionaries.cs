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
    partial class Program
    {
        static void GetDictionaries()
        {
            for (int i = 0; i < Slides.Count; i++)
            {
                var slide = Slides[i];
                foreach (string tag in slide.Tags)
                {
                    if (!SlidesByTags.ContainsKey(tag))
                    {
                        SlidesByTags.Add(tag, new List<int>() { i });
                    }
                    else
                    {
                        SlidesByTags[tag].Add(i);
                    }
                }
            }
        }
    }
}
