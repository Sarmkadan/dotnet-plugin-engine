public void Subscribe(IEventHandler handler)
{
    if (handler == null)
    {
        throw new ArgumentNullException(nameof(handler));
    }
}
