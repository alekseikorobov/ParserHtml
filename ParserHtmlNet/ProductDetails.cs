using System.Collections.Generic;

namespace ParserHtmlNet
{
    public class ProductDetails : LinkClass
    {
        public List<Image> Images { get; set; }
        public string Title { get; internal set; }
        public string Price { get; internal set; }
        public string Description { get; internal set; }
        public string OtherDescription { get; internal set; }
    }
}
