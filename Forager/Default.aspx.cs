using System;
using System.Collections;
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
        private List<DocumentWithLinks> _dwls = new List<DocumentWithLinks>();
        string _domain;

        private static string Get_normalized_Url(string url)
        {
            string normalized = "http://";
            if (normalized.Equals(url.Substring(0, 7)))
            {
                return url;
            }
            return url.Insert(0, normalized); ;
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            _domain = TextBox1.Text + "/";
            Get_Links(_domain);
        }

        private void Get_Links(string url)
        {
            HtmlWeb hw = new HtmlWeb();
            string normalUrl = Get_normalized_Url(url);
            if (normalUrl != null)
            {
                HtmlDocument doc = hw.Load(normalUrl);

                DocumentWithLinks nwl = new DocumentWithLinks(doc);

                _dwls.Add(nwl);

                for (int i = 0; i < nwl.Links.Count; i++)
                {
                    string temp = _domain + nwl.References[i].ToString();
                    Get_Links(temp);
                }
            }
        }

        /// <summary>
        /// Represents a document that needs linked files to be rendered, such as images or css files, and points to other HTML documents.
        /// </summary>
        public class DocumentWithLinks
        {
            private ArrayList _links;
            private ArrayList _references;
            private HtmlDocument _doc;

            /// <summary>
            /// Creates an instance of a DocumentWithLinkedFiles.
            /// </summary>
            /// <param name="doc">The input HTML document. May not be null.</param>
            public DocumentWithLinks(HtmlDocument doc)
            {
                if (doc == null)
                {
                    throw new ArgumentNullException("doc");
                }
                _doc = doc;
                GetLinks();
                GetReferences();
            }

            private void GetLinks()
            {
                _links = new ArrayList();
                HtmlNodeCollection atts = _doc.DocumentNode.SelectNodes("//*[@background or @lowsrc or @src or @href]");
                if (atts == null)
                    return;

                foreach (HtmlNode n in atts)
                {
                    ParseLink(n, "background");
                    ParseLink(n, "href");
                    ParseLink(n, "src");
                    ParseLink(n, "lowsrc");
                }
            }

            private void GetReferences()
            {
                _references = new ArrayList();
                HtmlNodeCollection hrefs = _doc.DocumentNode.SelectNodes("//a[@href]");
                if (hrefs == null)
                    return;

                foreach (HtmlNode href in hrefs)
                {
                    if(!_references.Contains(href.Attributes["href"].Value))
                        _references.Add(href.Attributes["href"].Value);
                }
            }

            private void ParseLink(HtmlNode node, string name)
            {
                HtmlAttribute att = node.Attributes[name];
                if (att == null)
                    return;

                // if name = href, we are only interested by <link> tags
                if ((name == "href") && (node.Name != "link"))
                    return;
                if(!_links.Contains(att.Value))
                    _links.Add(att.Value);
            }

            /// <summary>
            /// Gets a list of links as they are declared in the HTML document.
            /// </summary>
            public ArrayList Links
            {
                get
                {
                    return _links;
                }
            }

            /// <summary>
            /// Gets a list of reference links to other HTML documents, as they are declared in the HTML document.
            /// </summary>
            public ArrayList References
            {
                get
                {
                    return _references;
                }
            }

        }
    }
}