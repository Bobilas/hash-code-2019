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
        static Stopwatch _watch = new Stopwatch();
        static void Watch(string infoText)
        {
            _watch.Stop();
            long time = _watch.ElapsedMilliseconds;
            Console.WriteLine($"{infoText} ... {(time >= 10_000 ? (time / 1000.0) : time)}{(time < 10_000 ? "m" : "")}s");
            _watch.Restart();
        }

        public static List<Photo> Photos;
        public static List<int> Photos_H;
        public static List<int> Photos_V;

        public static List<Slide> Slides;
        public static List<int> TempSlides;
        public static Dictionary<string, List<int>> SlidesByTags = new Dictionary<string, List<int>>();

        public static Dictionary<string, int> TagImpacts = new Dictionary<string, int>();

        public static List<Slide> Slideshow = new List<Slide>();

        [STAThread]
        static void Main(string[] args)
        {
            var dialog = new OpenFileDialog();
            string filePath;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                filePath = dialog.FileName;
            }
            else
            {
                return;
            }
            _watch.Start();
            (Photos, Photos_H, Photos_V) = FileReader.ParseFile(filePath);
            Watch($"Parsed file. Photo count is {Photos_H.Count}H {Photos_V.Count}V");
            GetSlides();
            Watch("Got all slides");
            GetDictionaries();
            Watch($"Got slides by tags. Tag count is {SlidesByTags.Count}");
            GetTagImpacts();
            Watch("Got tag impacts");
            GetSlideImpacts();
            Watch("Got slice impacts");
            SortSlidesByImpact();
            Watch("Sorted slides by impacts");
            MakeSlideshow();
            Watch("Got slideshow");

            var line = new StringBuilder();
            line.AppendLine($"{Slideshow.Count}");
            foreach (var slide in Slideshow)
            {
                line.AppendLine(string.Concat(slide.Ids.Select(e => e.ToString() + " ")));
            }
            File.WriteAllText($"{filePath.Replace(".txt",".out")}", line.ToString());

            Watch("Wrote output file");

            while (Console.ReadKey().Key != ConsoleKey.Q);
        }



        private static Dictionary<int, byte> _addedSlides = new Dictionary<int, byte>();
        public static void MakeSlideshow()
        {
            TempSlides = Enumerable.Range(0, Slides.Count).ToList();
            int previousAddedCount = 0;
            int transitionCount = 0;

            while (true)
            {
                TempSlides.Sort((y, x) => Slides[x].Impact.CompareTo(Slides[y].Impact));
                if (TempSlides.Count == 1)
                {
                    _addedSlides.Add(TempSlides[0], 0);
                    TempSlides.Remove(TempSlides[0]);
                    break;
                }

                int lastId = TempSlides[0];
                GetBestId(0, new int[] { TempSlides[0] }, out int bestImpact);
                int count = 0;
                foreach (int id in TempSlides)
                {
                    GetBestId(0, new int[] { id }, out int impact);
                    if (impact > bestImpact)
                    {
                        bestImpact = impact;
                        lastId = id;
                    }
                    count++;
                    if (count >= 1000)
                    {
                        break;
                    }
                }
                while (true)
                {
                    int bestId = GetBestId(0, new int[] { lastId }, out bestImpact);

                    if (bestId != -1)
                    {
                        lastId = bestId;
                        UpdateImpacts(bestId);
                        _addedSlides.Add(bestId, 0);
                        TempSlides.Remove(bestId);
                    }
                    else
                    {
                        if (!_addedSlides.ContainsKey(lastId))
                        {
                            _addedSlides.Add(lastId, 0);
                            TempSlides.Remove(lastId);
                        }
                        break;
                    }
                }

                if (TempSlides.Count == 0)
                {
                    break;
                }

                if (_addedSlides.Count > previousAddedCount + 1)
                {
                    transitionCount += _addedSlides.Count - previousAddedCount - 1;
                    Console.WriteLine(
                        $"Chained transitions: " +
                        $"{(_addedSlides.Count - previousAddedCount - 1).ToString().PadLeft(5)} " +
                        $"(total: {transitionCount.ToString().PadLeft(5)} " +
                        $"@{_addedSlides.Count.ToString().PadLeft(5)})");
                }
                previousAddedCount = _addedSlides.Count;
            }

            Slideshow = _addedSlides.Keys.Select(e => Slides[e]).ToList();
            int score = GetFullScore(Slideshow);

            Console.WriteLine($"Score: {score}");
        }

        public static int GetBestId(int scoreIn, int[] usedIds, out int scoreOut, int count = 0)
        {
            count++;
            int usedId = usedIds[usedIds.Length - 1];

            var ids = new Dictionary<int, byte>();
            foreach (string tag in Slides[usedId].Tags)
            {
                foreach (int id in SlidesByTags[tag])
                {
                    var slide = Slides[id];

                    if (!_addedSlides.ContainsKey(id) && !ids.ContainsKey(id) && !usedIds.Contains(id))
                    {
                        ids.Add(id, 0);
                    }
                }
            }

            var scores = new int[ids.Count];
            var bestIds = new int[ids.Count];
            var asdIds = ids.Keys.ToArray();

            if (count <= 1)
            {
                UpdateImpacts(usedId, -1);
                for (int i = 0; i < asdIds.Length; i++)
                {
                    int tempScore = GetInterestFactor(Slides[asdIds[i]], Slides[usedId]);
                    int tempImpact =  (tempScore << 10) / Slides[asdIds[i]].Impact;

                    if (tempScore > 0)
                    {
                        bestIds[i] = GetBestId(scoreIn + tempImpact, usedIds.Concat(new int[] { asdIds[i] }).ToArray(), out scores[i], count);
                    }
                    else
                    {
                        scores[i] = int.MinValue;
                    }
                }
                UpdateImpacts(usedId, +1);

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
        }

        private static Dictionary<int, byte> _slidesUpdated = new Dictionary<int, byte>();
        public static void UpdateTagImpacts(string[] tags, int change)
        {
            foreach (string tag in tags)
            {
                TagImpacts[tag] += change;
            }
        }
        public static void UpdateImpacts(int slideId, int tagChange = -1)
        {
            UpdateTagImpacts(Slides[slideId].Tags, tagChange);

            _slidesUpdated.Clear();
            foreach (string tag in Slides[slideId].Tags)
            {
                foreach (int id in SlidesByTags[tag])
                {
                    if (!_slidesUpdated.ContainsKey(id) && !_addedSlides.ContainsKey(id))
                    {
                        UpdateSlideImpact(id);
                        _slidesUpdated.Add(id, 0);
                    }
                }
            }
        }

        public static void UpdateSlideImpact(int id)
        {
            var slide = Slides[id];

            int impact = -slide.Tags.Length;
            foreach (string tag in slide.Tags)
            {
                impact += TagImpacts[tag];
            }
            impact = (impact << 4) / slide.Tags.Length;
            slide.Impact = Math.Max(impact, 1);
            Slides[id] = slide;
        }

        public static void GetSlideImpacts()
        {
            for (int i = 0; i < Slides.Count; i++)
            {
                UpdateSlideImpact(i);
            }

            // NEW (VERY SLOW) IMPACTS
            {
                //_tempIdList.Clear();
                //for (int i = 0; i < Photos_H.Count; i++)
                //{
                //    var photo = Photos[Photos_H[i]];
                //    foreach (string tag in photo.Tags)
                //    {
                //        if (PhotosByTags_H.ContainsKey(tag))
                //        {
                //            foreach (int id in PhotosByTags_H[tag])
                //            {
                //                _tempIdList.Add(id);
                //            }
                //        }
                //        if (PhotosByTags_V.ContainsKey(tag))
                //        {
                //            foreach (int id in PhotosByTags_V[tag])
                //            {
                //                _tempIdList.Add(id);
                //            }
                //        }
                //    }

                //    photo.Impact = _tempIdList.Distinct().Count();
                //    Photos[Photos_H[i]] = photo;

                //    if (!PhotoImpacts.ContainsKey(photo.Impact))
                //    {
                //        PhotoImpacts.Add(photo.Impact, new List<int>() { photo.Id });
                //    }
                //    else
                //    {
                //        PhotoImpacts[photo.Impact].Add(photo.Id);
                //    }
                //}

                //_tempIdList.Clear();
                //for (int i = 0; i < Photos_V.Count; i++)
                //{
                //    var photo = Photos[Photos_V[i]];
                //    foreach (string tag in photo.Tags)
                //    {
                //        if (PhotosByTags_H.ContainsKey(tag))
                //        {
                //            foreach (int id in PhotosByTags_H[tag])
                //            {
                //                _tempIdList.Add(id);
                //            }
                //        }
                //        if (PhotosByTags_V.ContainsKey(tag))
                //        {
                //            foreach (int id in PhotosByTags_V[tag])
                //            {
                //                _tempIdList.Add(id);
                //            }
                //        }
                //    }

                //    photo.Impact = _tempIdList.Distinct().Count();
                //    Photos[Photos_V[i]] = photo;

                //    if (!PhotoImpacts.ContainsKey(photo.Impact))
                //    {
                //        PhotoImpacts.Add(photo.Impact, new List<int>() { photo.Id });
                //    }
                //    else
                //    {
                //        PhotoImpacts[photo.Impact].Add(photo.Id);
                //    }
                //}
            }
        }

        public static void GetTagImpacts()
        {
            TagImpacts.Clear();
            foreach (string tag in SlidesByTags.Keys)
            {
                TagImpacts.Add(tag, SlidesByTags[tag].Count);
            }
        }

        public static void SortSlidesByImpact()
        {
            foreach (string tag in SlidesByTags.Keys)
            {
                SlidesByTags[tag].Sort((x, y) =>
                    Slides[x].Impact
                    .CompareTo(Slides[y].Impact));
            }
        }
    }
}
