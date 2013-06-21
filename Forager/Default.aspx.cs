using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using HtmlAgilityPack;



namespace Forager
{
    public partial class Default : System.Web.UI.Page
    {
        ArrayList _references = new ArrayList();
        string _absolute_domain;
        string _domain;


        private static bool ConstainsHTTP(string url)
        {
            return (url.Contains("http://") || url.Contains("https://"));
        }

        private static string GetDomain(string absolute_domain)
        {
            string d;
            if (absolute_domain.Contains("http://"))
                d = absolute_domain.Substring(7);
            else
                d = absolute_domain;

            int index = d.Length - 1;

            for (int i = 0; i < d.Length; i++)
            {
                if (d[i] == '/')
                {
                    index = i - 1;
                }
            }

            string ret = null;
            int dot_count = 0;
            int len = 0;
            for (int i = index; i >= 0; i--)
            {
                if (d[i] == '.')
                {
                    dot_count++;
                    if (dot_count == 2)
                    {
                        ret = d.Substring(i + 1, len);
                        break;
                    }
                }
                len++;
            }

            return ret;
        }

        private static string Get_normalized_Url(string url)
        {
            string ret;

            if (url.Length >= 7 && ConstainsHTTP(url))
                ret = url;
            else
                ret = url.Insert(0, "http://");

            return ret;
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                _absolute_domain = Get_normalized_Url(TextBox1.Text);
                if (_absolute_domain != null)
                {
                    _domain = GetDomain(_absolute_domain);
                    Get_Links(_absolute_domain);
                }
            }
            catch (Exception ex)
            {
                string msg = "alert('Oops... " + ex.Message + "');";
                Page.ClientScript.RegisterStartupScript(GetType(), "msgbox", msg, true);
            }
            finally
            {
                string name = Directory.GetCurrentDirectory() + "\\result.txt";
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(name))
                {
                    string tmp;
                    foreach (string line in _references)
                    {

                        if (ConstainsHTTP(line))
                            tmp = line;
                        else
                            tmp = line.Insert(0, _absolute_domain);

                        file.WriteLine(tmp);
                    }
                }
            }
        }

        private void Get_Links(string url)
        {
            HtmlWeb hw = new HtmlWeb();
            HtmlDocument doc = hw.Load(url);
            DocumentWithLinks nwl = new DocumentWithLinks(doc);

            for (int i = 0; i < nwl.References.Count; i++)
            {
                if (!Has_Been_Visited(nwl.References[i]))
                {
                    _references.Add(nwl.References[i]);
                    string temp;
                    if (!ConstainsHTTP(nwl.References[i].ToString()))
                    {
                        temp = _absolute_domain;
                        if (nwl.References[i].ToString()[0] == '/')
                            temp += nwl.References[i].ToString();
                        else
                            temp += "/" + nwl.References[i].ToString();
                    }
                    else
                        temp = nwl.References[i].ToString();

                    if (temp.Contains(_domain))
                        Get_Links(temp);
                }
            }
        }

        private bool Has_Been_Visited(object page)
        {
            for (int i = 0; i < _references.Count; i++)
            {
                if (_references[i].Equals(page))
                    return true;
            }
            return false;
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