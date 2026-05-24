namespace EscapeFromSupermarket.Events
{
    public sealed record PickFailedEvent(int ShelfId, int InstanceId, string Reason);
}
