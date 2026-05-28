using EscapeFromSupermarket.Architecture;
using EscapeFromSupermarket.Commands;
using EscapeFromSupermarket.Core;
using EscapeFromSupermarket.Models;
using Godot;
using QFramework;

namespace EscapeFromSupermarket.Controllers
{
    public partial class KeycardController : Area3D, IController, IInteractionTarget
    {
        private RoundObjectiveModel _objective;
        private bool _playerInside;

        public Node3D TargetNode => this;
        public Vector3 PromptWorldPosition => GlobalPosition + Vector3.Up * 0.6f;
        public bool RequiresInteract => true;
        public bool IsInteractionAvailable => Visible && !_objective.HasKeycard.Value;

        public override void _Ready()
        {
            _objective = this.GetModel<RoundObjectiveModel>();
            AddToGroup(InteractionGroups.Targets);
            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;
        }

        public bool IsPlayerInInteractionArea(PlayerController player)
        {
            return _playerInside;
        }

        private void OnBodyEntered(Node3D body)
        {
            if (body is PlayerController) _playerInside = true;
        }

        private void OnBodyExited(Node3D body)
        {
            if (body is PlayerController) _playerInside = false;
        }

        public string GetInteractionPrompt()
        {
            return "钥匙卡 [E]";
        }

        public void Interact(PlayerController player)
        {
            if (!IsInteractionAvailable) return;
            this.SendCommand(new CollectKeycardCommand());
            Visible = false;
        }

        public IArchitecture GetArchitecture() => Supermarket.Interface;
    }
}
