using EscapeFromSupermarket.Models;

namespace EscapeFromSupermarket.Events
{
    public sealed record RoundEndedEvent(RoundResult Result);
}
