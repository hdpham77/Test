using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using CERS.Model;
using CERS.Guidance;
using System.Text;
using System.Web.Routing;

namespace CERS.Web.UI.Organization.ViewModels
{
    public class WhatsNextItem
    {
        public WhatsNextItem(int order, string whatsNextText, string linkText, string routeName, RouteValueDictionary routeValues)
        {
            Order = order;
            WhatsNextText = whatsNextText;
            LinkText = linkText;
            RouteName = routeName;
            RouteValues = routeValues;
            Url = null;
        }

        public WhatsNextItem(int order, string whatsNextText, string linkText, string routeName, object routeValues)
        {
            Order = order;
            WhatsNextText = whatsNextText;
            LinkText = linkText;
            RouteName = routeName;
            RouteValues = new RouteValueDictionary(routeValues);
            Url = null;
        }

        public WhatsNextItem(int order, string whatsNextText, string linkText, string url)
        {
            Order = order;
            WhatsNextText = whatsNextText;
            LinkText = linkText;
            Url = url;
            RouteName = null;
            RouteValues = null;
        }


        public int Order { get; set; }

        private string WhatsNextText { get; set; }

        private string LinkText { get; set; }

        private string RouteName { get; set; }

        private RouteValueDictionary RouteValues { get; set; }

        private string Url { get; set; }

        private string Link
        {
            get
            {
                string url = null;
                if (Url == null)
                {
                    url = RouteTable.Routes.GetVirtualPath(null, RouteName, RouteValues).VirtualPath;

                }
                else
                {
                    url = this.Url;
                }
                StringBuilder sb = new StringBuilder();
                return sb.Append("<a href=\"").Append(url).Append("\" class=\"defaultLink\">").Append(LinkText).Append("</a>").ToString();
            }
        }

        public string GetWhatsNext
        {
            get
            {
                return string.Format(this.WhatsNextText, this.Link);
            }
        }
    }
}