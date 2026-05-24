using EscapeFromSupermarket.Architecture;
using Godot;

namespace EscapeFromSupermarket.Controllers
{
    public partial class GameRoot : Node
    {
        private Supermarket _arch;

        public override void _Ready()
        {
            _arch = Supermarket.Instance;
        }

        public override void _Process(double delta)
        {
            _arch?.Tick(delta);
        }
    }
}
