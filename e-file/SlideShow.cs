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
        public static List<Slide> Slides;
        public static Dictionary<int, byte> AddedSlides = new Dictionary<int, byte>();
        public static List<int> TransitionScores = new List<int>();
        public static List<int> Slideshow = new List<int>();
        public static List<int> TempSlides;
        public static Dictionary<int, List<int>> SlidesByTags = new Dictionary<int, List<int>>();

        public static void AddSlide(int id)
        {
            //UpdateImpacts(id);
            AddedSlides.Add(id, 0);
            TempSlides.Remove(id);
            Slideshow.Add(id);
            foreach (var tag in Slides[id].Tags)
            {
                SlidesByTags[tag].Remove(id);
            }
            if (Slideshow.Count > 1)
            {
                TransitionScores.Add(Slides[Slideshow[Slideshow.Count - 1]].GetScore(Slideshow[Slideshow.Count - 2]));
            }
        }

        public static void MakeSlideshow()
        {
            TransitionScores.Clear();
            Slideshow.Clear();
            TempSlides = Enumerable.Range(0, Slides.Count).ToList();
            TempSlides.Sort((y, x) => Slides[x].Impact.CompareTo(Slides[y].Impact));

            while (true)
            {
                TempSlides.Sort((y, x) => Slides[x].Impact.CompareTo(Slides[y].Impact));
                if (TempSlides.Count == 1)
                {
                    AddSlide(TempSlides[0]);
                    break;
                }
                else if (TempSlides.Count == 0)
                {
                    break;
                }

                int lastId = 0;
                bool addedOne = false;
                while (true)
                {
                    //string bestTag = Slides[lastId].Tags[0];
                    //foreach (string tag in Slides[lastId].Tags)
                    //{
                    //    if (_tagWeights[tag] < _tagWeights[bestTag])
                    //    {
                    //        bestTag = tag;
                    //    }
                    //}
                    int bestId = -1;
                    int bestUnion = 0;
                    //foreach (int id in SlidesByTags[bestTag])
                    //{
                    //    if (!AddedSlides.ContainsKey(id))
                    //    {
                    //        if (bestId == -1)
                    //        {
                    //            bestId = id;
                    //            bestUnion = Slides[id].Tags.Union(Slides[lastId].Tags).Count();
                    //        }
                    //        else
                    //        {
                    //            int union = Slides[id].Tags.Union(Slides[lastId].Tags).Count();
                    //            if (union < bestUnion)
                    //            {
                    //                bestId = id;
                    //                bestUnion = union;
                    //            }
                    //        }
                    //    }
                    //}

                    int bestScore = 0;
                    int bestCount = 0;
                    if (bestId == -1)
                    {
                        foreach (var tag in Slides[lastId].Tags)
                        {
                            foreach (int id in SlidesByTags[tag])
                            {
                                if (!AddedSlides.ContainsKey(id))
                                {
                                    if (bestId == -1)
                                    {
                                        bestId = id;
                                        bestScore = (int)(Slides[id].GetScore(lastId));
                                    }
                                    else
                                    {
                                        int sc = (int)(Slides[id].GetScore(lastId));
                                        if (sc > bestScore)
                                        {
                                            bestId = id;
                                            bestScore = sc;
                                            bestCount = 0;
                                        }
                                        else
                                        {
                                            bestCount++;
                                        }
                                    }
                                }

                                //if (bestCount >= 200)
                                //{
                                //    break;
                                //}
                            }

                            //if (bestCount >= 200)
                            //{
                            //    break;
                            //}
                        }
                    }


                    if (bestId == -1)
                    {
                        break;
                    }
                    else
                    {
                        lastId = bestId;
                        AddSlide(lastId);
                        addedOne = true;
                        if (Slideshow.Count % 100 == 0)
                        {
                            Watch($"Slide count is now {Slideshow.Count}, score is now {TransitionScores.Sum()}");
                        }
                    }
                }
                if (!addedOne && TempSlides.Count > 0)
                {
                    AddSlide(TempSlides[0]);
                    //Watch($"Failed chain at {Slideshow.Count} slide");
                }
            }

            int score = TransitionScores.Sum();
            Console.WriteLine($"Score: {score}");
        }

        public static int GetBestId(int scoreIn, int[] usedIds, out int scoreOut, int count = 0)
        {
            count++;
            int usedId = usedIds[usedIds.Length - 1];

            var ids = new Dictionary<int, byte>();
            int a = 0;
            foreach (ushort tag in Slides[usedId].Tags)
            {
                foreach (int id in SlidesByTags[tag])
                {
                    var slide = Slides[id];
                    
                    if (!AddedSlides.ContainsKey(id) && !ids.ContainsKey(id) && !usedIds.Contains(id))
                    {
                        ids.Add(id, 0);
                        a++;

                        if (a >= 2)
                        {
                            break;
                        }
                    }
                }
                if (a >= 2)
                {
                    break;
                }
            }

            var scores = new int[ids.Count];
            var bestIds = new int[ids.Count];
            var asdIds = ids.Keys.ToArray();

            if (count <= 1)
            {
                //UpdateImpacts(usedId, -1);
                for (int i = 0; i < asdIds.Length; i++)
                {
                    int tempScore = GetInterestFactor(Slides[asdIds[i]], Slides[usedId]);
                    int tempImpact = tempScore;//(tempScore << 20) / Slides[asdIds[i]].Impact;

                    if (tempScore > 0)
                    {
                        bestIds[i] = GetBestId(scoreIn + tempImpact, usedIds.Concat(new int[] { asdIds[i] }).ToArray(), out scores[i], count);
                    }
                    else
                    {
                        scores[i] = int.MinValue;
                    }
                }
                //UpdateImpacts(usedId, +1);

                int bestIndex = scores.Length > 0 ? 0 : -1;
                int bestScore = int.MinValue;
                for (int i = 0; i < scores.Length; i++)
                {
                    if (scores[i] > bestScore)
                    {
                        bestIndex = i;
                        bestScore = scores[i];
                    }
                }
                if (scores.Length == 0)
                {
                    bestScore = scoreIn;
                }

                scoreOut = bestScore + scoreIn;
                return bestIndex >= 0 ? bestIds[bestIndex] : -1;
            }
            else
            {
                scoreOut = scoreIn;
                return usedIds[1];
            }
        }

        public static void GetSlides()
        {
            Slides = Photos_H.Select(e => new Slide(Photos[e].Tags, Photos[e].Id)).ToList();
            Slides.AddRange(JoinVerticalPhotos().ToArray());
        }

        public static IEnumerable<Slide> JoinVerticalPhotos()
        {
            var tempPhotos = Photos_V.ToList();
            tempPhotos.Sort((x, y) => Photos[x].Tags.Length.CompareTo(Photos[y].Tags.Length));

            while (tempPhotos.Count >= 2)
            {
                var b = tempPhotos.Count - 1;
                int bestUnion = getUnionScore(Photos[tempPhotos[0]].Tags, Photos[tempPhotos[b]].Tags);
                for (int i = tempPhotos.Count - 2; i >= 2 && i >= (tempPhotos.Count - 10); i--)
                {
                    int union = getUnionScore(Photos[tempPhotos[0]].Tags, Photos[tempPhotos[i]].Tags);
                    if (union > bestUnion)
                    {
                        b = i;
                        bestUnion = union;
                    }
                }
                yield return new Slide(Photos[tempPhotos[0]].Tags.Union(Photos[tempPhotos[b]].Tags).ToArray(), tempPhotos[0], tempPhotos[b]);
                tempPhotos.RemoveAt(b);
                tempPhotos.RemoveAt(0);
            }

            int getUnionScore(int[] a, int[] b)
            {
                return getUnionCount() - getIntersectionCount();

                int getUnionCount()
                {
                    return a.Union(b).Count();
                }
                int getIntersectionCount()
                {
                    return a.Intersect(b).Count();
                }
            }
        }

        static void GetDictionaries()
        {
            for (int i = 0; i < Slides.Count; i++)
            {
                var slide = Slides[i];
                foreach (ushort tag in slide.Tags)
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
