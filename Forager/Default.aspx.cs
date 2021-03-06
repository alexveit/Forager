﻿using HtmlAgilityPack;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web.UI;

namespace Forager
{
    public partial class Default : System.Web.UI.Page
    {
        ArrayList _references = new ArrayList();
        string _absolute_domain;
        string _domain;

        string _path = Directory.GetCurrentDirectory() + "\\result.txt";
        string _logpath = Directory.GetCurrentDirectory() + "\\errorlog.txt";

        private void addtolog(string expt_msg)
        {
            string msg = "Oops... " + expt_msg;
            using (StreamWriter sw = File.AppendText(_logpath))
            {
                sw.WriteLine(msg);
            }
        }

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
            using (StreamWriter sw = File.CreateText(_path))
            {
                sw.WriteLine("Links:");
            }
            using (StreamWriter sw = File.CreateText(_logpath))
            {
                sw.WriteLine("Log:");
            }
        }

        //when the user clicks the button on the website this gets called
        protected void Button1_Click(object sender, EventArgs e)
        {
            _absolute_domain = Get_normalized_Url(TextBox1.Text);
            if (_absolute_domain != null)
            {
                _domain = GetDomain(_absolute_domain);
                if (_domain != null)
                {

                    Get_Links(_absolute_domain);

                    //Write_to_File();

                    Send_Email();

                    string msg = "alert('Successful Forage');";
                    Page.ClientScript.RegisterStartupScript(GetType(), "msgbox", msg, true);
                }
                else
                {
                    string msg = "alert('Oops... Something whent wrong with the input');";
                    Page.ClientScript.RegisterStartupScript(GetType(), "msgbox", msg, true);
                }
            }
            else
            {
                string msg = "alert('Oops... Something whent wrong with the input');";
                Page.ClientScript.RegisterStartupScript(GetType(), "msgbox", msg, true);
            }
        }

        bool Get_Links(string url)
        {
            try
            {
                HtmlWeb hw = new HtmlWeb();
                HtmlDocument doc = hw.Load(url);
                DocumentWithLinks nwl = new DocumentWithLinks(doc);

                for (int i = 0; i < nwl.References.Count; i++)
                {
                    if (!_references.Contains(nwl.References[i]))
                    {
                        _references.Add(nwl.References[i]);
                        using (StreamWriter sw = File.AppendText(_path))
                        {
                            string tmp;
                            if (nwl.References[i].ToString().Length > 0)
                            {
                                if (ConstainsHTTP(nwl.References[i].ToString()))
                                    tmp = nwl.References[i].ToString();
                                else
                                {
                                    if (nwl.References[i].ToString().First().Equals('/'))
                                        tmp = nwl.References[i].ToString().Insert(0, _absolute_domain);
                                    else
                                        tmp = nwl.References[i].ToString().Insert(0, _absolute_domain + "/");
                                }
                                sw.WriteLine(tmp);
                            }
                        }
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
                        {
                            try
                            {
                                Get_Links(temp);
                            }
                            catch (Exception ex)
                            {
                                addtolog(ex.Message);
                            }
                        }
                    }
                }
            }
            catch (StackOverflowException ex)
            {
                addtolog(ex.Message);
            }
            return true;
        }

        private void Send_Email()
        {
            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("group2forager@gmail.com", "honeycomb");


            MailMessage mm = new MailMessage("donotreply@domain.com", "aveit@spsu.edu", "Forager Report", "test body");
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            //add following code before smtpClient.Send()

            client.EnableSsl = true;

            ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            client.Send(mm);
        }

        private void Write_to_File()
        {
            string name = Directory.GetCurrentDirectory() + "\\result.txt";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(name))
            {
                string tmp;
                foreach (string line in _references)
                {
                    if (line.Length > 0)
                    {
                        if (ConstainsHTTP(line))
                            tmp = line;
                        else
                        {
                            if (line.First().Equals('/'))
                                tmp = line.Insert(0, _absolute_domain);
                            else
                                tmp = line.Insert(0, _absolute_domain + "/");
                        }

                        file.WriteLine(tmp);
                    }
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