using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
            //Action();


            //GenerateCategory();

            //GenerateProduct();
        }

        private static void GenerateProduct()
        {
            JsonSerializerSettings setting = new JsonSerializerSettings();
            setting.Error += Errors;
            setting.MissingMemberHandling = MissingMemberHandling.Error;
            //setting.
            var productDetails = JsonConvert.DeserializeObject<List<ProductDetails>>
                (File.ReadAllText(pathProductDetails),setting);


            StringBuilder sb = new StringBuilder();
            StringBuilder sb1 = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            StringBuilder sb3 = new StringBuilder();
            StringBuilder sb4 = new StringBuilder();

            foreach (var prdet in productDetails)
            {
                GenerateLineProduct(sb, prdet);
                GenerateLineProductDetails(sb1, prdet);
                GenerateLineProductImage(sb2, prdet);
                GenerateLineProductCategory(sb3, prdet);
                GenerateLineProductOther(sb4, prdet);
            }

            var products = sb.ToString().TrimStart(',');
            var productsDetails = sb1.ToString().TrimStart(',');
            var productsImage = "INSERT INTO `oc_product_image` (`product_image_id`, `product_id`, `image`, `sort_order`) VALUES "+sb2.ToString().TrimStart(',');
            var productsCategory = "INSERT INTO `oc_product_to_category` (`product_id`, `category_id`) VALUES  " + sb3.ToString().TrimStart(',');
            var productsOther = sb4.ToString();

        }

        private static void GenerateLineProductOther(StringBuilder sb4, ProductDetails prdet)
        {
            sb4.AppendLine(
                $"INSERT INTO `oc_product_to_layout` (`product_id`, `store_id`, `layout_id`) VALUES ('{prdet.Id}', '0', '0');"
                );

            sb4.AppendLine(
                $"INSERT INTO `oc_product_to_store` (`product_id`, `store_id`) VALUES ('{prdet.Id}', '0');");
        }

        private static void GenerateLineProductCategory(StringBuilder sb, ProductDetails prdet)
        {
            sb.AppendLine($",('{prdet.Id}', '{prdet.Category.Id}')");
        }

        private static void GenerateLineProductImage(StringBuilder sb, ProductDetails prdet)
        {
            var str =
                $",('{prdet.Id}'," +
$"'{prdet.Id}'," +
$"'catalog{prdet.Images[0].LocalPath.Replace(ImageProductFolder, "").Replace("\\", "/")}'," +
$"'0')";
            sb.AppendLine(str);
        }

        private static void GenerateLineProductDetails(StringBuilder sb, ProductDetails prdet)
        {
            var str =
                $",('{prdet.Id}'," +	//	`product_id`
$"'1'," +	//	`language_id`
$"'{prdet.Title.Replace("'", "''")}'," +	//	`name`
$"'{prdet.Description?.Replace("'", "''")}'," +	//	`description`
$"''," +	//	`tag`
$"'{prdet.Title.Replace("'", "''")}'," +	//	`meta_title`
$"''," +	//	`meta_description`
$"'')"; //	`meta_keyword`
            sb.AppendLine(str);

        }

        private static void GenerateLineProduct(StringBuilder sb, ProductDetails prdet)
        {
            var str =
                $",('{prdet.Id}'," +	//	`product_id`
$"'{prdet.Title.Replace("'", "''")}'," +	//	`model`
$"''," +	//	`sku`
$"''," +	//	`upc`
$"''," +	//	`ean`
$"''," +	//	`jan`
$"''," +	//	`isbn`
$"''," +	//	`mpn`
$"''," +	//	`location`
$"'939'," +	//	`quantity`
$"'7'," +	//	`stock_status_id`
$"'catalog{prdet.Images[0].LocalPath.Replace(ImageProductFolder,"").Replace("\\","/")}'," +//  $"'catalog/demo/htc_touch_hd_1.jpg'" +	//	`image`
$"null," +//$"'5'" +	//	`manufacturer_id`
$"'1'," +	//	`shipping`
$"'{prdet.Price.Replace(" руб","").Replace(" ","")}'," +  //$"'100.0000'" +	//	`price`
$"'200'," +	//	`points`
$"'9'," +	//	`tax_class_id`
$"'2009-02-03'," +	//	`date_available`
$"'146.40000000'," +	//	`weight`
$"'2'," +	//	`weight_class_id`
$"'0.00000000'," +	//	`length`
$"'0.00000000'," +	//	`width`
$"'0.00000000'," +	//	`height`
$"'1'," +	//	`length_class_id`
$"'1'," +	//	`subtract`
$"'1'," +	//	`minimum`
$"'0'," +	//	`sort_order`
$"'1'," +	//	`status`
$"'0'," +	//	`viewed`
$"'2009-02-03 16:06:50'," +	//	`date_added`
$"'2011-09-30 01:05:39')";//	`date_modified`
            sb.AppendLine(str);
        }

        private static void Errors(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private static void GenerateCategory()
        {
            List<Category> cats = GetProductListAll();

            //int i = 1;
            //foreach (var cat in cats)
            //{
            //    cat.Id = i++;
            //    foreach (var subCat in cat.SubCategories)
            //    {
            //        subCat.Id = i++;
            //        subCat.ParentId = cat.Id;
            //    }
            //}
            //var jsonCat = Newtonsoft.Json.JsonConvert.SerializeObject(cats, Newtonsoft.Json.Formatting.Indented);
            //File.WriteAllText(CategoryProductJson, jsonCat);

            var sb = new StringBuilder();
            foreach (var cat in cats)
            {
                AddLineCategory(sb, cat);
                foreach (var subCat in cat.SubCategories)
                {
                    AddLineCategory(sb, subCat);
                }
            }
            var categoryValues = sb.ToString();

            var sb1 = new StringBuilder();
            foreach (var cat in cats)
            {
                sb1.AppendLine($",('{cat.Id}', '1', '{cat.Name}', '', '{cat.Name}', '', '')");
                foreach (var subCat in cat.SubCategories)
                {
                    sb1.AppendLine($",('{subCat.Id}', '1', '{subCat.Name}', '', '{subCat.Name}', '', '')");
                }
            }
            var categoryDescriptionValues = sb1.ToString().TrimStart(',');
        }

        private static void AddLineCategory(StringBuilder sb, Category cat)
        {
            sb.AppendLine(
                $"INSERT INTO `oc_category` (`category_id`, `image`, `parent_id`, `top`, `column`, `sort_order`, `status`, `date_added`, `date_modified`) VALUES ('{cat.Id}', '', '{cat.ParentId}', '{(cat.ParentId == 0 ? 1 : 0)}', '1', '0', '1', '2019-06-17 21:26:28', '2019-06-17 21:26:28');");

            sb.AppendLine(
                $"INSERT INTO `oc_category_path` (`category_id`, `path_id`, `level`) VALUES ('{cat.Id}', '{cat.Id}', '0');");

            sb.AppendLine(
                $"INSERT INTO `oc_category_to_layout` (`category_id`, `store_id`, `layout_id`) VALUES ('{cat.Id}', '0', '0');");

            sb.AppendLine(
                $"INSERT INTO `oc_category_to_store` (`category_id`, `store_id`) VALUES ('{cat.Id}', '0');");
        }

        private static void Action()
        {
            init();
            List<Category> cats = GetProductListAll();

            var products = cats.SelectMany(c => c.SubCategories).SelectMany(c => c.Products
            , (c, p) => new ProductCategory(p, c));


            List<ProductDetails> ProductDetails = GetProductsDetails(products);

            Console.WriteLine("Готово");
            Console.ReadLine();
        }
        static string pathProductDetails = "ProductDetail.json";

        private static List<ProductDetails> GetProductsDetails(IEnumerable<ProductCategory> products)
        {
            double count = products.Count();
            Console.WriteLine($"Всего продуктов - {count}");
            Console.WriteLine();

            var productDetails = new List<ProductDetails>();


            if (!File.Exists(pathProductDetails))
            {
                Console.WriteLine("Забираем детали");

                int i = 0;
                foreach (ProductCategory product in products)
                {
                    Console.WriteLine($"{i++},\t {i * 100.0 / count:0.00} %");
                    ProductDetails productDetail = GetDetailsProduct(product);
                    productDetails.Add(productDetail);
                }

                var jsonCat = Newtonsoft.Json.JsonConvert.SerializeObject(productDetails, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(pathProductDetails, jsonCat);
            }
            else
            {
                productDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductDetails>>(File.ReadAllText(pathProductDetails));
            }

            return productDetails;
        }

        private static void test()
        {
            //GetDetailsProduct(new Product
            //{
            //    Link = "http://xn--80adi1cd.xn--p1ai/%D0%BA%D0%B0%D0%BB%D1%8C%D1%8F%D0%BD%D1%8B/soft-smoke/softsmoke-lite.html",
            //    Name = "Шахта Soft Smoke Lite",
            //});
        }

        private static ProductDetails GetDetailsProduct(ProductCategory productCat)
        {
            ProductDetails productDetails = new ProductDetails
            {
                Images = new List<Image>()
            };
            productDetails.Category = productCat.Category;
            var product = productCat.Product;
            string fileName = string.Join("_", product.Paths);
            var doc = LoadOrReadGetDocument(fileName, product.Link);

            var block = doc.QuerySelector(".row-product");
            var title = block.QuerySelector("span[itemprop=\"name\"]");
            productDetails.Title = title.InnerText;
            productDetails.Link = product.Link;
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
                productDetails.OtherDescription = string.Join(";", dis.Skip(1).Select(c => c.InnerText));
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

        private static void MoveLocalImage(Image image)
        {
            throw new NotImplementedException();
        }

        private static string DownloadImage(string link, List<string> paths)
        {
            var pathsIm = link.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            string dir = Path.Combine(ImageProductFolder,
                string.Join(@"\", paths).Replace(".html", ""));

            dir = ReplaceSpecCharsDir(dir);

            paths = ReplaceSpecCharsDir(paths);
            string newDir = Path.Combine(ImageProductFolder,
                string.Join(@"\", paths).Replace(".html", ""));



            Directory.CreateDirectory(dir);
            string localFilename = dir + @"\" + pathsIm[pathsIm.Length - 1];

            string localFilenameNew = newDir + @"\" + pathsIm[pathsIm.Length - 1];

            if (!File.Exists(localFilename))
            {
                link = link.Replace("&amp;", "&");
                Console.WriteLine($"DownloadFile - {link}");
                wc.DownloadFile(link, localFilename);
            }
            else
            {
                Directory.CreateDirectory(newDir);
                if(!File.Exists(localFilenameNew))
                    File.Move(localFilename, localFilenameNew);
            }
            return localFilenameNew;
        }

        private static List<string> ReplaceSpecCharsDir(List<string> paths)
        {
            return paths.Select(ReplaceSpecCharsDir).ToList();
        }

        static string CategoryProductJson = "Category_Products.json";

        private static List<Category> GetProductListAll()
        {
            Console.WriteLine("Start - GetProductListAll");
            List<Category> cats;

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
