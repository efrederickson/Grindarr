using System;

namespace Grindarr.Core.Net
{
    public class ResponseFilenameEventArgs : EventArgs
    {
        public string Filename { get; }

        public ResponseFilenameEventArgs(string fn)
        {
            Filename = fn;
        }
    }
}
