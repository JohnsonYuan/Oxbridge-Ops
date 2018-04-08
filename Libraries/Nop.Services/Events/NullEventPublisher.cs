namespace Nop.Services.Events
{
    public class NullEventPublisher : IEventPublisher
    {
        public void Publish<T>(T eventMessage)
        {
            // do nothing
        }
    }
}
