using Godot;

namespace EscapeFromSupermarket.Config
{
    [GlobalClass]
    public partial class ShelfSpawnEntryResource : Resource
    {
        [Export] public string ProductTypeId { get; set; } = string.Empty;
        [Export] public int Weight { get; set; } = 1;
    }
}
