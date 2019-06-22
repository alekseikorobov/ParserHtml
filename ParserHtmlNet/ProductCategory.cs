namespace ParserHtmlNet
{
    internal class ProductCategory
    {
        private Product p;
        private Category c;

        public ProductCategory(Product p, Category c)
        {
            this.Product = p;
            this.Category = c;
        }

        public Product Product { get => this.p; set => this.p = value; }
        public Category Category { get => this.c; set => this.c = value; }
    }
}