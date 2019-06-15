using System.Collections.Generic;

namespace ParserHtmlNet
{
    public class Category : LinkClass
    {
        public List<Product> Products { get; set; }
        public List<Category> SubCategories { get; set; }
    }
}
