using EscapeFromSupermarket.Controllers;
using Godot;

namespace EscapeFromSupermarket.Core
{
    public static class InteractionGroups
    {
        public const string Targets = "interaction_targets";
    }

    public interface IInteractionTarget
    {
        Node3D TargetNode { get; }
        Vector3 PromptWorldPosition { get; }
        bool RequiresInteract { get; }
        bool IsInteractionAvailable { get; }
        bool IsPlayerInInteractionArea(PlayerController player);
        string GetInteractionPrompt();
        void Interact(PlayerController player);
    }
}
