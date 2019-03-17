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
            (Photos, Photos_H, Photos_V) = ParseFile(filePath);
            Watch($"Parsed file. Photo count is {Photos_H.Count}H {Photos_V.Count}V");
            GetSlides();
            Watch($"Got all slides. Slide count is {Slides.Count}");
            GetDictionaries();
            Watch($"Got slides by tags. Tag count is {SlidesByTags.Count}");
            CalculateInitialImpacts();
            Watch($"Calculated weights and impacts");
            SortAllByImpact();
            Watch($"Sorted slides by impacts. Unique tag impact count is {TagsByImpacts.Count}.");
            MakeSlideshow();
            Watch("Got slideshow");
            WriteOutut(filePath);
            Watch("Wrote output file");

            while (Console.ReadKey().Key != ConsoleKey.Q);
        }
    }
}
