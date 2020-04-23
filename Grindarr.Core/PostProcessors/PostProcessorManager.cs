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
        /// This method is used to safely update the enabled state of a post processor registered to this PostProcessorManager. 
        /// It returns a boolean equivalent to whether the enabled state was updated or not. If false, it may be because 
        /// the post processor given is not assigned to this PostProcessorManager or the post processor is marked as mandatory. 
        /// Note: updating the enableState to 'false' will return 'true' because it successfully updated the state. 
        /// </summary>
        /// <param name="pp"></param>
        /// <param name="enableState"></param>
        /// <returns></returns>
        public bool SetPostProcessorEnableState(IPostProcessor pp, bool enableState)
        {
            if (!PostProcessors.Contains(pp))
                return false;
            if (pp.Mandatory)
                return false;
            pp.Enabled = enableState;
            return true;
        }
    }
}
