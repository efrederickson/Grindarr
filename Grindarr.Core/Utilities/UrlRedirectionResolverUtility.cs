using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Grindarr.Core.Utilities
{
    public static class UrlRedirectionResolverUtility
    {
        /// <summary>
        /// This method will resolve Uri into its final destination Uri by following all 300-type HTTP redirects
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static async Task<Uri> ResolveAsync(Uri source)
        {
            // Due to how much this slows down responses (obviously), i've disabled it for now...
            // Additionally GetComics seems to provide a one time use URL, so this breaks that
            // TODO: ... ?

            var req = WebRequest.CreateHttp(source);
            req.Method = "HEAD";

            try
            {
                var res = req.GetResponseAsync();
                return (await res).ResponseUri;
            }
            catch (WebException ex)
            {
                var httpResp = ((HttpWebResponse)ex.Response);
                if ((int)httpResp.StatusCode >= 300 && (int)httpResp.StatusCode < 400)
                {
                    var key = httpResp.Headers.AllKeys.Where(header => header.Equals("Location", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (!string.IsNullOrEmpty(key))
                    {
                        var redirect = httpResp.GetResponseHeader(key);
                        Console.WriteLine("Following redirect to: " + redirect);
                        var newUrl = new Uri(redirect);
                        return await ResolveAsync(newUrl);
                    }
                }
                return source;
            }
        }
    }
}
