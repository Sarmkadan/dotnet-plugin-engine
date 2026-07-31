public class WebhookHandler : IEquatable<WebhookHandler>
{
    public bool Equals(WebhookHandler? other)
    {
        return PluginId == other?.PluginId && EventType == other?.EventType && TimestampUtc == other?.TimestampUtc && Data == other?.Data;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as WebhookHandler);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PluginId, EventType, TimestampUtc, Data);
    }

    public static bool operator ==(WebhookHandler? left, WebhookHandler? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(WebhookHandler? left, WebhookHandler? right)
    {
        return !Equals(left, right);
    }
}
