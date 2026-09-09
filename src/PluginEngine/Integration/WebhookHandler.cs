/// <summary>
/// Represents a webhook handler for processing plugin events.
/// </summary>
public class WebhookHandler : IEquatable<WebhookHandler>
{
    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other"> parameter; otherwise, false.
    /// </returns>
    public bool Equals(WebhookHandler? other)
    {
        return PluginId == other?.PluginId && EventType == other?.EventType && TimestampUtc == other?.TimestampUtc && Data == other?.Data;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>
    /// true if the specified object  is equal to the current object; otherwise, false.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as WebhookHandler);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>
    /// A hash code for the current object.
    /// </returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(PluginId, EventType, TimestampUtc, Data);
    }

    /// <summary>
    /// Determines whether two specified objects are equal.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns>
    /// true if the objects are equal; otherwise, false.
    /// </returns>
    public static bool operator ==(WebhookHandler? left, WebhookHandler? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Determines whether two specified objects are not equal.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns>
    /// true if the objects are not equal; otherwise, false.
    /// </returns>
    public static bool operator !=(WebhookHandler? left, WebhookHandler? right)
    {
        return !Equals(left, right);
    }
}