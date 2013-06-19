using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using HtmlAgilityPack;

namespace Forager
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

            // There are various options, set as needed
            htmlDoc.OptionFixNestedTags = true;

            // filePath is a path to a file containing the html
            htmlDoc.LoadHtml("www.alexveit.com");

            // Use:  htmlDoc.LoadHtml(xmlString);  to load from a string (was htmlDoc.LoadXML(xmlString)

            // ParseErrors is an ArrayList containing any errors from the Load statement
            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
            {
                // Handle any parse errors as required

            }
            else
            {

                if (htmlDoc.DocumentNode != null)
                {
                    //HtmlAgilityPack.HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("//*");

                    var ls = new List<string>();
                    var nodes = htmlDoc.DocumentNode.SelectNodes("//html");
                    nodes.ToList().ForEach(a => ls.Add(a.GetAttributeValue("href", "")));

                   /* if (node != null)
                    {
                        int i = 0;
                        return;
                    }*/
                }
            }
        }
    }
}