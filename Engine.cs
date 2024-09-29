using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace kaelus
{
    internal class Engine
    {
        internal static String GrabbedMails;

        internal static String CheckUrl(String url)
        {
            string urlString = url;
            bool isUrl = IsUrl(urlString);

            if (isUrl)
            {
                Console.WriteLine($"{urlString} is a valid URL.");
                return url;
            }
            else
            {
                Console.WriteLine($"You provided an invalid url. {urlString} is not a valid URL.");
                //System.Environment.Exit(0);
                return "invalid";
            }
        }

        internal static String ExtractEmails(String url)
        {
            // Extract Sourcecode
            GetSourceCode(url);
            String sourceCode = GetSourceCode(url);

            // Extract E-Mails
            String extractedMails = GetEmails(sourceCode);

            // Remove Duplicates
            String cleanResults = RemoveDuplicates(GrabbedMails);

            return cleanResults;
            //return sourceCode;//TODO: Debug
        }

        internal static String GetSourceCode(String url)
        {
            using (HttpClient client = new())
            {
                using (HttpResponseMessage response = client.GetAsync(url).Result)
                {
                    using (HttpContent content = response.Content)
                    {
                        string sourceCode = content.ReadAsStringAsync().Result;
                        return sourceCode;
                    }
                }
            }
        }

        internal static String GetEmails(String sourceCode)
        {
            string emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";

            // Use Regex.Matches to find all occurrences of email addresses in the input string
            MatchCollection matches = Regex.Matches(sourceCode, emailPattern, RegexOptions.IgnoreCase);

            if (matches.Count > 0)
            {
                Console.WriteLine("Good news! I found some email-addresses");
                string prevMail = "";
                foreach (Match match in matches)
                {
                    //Check if has been output already
                    if (match.Value.Equals(prevMail))
                    {
                        //skip to next
                    }
                    else
                    {
                        //add match to string
                        GrabbedMails = GrabbedMails + match + System.Environment.NewLine;
                        Console.WriteLine(match.Value);
                    }
                }
                return GrabbedMails;
            }
            else
            {
                Console.WriteLine("No email addresses found.");
                return "No email addresses found.";
            }
        }

        static bool IsUrl(string input)
        {
            return Uri.TryCreate(input, UriKind.Absolute, out Uri result) &&
                   (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }

        internal static String RemoveDuplicates(String emails)
        {
            string input = emails;
            string output = Regex.Replace(input, @"\b(\w+)\s+\1\b", "$1");
            return output;
        }
    }
}
