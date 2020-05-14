using System.IO;

namespace Grindarr.Core.PostProcessors
{
    class MoveOutputPostProcessor : IPostProcessor
    {
        public int Priority => 0;
        public bool Mandatory => true;
        public bool Enabled { get; set; } = true;

        public string Title => "Move Output";
        public string Description => "Move output file to destination folder";

        public void Run(IDownloadItem item)
        {
            File.Move(item.GetDownloadingPath(), item.GetCompletedPath());
        }
    }
}
