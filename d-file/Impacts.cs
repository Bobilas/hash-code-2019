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
        public static Dictionary<int, long> TagImpacts = new Dictionary<int, long>();
        private static Dictionary<int, long> _tagWeights = new Dictionary<int, long>();
        public static Dictionary<long, List<int>> TagsByImpacts = new Dictionary<long, List<int>>();

        public static void UpdateImpacts(int slideId)
        {
            foreach (var tag in Slides[slideId].Tags)
            {
                _tagWeights[tag]--;
                foreach (int id in SlidesByTags[tag])
                {
                    if (!AddedSlides.ContainsKey(id))
                    {
                        var slide = Slides[id];
                        slide.Impact--;
                        Slides[id] = slide;
                        //foreach (string t in Slides[id].Tags)
                        //{
                        //    TagImpacts[tag]--;
                        //}
                    }
                }
            }
        }

        public static void CalculateInitialImpacts()
        {
            getTagWeights();
            getSlideImpacts();
            getTagImpacts();

            void getTagWeights()
            {
                _tagWeights.Clear();
                foreach (var tag in SlidesByTags.Keys)
                {
                    _tagWeights.Add(tag, SlidesByTags[tag].Count);
                }
            }
            void getSlideImpacts()
            {
                for (int i = 0; i < Slides.Count; i++)
                {
                    var slide = Slides[i];
                    long impact = 1;
                    foreach (ushort tag in slide.Tags)
                    {
                        impact += _tagWeights[tag];
                    }
                    slide.Impact = impact;
                    Slides[i] = slide;
                }
            }
            void getTagImpacts()
            {
                TagImpacts.Clear();
                foreach (var tag in SlidesByTags.Keys)
                {
                    long impact = 0;
                    foreach (int id in SlidesByTags[tag])
                    {
                        impact += Slides[id].Impact;
                    }
                    TagImpacts.Add(tag, impact);
                }
            }
        }

        public static void SortAllByImpact()
        {
            foreach (var tag in SlidesByTags.Keys)
            {
                SlidesByTags[tag].Sort((x, y) =>
                    Slides[x].Impact
                    .CompareTo(Slides[y].Impact));
            }

            foreach (var tag in TagImpacts.Keys)
            {
                if (!TagsByImpacts.ContainsKey(TagImpacts[tag]))
                {
                    TagsByImpacts.Add(TagImpacts[tag], new List<int>() { tag });
                }
                else
                {
                    TagsByImpacts[TagImpacts[tag]].Add(tag);
                }
            }

            for (int i = 0; i < Slides.Count; i++)
            {
                var slide = Slides[i];
                var a = Slides[i].Tags.ToList();
                a.Sort((y, x) => TagImpacts[x].CompareTo(TagImpacts[y]));
                slide.Tags = a.ToArray();
                Slides[i] = slide;
            }
        }
    }
}
