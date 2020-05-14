using Grindarr.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grindarr.Soulseek
{
    public class SoulseekContentItem : ContentItem
    {
        /// <summary>
        /// The username that the file belongs to
        /// </summary>
        public string SoulseekUsername { get; set; }

        /// <summary>
        /// The soulseek full filename, which may be different than the generated URI
        /// </summary>
        public string SoulseekFilename { get; set; }
    }
}
