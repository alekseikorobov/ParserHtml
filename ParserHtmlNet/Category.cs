using System.Collections.Generic;

namespace ParserHtmlNet
{
    public class Category : LinkClass
    {
        public int Id { get; set; }
        public List<Product> Products { get; set; }
        public List<Category> SubCategories { get; set; }
        public int ParentId { get; set; }
    }
}
