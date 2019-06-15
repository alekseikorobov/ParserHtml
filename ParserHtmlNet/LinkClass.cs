using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

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
            }
        }
        public List<string> Paths
        {
            get
            {
                if (_link != null)
                {
                    return WebUtility.UrlDecode(_link).Replace("http://xn--80adi1cd.xn--p1ai/", "").Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                return new List<string>();
            }
        }
        public string Name { get; set; }
    }
}
