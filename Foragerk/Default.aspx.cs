using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Odbc;
using System.Configuration;
using HtmlAgilityPack;
using System.IO;
using System.Text;
using System.Net;
using TestCrawler;

using System.Collections;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public partial class _Default : System.Web.UI.Page
{
    /*DO NOT EVEN THINK OF DELETING Karl's old code. DO NOT DELETE!!!
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            using (OdbcConnection connection = new OdbcConnection(ConfigurationManager.ConnectionStrings["Forager_Connection"].ConnectionString))
            {
                connection.Open();
                using (OdbcCommand command = new OdbcCommand("select * from team", connection))

                using (OdbcDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Response.Write(reader["name"].ToString() + "<br />");
                    }
                    reader.Close();
                }
                connection.Close();
            }
        }
        catch (Exception ex)
        {
            Response.Write("Error: " + ex.Message);
        }
    }
    */

    ArrayList _references = new ArrayList();
    ArrayList _second_references = new ArrayList();
    Dictionary<string, string> arrFailedLinks = new Dictionary<string, string>();
    string _absolute_domain;
    string _domain;

    //string _path = Directory.GetCurrentDirectory() + "\\result.txt";
    //string _path = @"C:\Users\karlloic\Documents\links.txt";
    string _path = @"C:\links.txt";

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
    }

    protected bool KarlCheck(string str)
    {
        bool isgoodlink = false;
        string tmp = "";
        if (str.Length > 0)
        {
            if (ConstainsHTTP(str))
                tmp = str;
            else
            {
                if (str.First().Equals('/'))
                    tmp = str.Insert(0, _absolute_domain);
                else
                    tmp = str.Insert(0, _absolute_domain + "/");
            }
            _second_references.Add(tmp);
        }

        string strErrorMessage;
        string strFullUrl = CrawlerUtilities.MapVirtualUrl(_absolute_domain, tmp);
        string dummyContents = CrawlerUtilities.GetRemotePageContents(strFullUrl, out strErrorMessage);
        if (strErrorMessage.Length > 0)
        {
            Console.WriteLine(string.Format("URL '{0}' is invalid or server is down ({1})", strFullUrl, strErrorMessage));
            arrFailedLinks.Add(strFullUrl, strErrorMessage);
            CrawlerUtilities.WriteToFile(strFullUrl, strErrorMessage);
        }
        else
        {
            Console.WriteLine("URL '" + strFullUrl + "' is valid!");
            isgoodlink = true;
        }

        return isgoodlink;
    }

    //when the user clicks the button on the website this gets called
    protected void Button1_Click1(object sender, EventArgs e)
    {
        _absolute_domain = Get_normalized_Url(TextBox1.Text);
        if (_absolute_domain != null)
        {
            _domain = GetDomain(_absolute_domain);
            if (_domain != null)
            {
                Get_Links(_absolute_domain);

                //not using for now
                Write_to_File();

                //Send_Email();

                if (arrFailedLinks.Count > 0)
                {
                    Console.WriteLine("Found " + arrFailedLinks.Count + " invalid links, trying to send report.");
                    try
                    {
                        CrawlerUtilities.SendReport(_absolute_domain, arrFailedLinks);
                        CrawlerUtilities.WriteToFile(arrFailedLinks.Count.ToString(), " Failed links");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to send report: " + ex.ToString());
                        CrawlerUtilities.WriteToFile("Failed to send report: ", ex.ToString());
                    }
                }
                else
                {
                    try
                    {
                        CrawlerUtilities.SendReport(_absolute_domain, arrFailedLinks);
                        CrawlerUtilities.WriteToFile(arrFailedLinks.Count.ToString(), " Failed links");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to send report: " + ex.ToString());
                        CrawlerUtilities.WriteToFile("Failed to send report: ", ex.ToString());
                    }
                }

                //Response.Redirect("result.aspx");
                    
                 /* 
                 * 
                 * 
                 * 
                 * 
                  */

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

    private void Get_Links(string url)
    {
        HtmlWeb hw = new HtmlWeb();
        HtmlDocument doc = hw.Load(url);
        DocumentWithLinks nwl = new DocumentWithLinks(doc);

        for (int i = 0; i < nwl.References.Count; i++)
        {
            if (!Has_Been_Visited(nwl.References[i]))
            {
                //this statement does not append complete url to _references- just the remining path
                _references.Add(nwl.References[i]);


                //this is used just to print complete links to the file (_path) as they are got
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
                        if(KarlCheck(temp))
                            Get_Links(temp);
                    }
                    catch (Exception ex)
                    {
                        string msg = "alert('Oops... " + ex.Message + "');";
                        Page.ClientScript.RegisterStartupScript(GetType(), "msgbox", msg, true);
                    }
                }
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
        //string name = @"C:\Users\karlloic\Documents\links.txt";
        string name = @"C:\links.txt";
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
                if (!_references.Contains(href.Attributes["href"].Value))
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
            if (!_links.Contains(att.Value))
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