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
        public static int GetInterestFactor(Slide slideA, Slide slideB)
        {
            string[] 
                a = slideA.Tags,
                b = slideB.Tags;
            string A = string.Concat(a.Select(e => e + " "));
            string B = string.Concat(b.Select(e => e + " "));

            int[] uniqueTags = getUniqueTagCount();
            int commonTags = getCommonTagCount();

            int score = Math.Min(
                Math.Min(uniqueTags[0], uniqueTags[1]), 
                commonTags);

            return score;

            int[] getUniqueTagCount()
            {
                return new int[]
                {
                    a.Except(b).Count(),
                    b.Except(a).Count()
                };
            }
            int getCommonTagCount()
            {
                return a.Intersect(b).Count();
            }
        }

        public static int GetFullScore(List<Slide> allSlides)
        {
            int sum = 0;
            for (int i = 0; i < allSlides.Count() - 1; i++)
            {
                sum += GetInterestFactor(allSlides[i], allSlides[i + 1]);
            }

            return sum;
        }
    }
}
