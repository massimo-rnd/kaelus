using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace kaelus
{
    internal class Engine
    {
        #region statics

        internal static HashSet<string> foundEmails = new HashSet<string>();

        internal static string finishedResults = "";

        #endregion

        #region static methods

        internal static void ExtractSourceCode(string url)
        {

            bool isUrl = IsUrl(url);

            if (isUrl)
            {

            }
            else
            {
                url = "https://" + url;
            }

            // Scan the main index page and get all links on it
            ScanLinks(url);

            // Display the collected emails at the end
            DisplayCollectedEmails();
        }

        public static string kaelusScan(string url)
        {
            bool isUrl = IsUrl(url);

            if (isUrl)
            {

            }
            else
            {
                url = "https://" + url;
            }

            // Scan the main index page and get all links on it
            ScanLinks(url);

            // Display the collected emails at the end
            DisplayCollectedEmails();
            return finishedResults;
        }

        internal static void ScanLinks(String url)
        {
            List<string> links = new List<string>();

            // Fetch page source
            string pageContent = FetchPageContent(url);

            if (string.IsNullOrEmpty(pageContent))
            {
                Console.WriteLine("No content found on the page.");
                return;
            }

            // Use HtmlAgilityPack to parse the HTML and find all links
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(pageContent);

            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                string href = link.GetAttributeValue("href", string.Empty);

                // If it's a relative link, convert it to an absolute URL
                Uri baseUri = new Uri(url);
                Uri fullUri = new Uri(baseUri, href);

                if (fullUri.Host == baseUri.Host) // Ensure it's the same domain
                {
                    links.Add(fullUri.ToString());
                }
            }

            // Process each link to extract email addresses
            foreach (var link in links)
            {
                Console.WriteLine($"Processing link: {link}");

                // Fetch the page content for each link or decode it if it's obfuscated
                if (link.Contains("/cdn-cgi/l/email-protection#"))
                {
                    // Extract and decode the email from the URL fragment
                    string encodedEmail = link.Split('#').Last();
                    string decodedEmail = DecodeCloudflareEmail(encodedEmail);

                    if (!foundEmails.Contains(decodedEmail))
                    {
                        foundEmails.Add(decodedEmail);
                    }
                }
                else
                {
                    // Fetch page content normally if not Cloudflare protected
                    string linkContent = FetchPageContent(link);

                    if (!string.IsNullOrEmpty(linkContent))
                    {
                        // Process the HTML to extract email addresses
                        ProcessHtml(linkContent);
                    }
                }
            }
        }

        // Process HTML to extract both standard and obfuscated emails
        private static void ProcessHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Use regular expressions to find email addresses
            var emailRegex = new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
            var matches = emailRegex.Matches(html);

            foreach (Match match in matches)
            {
                string email = match.Value;
                if (!foundEmails.Contains(email))
                {
                    foundEmails.Add(email);
                }
            }

            // Look for Cloudflare obfuscated emails (data-cfemail attribute)
            var obfuscatedEmailNodes = doc.DocumentNode.SelectNodes("//a[@data-cfemail]");
            if (obfuscatedEmailNodes != null)
            {
                foreach (var node in obfuscatedEmailNodes)
                {
                    var encodedEmail = node.GetAttributeValue("data-cfemail", null);
                    if (!string.IsNullOrEmpty(encodedEmail))
                    {
                        string decodedEmail = DecodeCloudflareEmail(encodedEmail);
                        if (!foundEmails.Contains(decodedEmail))
                        {
                            foundEmails.Add(decodedEmail);

                        }
                    }
                }
            }
        }

        // Method to decode Cloudflare obfuscated emails
        private static string DecodeCloudflareEmail(string encoded)
        {
            int r = Convert.ToInt32(encoded.Substring(0, 2), 16);
            StringBuilder decodedEmail = new StringBuilder();

            for (int i = 2; i < encoded.Length; i += 2)
            {
                int c = Convert.ToInt32(encoded.Substring(i, 2), 16) ^ r;
                decodedEmail.Append((char)c);
            }

            return decodedEmail.ToString();
        }

        // Method to fetch the page content
        internal static string FetchPageContent(string url)
        {
            try
            {
                using (HttpClient client = new())
                {
                    HttpResponseMessage response = client.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        return response.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {
                        Console.WriteLine($"Failed to fetch {url}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching page content: {ex.Message}");
            }
            return string.Empty;
        }

        // Method to collect emails found on a page
        internal static void GetMails(String sourceCode)
        {
            string emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";

            // Use Regex.Matches to find all occurrences of email addresses in the input string
            MatchCollection matches = Regex.Matches(sourceCode, emailPattern, RegexOptions.IgnoreCase);

            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    // Add to the HashSet to ensure uniqueness
                    foundEmails.Add(match.Value);
                }
            }
        }

        // Display collected emails and filter duplicates
        internal static void DisplayCollectedEmails()
        {
            if (foundEmails.Count > 0)
            {
                //Console.WriteLine(Color.green("Good news! I found the following unique email addresses:"));
                foreach (var email in foundEmails)
                {
                    Console.WriteLine(email);
                    finishedResults = finishedResults + email + "\n";
                }
            }
            else
            {
                //Console.WriteLine(Color.red("No email addresses found."));
                finishedResults = "No email addresses found";
            }
        }

        static bool IsUrl(string input)
        {
            return Uri.TryCreate(input, UriKind.Absolute, out Uri result) &&
                   (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }

        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmss");
        }

        #endregion
    }
}
