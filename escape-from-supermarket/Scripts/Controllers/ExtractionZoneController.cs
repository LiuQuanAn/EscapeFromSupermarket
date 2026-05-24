using EscapeFromSupermarket.Architecture;
using EscapeFromSupermarket.Commands;
using EscapeFromSupermarket.Config;
using EscapeFromSupermarket.Core;
using EscapeFromSupermarket.Models;
using Godot;
using QFramework;

namespace EscapeFromSupermarket.Controllers
{
    public partial class ExtractionZoneController : Area3D, IController, IInteractionTarget
    {
        [Export] public ExtractionExitType ExitType { get; set; } = ExtractionExitType.FrontDoor;

        private PrototypeBalance _balance = PrototypeBalance.Default;
        private RoundObjectiveModel _objective;

        public Node3D TargetNode => this;
        public Vector3 PromptWorldPosition => GlobalPosition + Vector3.Up * 0.8f;
        public float InteractionRange => _balance.InteractionRange;
        public bool RequiresInteract => false;
        public bool IsInteractionAvailable => Visible;

        public override void _Ready()
        {
            _balance = this.GetUtility<PrototypeBalance>();
            _objective = this.GetModel<RoundObjectiveModel>();
            AddToGroup(InteractionGroups.Targets);
            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;
        }

        private void OnBodyEntered(Node3D body)
        {
            if (body is PlayerController)
            {
                this.SendCommand(new StartExtractionCommand(ExitType));
            }
        }

        private void OnBodyExited(Node3D body)
        {
            if (body is PlayerController)
            {
                this.SendCommand(new CancelExtractionCommand());
            }
        }

        public string GetInteractionPrompt()
        {
            if (ExitType == ExtractionExitType.StaffDoor && (_objective == null || !_objective.HasKeycard.Value))
            {
                return "员工门 [需要钥匙卡]";
            }

            return ExitType == ExtractionExitType.StaffDoor
                ? "员工门 [停留撤离]"
                : "正门 [停留撤离]";
        }

        public void Interact(PlayerController player)
        {
            // Extraction starts by staying inside the zone; no active interact action.
        }

        public IArchitecture GetArchitecture() => Supermarket.Interface;
    }
}
