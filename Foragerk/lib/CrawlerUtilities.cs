using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Configuration;
using System.Net;
using MySql.Data.MySqlClient;

using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace TestCrawler
{
    public static class CrawlerUtilities
    {
        public static string MapVirtualUrl(string originalURL, string virtURL)
        {
            //maybe not virtual?
            if (virtURL.StartsWith("http://") || virtURL.StartsWith("https://"))
                return virtURL;

            string strCleanOriginal = originalURL.ToLower();
            string prefix = strCleanOriginal.StartsWith("http://") ? ("http://") : (strCleanOriginal.StartsWith("https://") ? "https://" : "");
            int index = strCleanOriginal.IndexOf("?");
            if (index > 0)
                strCleanOriginal = strCleanOriginal.Substring(0, index);
            string[] parts = strCleanOriginal.Replace("http://", "").Replace("https://", "").Split('/');
            string root = parts[0];
            string retVal = prefix + root;
            if (virtURL.StartsWith("?"))
                virtURL = parts[parts.Length - 1] + virtURL;
            if (!virtURL.StartsWith("/"))
            {
                for (int i = 1; i < parts.Length - 1; i++)
                {
                    retVal += "/" + parts[i] + "/";
                }
            }
            retVal += virtURL;

            return retVal;
        }

        public static string GetRemotePageContents(string url, out string error)
        {
            error = string.Empty;
            string contents = string.Empty;
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.UserAgent] = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; Trident/4.0)";
                try
                {
                    contents = client.DownloadString(url);
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }
            }
            return contents;
        }

        public static void SendReport(string url, Dictionary<string, string> badLinks)
        {
            /*
            string recepientEmail = GetAppSetting("RecepientEmail", "", true, "RecepientEmail");
            string senderName = GetAppSetting("SenderName", "");
            string senderEmail = GetAppSetting("SenderEmail", "", true, "SenderEmail");
            string smtpServerAddress = GetAppSetting("SmtpServerAddress", "localhost");
            string subject = GetAppSetting("ReportEmailSubject", "Found some bad links");
            string smtpPort = GetAppSetting("SmtpPort", "");

            //format?
            if (subject.IndexOf("{0}") > 0)
                subject = subject.Replace("{0}", url);

            string fullSender = (senderName.Length > 0) ? string.Format("{0}<{1}>", senderName, senderEmail) : senderEmail;
            SmtpClient client = new SmtpClient(smtpServerAddress);
            client.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis;
            if (smtpPort.Length > 0)
                client.Port = Int32.Parse(smtpPort);

            StringBuilder body = new StringBuilder();
            body.Append("The page ").Append(url).Append(" contains ").Append(badLinks.Count).Append(" invalid links:").Append(Environment.NewLine);
            foreach (string badUrl in badLinks.Keys)
            {
                body.AppendFormat("'{0}' is invalid, error was: {1}{2}", badUrl, badLinks[badUrl], Environment.NewLine);
            }

            using (MailMessage message = new MailMessage(fullSender, recepientEmail, subject, body.ToString()))
            {
                try
                {
                    client.Send(message);
                    Console.WriteLine("Email sent successfully to " + recepientEmail);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to send email: " + ex.Message);
                }
            }
            */

            /*
            var fromAddress = new MailAddress("karlloic@gmail.com", "Group 2 Forager");
            var toAddress = new MailAddress("kkamdem@spsu.edu", "Karl-Loic");
            const string fromPassword = "christelle11";
            const string subject = "Forager Report";
            //const string body = "Body";

            StringBuilder body = new StringBuilder();
            body.Append("The page ").Append(url).Append(" contains ").Append(badLinks.Count).Append(" invalid links:").Append(Environment.NewLine);
            foreach (string badUrl in badLinks.Keys)
            {
                body.AppendFormat("'{0}' is invalid, error was: {1}{2}", badUrl, badLinks[badUrl], Environment.NewLine);
            }

            var smtp = new SmtpClient
                       {
                           Host = "smtp.gmail.com",
                           Port = 587,
                           EnableSsl = true,
                           DeliveryMethod = SmtpDeliveryMethod.Network,
                           UseDefaultCredentials = false,
                           Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                       };
            using (var message = new MailMessage(fromAddress, toAddress)
                                 {
                                     Subject = subject,
                                     Body = body.ToString()
                                 })
            {
                smtp.Send(message);
            }*/

            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("group2forager@gmail.com", "honeycomb");

            StringBuilder body = new StringBuilder();
            body.Append("The page ").Append(url).Append(" contains ").Append(badLinks.Count).Append(" invalid links:").Append(Environment.NewLine);
            foreach (string badUrl in badLinks.Keys)
            {
                body.AppendFormat("'{0}' is invalid, error was: {1}{2}", badUrl, badLinks[badUrl], Environment.NewLine);
            }

            MailMessage mm = new MailMessage("donotreply@domain.com", "kkamdem@spsu.edu", "Forager Report", body.ToString());
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
 
            //add following code before smtpClient.Send()

            client.EnableSsl = true;
 
            ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };   
            client.Send(mm);
        }

        public static string GetAppSetting(string key, string defVal, bool blnRequired, string keyName)
        {
            string value = ConfigurationManager.AppSettings[key] + "";
            if (value.Length == 0)
                value = defVal;
            if (value.Length == 0 && blnRequired)
                throw new Exception(string.Format("Missing application setting {0}!", keyName));
            return value;
        }

        public static string GetAppSetting(string key, string defVal)
        {
            return GetAppSetting(key, defVal, false, string.Empty);
        }

        /// <summary>
        /// Map all attributes of given element.
        /// When same attribute appears more than once, only first value is taken.
        /// </summary>
        /// <param name="elementRawContents">Raw HTML of the element to parse</param>
        /// <returns>Dictionary of attributes, key is the attribute name and value is its value.</returns>
        static public Dictionary<string, string> MapElementAttributes(string elementRawContents)
        {
            //initialize result:
            Dictionary<string, string> attributes = new Dictionary<string, string>();

            //remove extra spaces:
            while (elementRawContents.IndexOf("  ") >= 0)
                elementRawContents = elementRawContents.Replace("  ", " ");

            //split by space:
            string[] parts = elementRawContents.Split(' ');

            //iterate over parts, analyzing each.
            foreach (string part in parts)
            {
                //look for equal sign..
                int eqSignIndex = part.IndexOf("=");
                if (eqSignIndex > 0)
                {
                    //got attribute here. store name:
                    string attName = part.Substring(0, eqSignIndex).ToLower();

                    //maybe already exists?
                    if (attributes.ContainsKey(attName))
                        continue;

                    //get value and add to result.
                    string attValue = GetAttributeValue(part, eqSignIndex);
                    attributes.Add(attName, attValue);
                }
            }

            //done.
            return attributes;
        }

        /// <summary>
        /// Parse given content based on index of equal sign and return the value after the equal sign.
        /// </summary>
        /// <param name="rawContent"></param>
        /// <param name="eqSignIndex"></param>
        /// <returns></returns>
        static public string GetAttributeValue(string rawContent, int eqSignIndex)
        {
            //maybe ends with it?
            if (eqSignIndex >= (rawContent.Length - 1))
                return string.Empty;

            //got something.. put aside:
            string value = rawContent.Substring(eqSignIndex + 1, rawContent.Length - eqSignIndex - 1);

            //need to calculate index of value start. default is right after equal sign.
            int startIndex = 0;
            int length = value.Length;

            //might start with quote, single or double:
            if ((value.StartsWith("'") && value.LastIndexOf("'") > 0) ||
                (value.StartsWith("\"") && value.LastIndexOf("\"") > 0))
            {
                startIndex = 1;
                length = (value.StartsWith("'") ? value.LastIndexOf("'") : value.LastIndexOf("\"")) - 1;
            }
            else
            {
                //might end with > or /> so handle this corner.
                if (value.EndsWith("/>"))
                    length -= 2;
                else if (value.EndsWith(">"))
                    length -= 1;
            }

            return value.Substring(startIndex, length);
        }

        /// <summary>
        /// This function will return all occurrences of the string between "start" and "end", including both, inside the given raw contents. 
        /// When no match is found, return empty array.
        /// </summary>
        /// <param name="rawContents">The string we want to analyze</param>
        /// <param name="start">Match start point, look for this string first</param>
        /// <param name="end">Match end point, return anything between start and this value</param>
        /// <returns>All occurrences between start and end</returns>
        static public string[] GetBetween(string rawContents, string start, string end)
        {
            //initialize return value.. can be any number of results
            List<string> results = new List<string>();

            //look for the first occurance. Might not be found, in which case empty array is returned.
            int indexStart = rawContents.IndexOf(start, 0, StringComparison.CurrentCultureIgnoreCase);
            while (indexStart > 0)
            {
                //getting here means "start" was found. look for matching "end"
                int indexEnd = rawContents.IndexOf(end, indexStart + start.Length, StringComparison.CurrentCultureIgnoreCase);
                if (indexEnd > indexStart)
                {
                    //match found! add to resulrs array and update the index:
                    results.Add(rawContents.Substring(indexStart, indexEnd + end.Length - indexStart));
                    indexStart = indexEnd;
                }

                //search for next occurrence of "start" in the string: might reach end of string so avoid error by limiting the count to string length.
                indexStart = rawContents.IndexOf(start, Math.Min(indexStart + 1, rawContents.Length - 1), StringComparison.CurrentCultureIgnoreCase);
            }

            //done. return what we found:
            return results.ToArray();
        }

        /// <summary>
        /// Little function to remove all HTML comments from given string.
        /// </summary>
        /// <param name="html">The raw HTML contents to be cleaned.</param>
        static public void CleanHtmlComments(ref string html)
        {
            int commentStart = html.IndexOf("<!--", 0);
            while (commentStart >= 0)
            {
                int commentEnd = html.IndexOf("-->", commentStart + 3);
                if (commentEnd > commentStart)
                {
                    html = html.Remove(commentStart, commentEnd + 3 - commentStart);
                    commentStart = html.IndexOf("<!--", 0);
                    continue;
                }
                commentStart = html.IndexOf("<!--", Math.Min(commentStart + 1, html.Length - 1));
            }
        }

        static public void WriteToFile(string fullUrl, string errorMessage)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\brokenlinks.txt", true))
            {
                file.WriteLine(fullUrl + " " + errorMessage);
            }

            //inserting result into database
            /*Database db = new Database();
            db.Insert(fullUrl, errorMessage);*/
        }
   

    }
}
