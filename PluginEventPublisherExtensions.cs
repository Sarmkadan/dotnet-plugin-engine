public void SubscribeAsync(IEventHandler handler)
{
    if (handler == null)
    {
        throw new ArgumentNullException(nameof(handler));
    }
}
