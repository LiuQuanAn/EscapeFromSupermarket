using EscapeFromSupermarket.Architecture;
using EscapeFromSupermarket.Commands;
using EscapeFromSupermarket.Config;
using EscapeFromSupermarket.Core;
using EscapeFromSupermarket.Models;
using Godot;
using QFramework;

namespace EscapeFromSupermarket.Controllers
{
    public partial class KeycardController : Area3D, IController, IInteractionTarget
    {
        private PrototypeBalance _balance = PrototypeBalance.Default;
        private RoundObjectiveModel _objective;

        public Node3D TargetNode => this;
        public Vector3 PromptWorldPosition => GlobalPosition + Vector3.Up * 0.6f;
        public float InteractionRange => _balance.InteractionRange;
        public bool RequiresInteract => true;
        public bool IsInteractionAvailable => Visible && !_objective.HasKeycard.Value;

        public override void _Ready()
        {
            _balance = this.GetUtility<PrototypeBalance>();
            _objective = this.GetModel<RoundObjectiveModel>();
            AddToGroup(InteractionGroups.Targets);
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
