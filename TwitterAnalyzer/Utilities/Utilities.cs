using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterAnalyzer.Utilities
{
    public static class Utilities
    {

        public static string ExtractTwitterHandleFromEndOfUrl(string url)
        {
            try
            {
                // Check if the URL is valid
                if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
                {
                    // Get the last segment of the URL
                    string path = uri.AbsolutePath;

                    // Remove the leading slash, if any
                    return path.TrimStart('/');
                }
                else
                {
                    throw new ArgumentException("Invalid URL format.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        // Extract Twitter handles/URLs from social media links
        public static List<string> ExtractTwitterHandles(string socialMediaURL)
        {
            try
            {
                return socialMediaURL
                    .Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(url => url.Contains("twitter.com") || url.Contains("x.com"))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                throw;
            }
        }
    }
}
