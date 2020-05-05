using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grindarr.Core.Scrapers
{
    public class ScraperModel
    {
        public string ClassName { get; set; }
        public IEnumerable<string> Arguments { get; set; }
    }
}
