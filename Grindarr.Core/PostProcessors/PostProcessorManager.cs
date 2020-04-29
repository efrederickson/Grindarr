using System;
using System.Collections.Generic;
using System.Linq;

namespace Grindarr.Core.PostProcessors
{
    public class PostProcessorManager
    {
        private static PostProcessorManager _instance = null;
        public static PostProcessorManager Instance => _instance ??= new PostProcessorManager();

        private PostProcessorManager()
        {
            PostProcessors.Add(new MoveOutputPostProcessor());
        }

        public List<IPostProcessor> PostProcessors { get; } = new List<IPostProcessor>();

        public void Run(DownloadItem item)
        {
            PostProcessors.Where(pp => pp.Enabled || pp.Mandatory).ToList().ForEach(pp => pp.Run(item));
        }

        /// <summary>
        /// This method is used to update the enabled state of a post processor registered to this PostProcessorManager.
        /// It will raise an exception if the post processor cannot be safely updated (e.g. it's mandatory). 
        /// </summary>
        /// <param name="pp"></param>
        /// <param name="enableState"></param>
        /// <returns></returns>
        public void SetPostProcessorEnableState(IPostProcessor pp, bool enableState)
        {
            if (!PostProcessors.Contains(pp))
                throw new InvalidOperationException($"Post Processor {pp} does not belong to this manager");
            if (pp.Mandatory)
                throw new InvalidOperationException("Unable to change the enabled state of a mandatory post processor");
            pp.Enabled = enableState;
        }
    }
}
