using System;
using System.Collections.Generic;
using EscapeFromSupermarket.Architecture;
using EscapeFromSupermarket.Config;
using EscapeFromSupermarket.Controllers.UI;
using EscapeFromSupermarket.Core;
using EscapeFromSupermarket.Models;
using Godot;
using QFramework;

namespace EscapeFromSupermarket.Controllers
{
    public partial class ShelfController : Area3D, IController, IInteractionTarget
    {
        [Export] public int ShelfId { get; set; }
        [Export] public string ShelfName { get; set; } = "货架";
        [Export] public int MinItemCount { get; set; } = 2;
        [Export] public int MaxItemCount { get; set; } = 4;
        [Export] public Godot.Collections.Array<ShelfSpawnEntryResource> SpawnOptions { get; set; } = new();

        private ShelfPanelController _panel;
        private bool _playerInside;
        public Node3D TargetNode => this;
        public Vector3 PromptWorldPosition => GlobalPosition + Vector3.Up * 1.1f;
        public bool RequiresInteract => true;
        public bool IsInteractionAvailable => Visible;

        public override void _Ready()
        {
            if (ShelfId <= 0)
            {
                throw new InvalidOperationException(
                    $"ShelfController '{Name}' requires ShelfId > 0 in the scene; got {ShelfId}.");
            }

            this.GetModel<ShelfModel>().RegisterShelfConfig(
                ShelfId,
                MinItemCount,
                MaxItemCount,
                BuildSpawnOptions());

            AddToGroup(InteractionGroups.Targets);
            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;
            _panel = GetTree().CurrentScene?.GetNodeOrNull<ShelfPanelController>("UI/ShelfPanel");
        }

        private IReadOnlyList<ShelfSpawnOption> BuildSpawnOptions()
        {
            if (MinItemCount < 0)
            {
                throw new InvalidOperationException($"ShelfController '{Name}' MinItemCount must be >= 0; got {MinItemCount}.");
            }

            if (MaxItemCount < MinItemCount)
            {
                throw new InvalidOperationException($"ShelfController '{Name}' MaxItemCount must be >= MinItemCount.");
            }

            if (SpawnOptions.Count == 0)
            {
                throw new InvalidOperationException($"ShelfController '{Name}' requires at least one SpawnOptions entry.");
            }

            var options = new List<ShelfSpawnOption>(SpawnOptions.Count);
            for (int i = 0; i < SpawnOptions.Count; i++)
            {
                var entry = SpawnOptions[i];
                if (entry == null)
                {
                    throw new InvalidOperationException($"ShelfController '{Name}' SpawnOptions[{i}] is empty.");
                }

                if (string.IsNullOrWhiteSpace(entry.ProductTypeId))
                {
                    throw new InvalidOperationException($"ShelfController '{Name}' SpawnOptions[{i}].ProductTypeId is empty.");
                }

                if (entry.Weight <= 0)
                {
                    throw new InvalidOperationException($"ShelfController '{Name}' SpawnOptions[{i}].Weight must be > 0.");
                }

                options.Add(new ShelfSpawnOption(entry.ProductTypeId, entry.Weight));
            }

            return options;
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
