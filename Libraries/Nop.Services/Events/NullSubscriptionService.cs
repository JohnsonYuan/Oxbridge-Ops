using System.Collections.Generic;

namespace Nop.Services.Events
{
    public class NullSubscriptionService : ISubscriptionService
    {
        public IList<IConsumer<T>> GetSubscriptions<T>()
        {
            // do nothing
            return null;
        }
    }
}
