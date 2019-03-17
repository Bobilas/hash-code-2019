using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashCode
{
    partial class Program
    {
        public static (List<Photo> AllPhotos, List<int> H, List<int> V) ParseFile(string filePath)
        {
            var addedTags = new Dictionary<string, int>();

            string allText = File.ReadAllText(filePath);
            string[] lines = allText.Split('\n', '\r').Where(e => !string.IsNullOrEmpty(e)).ToArray();
            uint photoCount = uint.Parse(lines[0]);
            var photos = parsePhotos().ToList();
            return (
                photos,
                photos.Where(e => !e.IsVertical).Select(e => e.Id).ToList(),
                photos.Where(e => e.IsVertical).Select(e => e.Id).ToList());

            IEnumerable<Photo> parsePhotos()
            {
                for (int i = 1; i <= photoCount; i++)
                {
                    string[] values = lines[i].Split(' ');
                    bool isVertical = false;
                    switch (values[0])
                    {
                        case "V":
                            isVertical = true;
                            break;
                        default:
                            break;
                    }
                    int tagCount = int.Parse(values[1]);
                    var tagsToAdd = new List<int>();
                    foreach (string tag in values.Skip(2).Take(tagCount).ToArray())
                    {
                        if (!addedTags.ContainsKey(tag))
                        {
                            addedTags.Add(tag, addedTags.Count);
                        }
                        tagsToAdd.Add(addedTags[tag]);
                    }
                    yield return new Photo(i - 1, tagsToAdd.ToArray(), isVertical);
                }
            }
        }

        public static void WriteOutut(string filePath)
        {
            var line = new StringBuilder();
            line.AppendLine($"{Slideshow.Count}");
            foreach (var id in Slideshow)
            {
                line.AppendLine(string.Concat(Slides[id].Ids.Select(e => e.ToString() + " ")));
            }
            File.WriteAllText($"{filePath.Replace(".txt", ".out")}", line.ToString());
        }
    }
}
