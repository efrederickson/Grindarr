using Grindarr.Core.Downloaders.Implementations;
using Grindarr.Core.Net;
using HtmlAgilityPack;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Grindarr.Core.Downloaders.Zippyshare
{
    public class ZippyshareDownloader : GenericDownloader, IDownloader
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public async override void SetItem(IDownloadItem item) => base.SetItem(item, await GetActualDownloadUri(item.DownloadUri));

        private async Task<Uri> GetActualDownloadUri(Uri zippysharePage)
        {
            var httpResponse = await httpClient.GetAsync(zippysharePage);
            var responseBodyText = httpResponse.Content.ReadAsStringAsync();

            // Zippyshare does this interesting thing where they change the link via JS and some math to the actual link
            // Clever
            // For example: 
            /*
            <script type="text/javascript">
                document.getElementById('dlbutton').href = "/d/aSmvXalA/" + (844037 % 51245 + 844037 % 913) + "/...filename";
            </script>
            */
            // Nothing a little bit of fragile pattern matching can't beat!
            TryParseLink1(await responseBodyText, out string link);

            // If unable to get value, will raise an exception when it tries to create the URI from the href (i hope)
            if (string.IsNullOrEmpty(link))
            {
                var document = new HtmlDocument();
                document.LoadHtml(await responseBodyText);
                link = document.GetElementbyId("dlButton").GetAttributeValue("href", "<unable to scrape zippyshare link>");
            }
            
            return new Uri(zippysharePage, link);
        }

        private bool TryParseLink1(string responseBody, out string parsed)
        {
            // \"(\/d\/\w+\/)\"\s\+\s\((\d+)\s%\s(\d+)\s\+\s(\d+)\s%\s(\d+)\)\s\+\s\"([^\"]+)\";
            const string rs = @"\""(\/d\/\w+\/)\""\s\+\s\((\d+)\s%\s(\d+)\s\+\s(\d+)\s%\s(\d+)\)\s\+\s\""([^\""]+)\"";";
            Regex regex = new Regex(rs);
            //var c = "document.getElementById('dlbutton').href = \"/d/aSmvXalA/\" + (844037 % 51245 + 844037 % 913) + \"/...filename\";";
            //var c = "document.getElementById('dlbutton').href = \"/d/aSmvXalA/\" + (521588 % 51245 + 521588 % 913) + \"/War%20Is%20Hell%20The%20First%20Flight%20of%20the%20Phantom%20Eagle%20%282020%29%20%28FM%20TPB%29.cbr\";";
            if (!regex.IsMatch(responseBody))
            {
                parsed = null;
                return false;
            }

            var match = regex.Match(responseBody);
            var first = match.Groups[1].Value;
            var a = int.Parse(match.Groups[2].Value);
            var b = int.Parse(match.Groups[3].Value);
            var c = int.Parse(match.Groups[4].Value);
            var d = int.Parse(match.Groups[5].Value);
            var computed = a % b + c % d;
            var last = match.Groups[6].Value;

            parsed = first + computed + last;
            return true;
        }
    }
}
