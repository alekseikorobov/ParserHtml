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

    class Program
    {
        static System.Net.WebClient wc = new WebClient();
        static string html = "";
        static string CacheFolder = "CacheHtml";
        static string ImageProductFolder = "ImageProducts";


        private static void init()
        {
            (wc.Proxy = WebRequest.DefaultWebProxy).Credentials = CredentialCache.DefaultCredentials;

            Directory.CreateDirectory(CacheFolder);
            Directory.CreateDirectory(ImageProductFolder);
        }

        static void Main(string[] args)
        {
            init();
            List<Category> cats = null;

            cats = GetProductListAll();

            var products = cats.SelectMany(c => c.SubCategories).SelectMany(c => c.Products);


            List<ProductDetails> ProductDetails = GetProductsDetails(products);

            Console.WriteLine("Готово");
            Console.ReadLine();

        }

        private static List<ProductDetails> GetProductsDetails(IEnumerable<Product> products)
        {
            double count = products.Count();
            Console.WriteLine($"Всего продуктов - {count}");
            Console.WriteLine();

            List<ProductDetails> ProductDetails = new List<ProductDetails>();

            string pathProductDetails = "ProductDetail.json";

            if (!File.Exists(pathProductDetails))
            {
                Console.WriteLine("Забираем детали");

                int i = 0;
                foreach (Product product in products)
                {
                    Console.WriteLine($"{i++},\t {i * 100.0 / count:0.00} %");
                    ProductDetails productDetail = GetDetailsProduct(product);
                    ProductDetails.Add(productDetail);
                }

                var jsonCat = Newtonsoft.Json.JsonConvert.SerializeObject(ProductDetails,Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(pathProductDetails, jsonCat);
            }
            else
            {
                ProductDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductDetails>>(File.ReadAllText(pathProductDetails));
            }

            return ProductDetails;
        }

        private static void test()
        {
            GetDetailsProduct(new Product
            {
                Link = "http://xn--80adi1cd.xn--p1ai/%D0%BA%D0%B0%D0%BB%D1%8C%D1%8F%D0%BD%D1%8B/soft-smoke/softsmoke-lite.html",
                Name = "Шахта Soft Smoke Lite",
            });
        }

        private static ProductDetails GetDetailsProduct(Product product)
        {
            ProductDetails productDetails = new ProductDetails
            {
                Images = new List<Image>()
            };
            string fileName = string.Join("_", product.Paths);
            var doc = LoadOrReadGetDocument(fileName, product.Link);

            var block = doc.QuerySelector(".row-product");
            var title = block.QuerySelector("span[itemprop=\"name\"]");
            productDetails.Title = title.InnerText;

            var thumbnail = block.QuerySelector("a.thumbnail");
            var image = new Image()
            {
                ServerPath = thumbnail.Attributes["href"].Value,
                LocalPath = DownloadImage(thumbnail.Attributes["href"].Value, product.Paths)
            };
            productDetails.Images.Add(image);

            var priceBlock = block.QuerySelector("span#formated_price");

            productDetails.Price = priceBlock.InnerText;

            var tabs = doc.QuerySelector(".row.tabs.pos-11");
            var disBlock = tabs.QuerySelector("div[itemprop='description']");
            var dis = disBlock.QuerySelectorAll("p").ToList();
            if (dis.Count != 0)
            {
                productDetails.Description = dis[0].InnerText;
                productDetails.OtherDescription = string.Join(";", dis.Skip(1));
            }
            else
            {
                var dis1 = disBlock.QuerySelectorAll("div").ToList();
                if (dis1.Count > 0)
                {
                    productDetails.Description = dis1[0].InnerText;
                    productDetails.OtherDescription = string.Join(";", dis1.Skip(1));
                }
            }
            return productDetails;
        }

        private static string DownloadImage(string link, List<string> paths)
        {
            var pathsIm = link.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            string dir = Path.Combine(ImageProductFolder, 
                string.Join(@"\", paths).Replace(".html", ""));

            dir = ReplaceSpecCharsDir(dir);
            Directory.CreateDirectory(dir);
            string localFilename = dir + @"\" + pathsIm[pathsIm.Length - 1];

            if (!File.Exists(localFilename))
            {
                link = link.Replace("&amp;", "&");
                Console.WriteLine($"DownloadFile - {link}");
                wc.DownloadFile(link, localFilename);
            }
            return localFilename;
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

                var jsonCat = Newtonsoft.Json.JsonConvert.SerializeObject(cats, Newtonsoft.Json.Formatting.Indented);
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

                var jsonCat = Newtonsoft.Json.JsonConvert.SerializeObject(cats, Newtonsoft.Json.Formatting.Indented);
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
            fileName = ReplaceSpecChars(fileName);
            var fullName = Path.Combine(CacheFolder, fileName);
            var html = "";
            if (!File.Exists(fullName))
            {
                url = url.Replace("&amp;", "&");
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

        private static string ReplaceSpecChars(string fileName)
        {
            foreach (var ch in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(ch, '_');
            }
            fileName = fileName.Replace("&amp;", "_");
            return fileName;
        }
        private static string ReplaceSpecCharsDir(string dirName)
        {
            foreach (var ch in Path.GetInvalidPathChars().Union(Path.GetInvalidFileNameChars()))
            {
                dirName = dirName.Replace(ch, '_');
            }
            dirName = dirName.Replace("&amp;", "_");
            return dirName;
        }
    }
}
