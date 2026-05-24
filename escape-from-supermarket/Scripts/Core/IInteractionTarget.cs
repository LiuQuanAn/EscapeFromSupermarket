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
        float InteractionRange { get; }
        bool RequiresInteract { get; }
        bool IsInteractionAvailable { get; }
        string GetInteractionPrompt();
        void Interact(PlayerController player);
    }
}
