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
            int[] 
                a = slideA.Tags,
                b = slideB.Tags;

            int[] uniqueTags = getUniqueTagCount();
            int commonTags = getCommonTagCount();

            return getMin(uniqueTags[0], uniqueTags[1], commonTags);

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
            int getMin(params int[] values)
            {
                int min = values[0];
                for (int i = 1; i < values.Length; i++)
                {
                    min = Math.Min(min, values[i]);
                }
                return min;
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
