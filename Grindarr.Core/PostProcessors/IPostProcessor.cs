namespace Grindarr.Core.PostProcessors
{
    public interface IPostProcessor
    {
        /// <summary>
        /// The priority of the post processor, from 1 to 100, where lower is earlier
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// Whether the post processor is required to run, no matter what (e.g. move output)
        /// </summary>
        public bool Mandatory { get; }

        /// <summary>
        /// Whether the post processor is enabled or not (this should be controlled outside of your class and may be ignored if Mandatory is true)
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Run the post processor on the specified completed item
        /// </summary>
        /// <param name="item">The completed download item</param>
        public void Run(IDownloadItem item);

        /// <summary>
        /// Title of the post processor (for UI)
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Description of the post processor (for UI)
        /// </summary>
        public string Description { get; }
    }
}
