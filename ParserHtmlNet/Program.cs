using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ParserHtmlNet
{
    public abstract class LinkClass
    {
        private string _link;

        public string Link
        {
            get => _link; set
            {
                _link = value;

                Paths = WebUtility.UrlDecode(_link).Replace("http://xn--80adi1cd.xn--p1ai/", "").Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }
        public List<string> Paths { get; set; }
        public string Name { get; set; }
    }
    public class Product : LinkClass
    {
        public string ImageLink { get; internal set; }
    }
    public class Category : LinkClass
    {
        public List<Product> Products { get; set; }
        public List<Category> SubCategories { get; set; }
    }

    class Program
    {
        static System.Net.WebClient wc = new WebClient();
        static string html = "";
        static string CacheFolder = "CacheHtml";

        private static void init()
        {
            (wc.Proxy = WebRequest.DefaultWebProxy).Credentials = CredentialCache.DefaultCredentials;

            Directory.CreateDirectory(CacheFolder);
        }

        static void Main(string[] args)
        {
            init();
            List<Category> cats = null;

            cats = GetProductListAll();

            var products = cats.SelectMany(c => c.SubCategories).SelectMany(c => c.Products);

            Console.WriteLine($"Всего продуктов - {products.Count()}");

        }

        private static List<Category> GetProductListAll()
        {
            Console.WriteLine("Start - GetProductListAll");
            List<Category> cats;
            string CategoryProductJson = "Category_Products.json";

            if (!File.Exists(CategoryProductJson))
            {
                cats = GetAllCategory();

                var catSubs = cats.SelectMany(c => c.SubCategories);
                foreach (var catSub in catSubs)
                {
                    GetProductList(catSub);
                }

                var jsonCat = Newtonsoft.Json.JsonConvert.SerializeObject(cats);
                File.WriteAllText(CategoryProductJson, jsonCat);
            }
            else
            {
                cats = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Category>>(File.ReadAllText(CategoryProductJson));
            }
            Console.WriteLine("End - GetProductListAll");
            return cats;
        }

        private static void GetProductList(Category cat)
        {
            Console.WriteLine($"Start GetProductList");
            cat.Products = new List<Product>();
            var cachFile = string.Join("_", cat.Paths) + ".html";

            HtmlNode doc = LoadOrReadGetDocument(cachFile, cat.Link);

            var pages = getPages(doc);

            Console.WriteLine($"Получено страниц {pages}");

            if (pages == 0) return;

            int page = 1;
            do
            {
                if (page > 1)
                {
                    cachFile = string.Join("_", cat.Paths) + "_page_" + page + ".html";
                    doc = LoadOrReadGetDocument(cachFile, cat.Link + "?page=" + page);
                }

                var elms = doc.QuerySelectorAll(".product-layout");

                foreach (var elm in elms)
                {
                    var nameBlock = elm.QuerySelector(".name");
                    var nameLink = nameBlock.QuerySelector("a");

                    var imageBlock = elm.QuerySelector("img");

                    cat.Products.Add(new Product()
                    {
                        Link = nameLink.Attributes["href"].Value,
                        Name = nameLink.InnerText,
                        ImageLink = imageBlock.Attributes["src"].Value
                    });
                }

            } while (page++ < pages);

            Console.WriteLine($"End GetProductList");
        }

        private static int getPages(HtmlNode doc)
        {
            if (doc.InnerHtml.Contains("В этой категории нет товаров.")) return 0;

            var res = doc.QuerySelector("div.results");
            var match = Regex.Match(res.InnerText, "страниц: (\\d+)");

            var pages = match.Groups[1].Value;

            return int.Parse(pages);
        }



        #region Category
        private static List<Category> GetAllCategory()
        {
            Console.WriteLine("Start - GetAllCategory");
            string Categoryjson = "Category.json";

            List<Category> cats = null;
            if (!File.Exists(Categoryjson))
            {
                cats = GetBaseCategory();

                foreach (var c in cats)
                {
                    GetLineSubCategory(c);
                }

                var jsonCat = Newtonsoft.Json.JsonConvert.SerializeObject(cats);
                File.WriteAllText(Categoryjson, jsonCat);
            }
            else
            {
                cats = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Category>>(File.ReadAllText(Categoryjson));
            }
            Console.WriteLine("End - GetAllCategory");
            return cats;
        }
        private static void GetLineSubCategory(Category c)
        {
            string pathStart = $"{c.Paths[0]}_subCat.html";

            HtmlNode doc = LoadOrReadGetDocument(pathStart, c.Link);

            HtmlNode link = doc.QuerySelector(".box-category");
            c.SubCategories = new List<Category>();
            foreach (var htmlNode in link.ChildNodes)
            {
                if (htmlNode.Name == "li")
                {
                    if (htmlNode.ChildNodes[0].Name == "ul")
                    {
                        var links1 = htmlNode.ChildNodes[0].QuerySelectorAll("a");

                        foreach (HtmlNode linkCat in links1)
                        {

                            c.SubCategories.Add(new Category()
                            {
                                Link = linkCat.Attributes["href"].Value,
                                Name = linkCat.InnerText
                            });
                        }
                    }
                }
            }
        }
        private static List<Category> GetBaseCategory()
        {
            List<Category> cats = new List<Category>();
            string pathStart = "baseCat.html";
            HtmlNode doc = LoadOrReadGetDocument(pathStart, "http://xn--80adi1cd.xn--p1ai");

            HtmlNode link = doc.SelectSingleNode("//*[@id=\"column-left\"]");
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
        #endregion

        private static HtmlNode GetDocument(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode;
        }

        private static HtmlNode LoadOrReadGetDocument(string fileName, string url)
        {
            return GetDocument(LoadOrRead(fileName, url));
        }

        private static string LoadOrRead(string fileName, string url)
        {
            var fullName = Path.Combine(CacheFolder, fileName);
            var html = "";
            if (!File.Exists(fullName))
            {
                html = wc.DownloadString(url);
                Console.WriteLine($"{WebUtility.UrlDecode(url)}");
                File.WriteAllText(fullName, html);
            }
            else
            {
                html = File.ReadAllText(fullName);
            }
            return html;
        }
    }
}
