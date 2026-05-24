using EscapeFromSupermarket.Architecture;
using EscapeFromSupermarket.Commands;
using Godot;
using QFramework;

namespace EscapeFromSupermarket.Controllers
{
    public partial class ExtractionZoneController : Area3D, IController
    {
        public override void _Ready()
        {
            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;
        }

        private void OnBodyEntered(Node3D body)
        {
            if (body is PlayerController)
            {
                this.SendCommand(new StartExtractionCommand());
            }
        }

        private void OnBodyExited(Node3D body)
        {
            if (body is PlayerController)
            {
                this.SendCommand(new CancelExtractionCommand());
            }
        }

        public IArchitecture GetArchitecture() => Supermarket.Interface;
    }
}
