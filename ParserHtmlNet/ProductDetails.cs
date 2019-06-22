using System.Collections.Generic;

namespace ParserHtmlNet
{
    public class ProductDetails : LinkClass
    {
        public int Id { get; set; }
        public Category Category { get; set; }
        public List<Image> Images { get; set; }
        public string Title { get;  set; }
        public string Price { get; set; }
        public string Description { get;  set; }
        public string OtherDescription { get;  set; }
    }
}
