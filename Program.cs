using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Fizzler;

namespace ParserSevas
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Net.WebClient wc = new WebClient();
            
            string pathStart = "cal.html";
            string html = "";

            if(!File.Exists(pathStart)){
                html = wc.DownloadString("http://xn--80adi1cd.xn--p1ai/%D0%BA%D0%B0%D0%BB%D1%8C%D1%8F%D0%BD%D1%8B/");
            }
            else{
                html  = File.ReadAllText(pathStart);
            }

            HtmlDocument doc = new HtmlDocument(); 
 
            doc.LoadHtml(html); 
 
            HtmlNodeCollection links = doc.DocumentNode.SelectNodes("//*[@id=\"column-left\"]");

            foreach (HtmlNode link in links)
            {
                System.Console.WriteLine(link.OuterHtml);
                System.Console.WriteLine("------------------------");
                var lis = link;

                System.Console.WriteLine($"ul.Count - {lis.Count()}");

                foreach (HtmlNode li in lis)
                {
                    System.Console.WriteLine(li.OuterHtml);
                    var a  = li.SelectSingleNode("//a");
                    System.Console.WriteLine(a.OuterHtml);
                    break;
                }
            }
        }
    }
}
