using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ParserHtmlNet
{
    public class Category
    {
        public string Link { get; set; }
        public string Name { get; set; }
        public List<Category> SubCategories { get; set; }
    }
    class Program
    {
        static System.Net.WebClient wc = new WebClient();
        static void Main(string[] args)
        {

            var u = "http://xn--80adi1cd.xn--p1ai/%D0%BA%D0%B0%D0%BB%D1%8C%D1%8F%D0%BD%D1%8B/";


            string pathStart = "cat1.html";

            if (!File.Exists(pathStart))
                html = wc.DownloadString(u);
            else
                html = File.ReadAllText(pathStart);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNode link = doc.DocumentNode.QuerySelector(".box-category");
            var linksCat = link.QuerySelector(".active");
            var par = linksCat.ParentNode;
            var par1 = par.ParentNode;
            //System.Console.WriteLine($"ul.Count - {linksCat.Count()}");

            //var cats = GetBaseCategory();
            //foreach (var cat in cats)
            //{
            //    html = wc.DownloadString(cat.Link);
            //}
        }

        static string html = "";
        private static List<Category> GetBaseCategory()
        {
            List<Category> cats = new List<Category>();
            string pathStart = "cal.html";

            if (!File.Exists(pathStart))
                html = wc.DownloadString("http://xn--80adi1cd.xn--p1ai");
            else
                html = File.ReadAllText(pathStart);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNode link = doc.DocumentNode.SelectSingleNode("//*[@id=\"column-left\"]");
            var linksCat = link.QuerySelectorAll("li");
            System.Console.WriteLine($"ul.Count - {linksCat.Count()}");

            foreach (HtmlNode linkCat in linksCat)
            {
                var a = linkCat.QuerySelector("a");
                cats.Add(new Category()
                {
                    Link = a.Attributes["href"].Value,
                    Name = a.InnerText
                });
                System.Console.WriteLine(a.OuterHtml);
            }

            return cats;

        }
    }
}
