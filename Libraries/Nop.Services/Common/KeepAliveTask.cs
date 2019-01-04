using System;
using System.Net;
using Nop.Core;
using Nop.Services.Logging;
using Nop.Services.Tasks;

namespace Nop.Services.Common
{
    public class KeepAliveTask : ITask
    {
        private readonly IStoreContext _storeContext;

        public KeepAliveTask(IStoreContext storeContext)
        {
            this._storeContext = storeContext;
        }

        public string Name => "Keey alive";

        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            string url = _storeContext.CurrentStore.Url + "keepalive/index";
            using (var wc = new WebClient())
            {
                wc.DownloadString(url);

                //log
                //var logger = Core.Infrastructure.EngineContext.Current.Resolve<Nop.Services.Logging.ILogger>();
                //logger.Information("keep alive request done " + System.IO.Directory.GetCurrentDirectory(), null, null);
            }
        }
    }
}
