using System;
using EscapeFromSupermarket.Architecture;
using EscapeFromSupermarket.Config;
using EscapeFromSupermarket.Controllers.UI;
using EscapeFromSupermarket.Core;
using Godot;
using QFramework;

namespace EscapeFromSupermarket.Controllers
{
    public partial class ShelfController : Area3D, IController, IInteractionTarget
    {
        [Export] public int ShelfId { get; set; }
        [Export] public string ShelfName { get; set; } = "货架";

        private PrototypeBalance _balance = PrototypeBalance.Default;
        private ShelfPanelController _panel;
        public Node3D TargetNode => this;
        public Vector3 PromptWorldPosition => GlobalPosition + Vector3.Up * 1.1f;
        public float InteractionRange => _balance.InteractionRange;
        public bool RequiresInteract => true;
        public bool IsInteractionAvailable => Visible;

        public override void _Ready()
        {
            _balance = this.GetUtility<PrototypeBalance>();
            if (ShelfId <= 0)
            {
                throw new InvalidOperationException(
                    $"ShelfController '{Name}' requires ShelfId > 0 in the scene; got {ShelfId}.");
            }

            AddToGroup(InteractionGroups.Targets);
            _panel = GetTree().CurrentScene?.GetNodeOrNull<ShelfPanelController>("UI/ShelfPanel");
        }

        public string GetInteractionPrompt()
        {
            return $"{ShelfName} [E]";
        }

        public void Interact(PlayerController player)
        {
            _panel ??= GetTree().CurrentScene?.GetNodeOrNull<ShelfPanelController>("UI/ShelfPanel");
            _panel?.ToggleShelf(this);
        }

        public IArchitecture GetArchitecture() => Supermarket.Interface;
    }
}
