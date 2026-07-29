public void Publish(TEvent @event)
{
    if (@event == null)
    {
        throw new ArgumentNullException(nameof(@event));
    }
}
