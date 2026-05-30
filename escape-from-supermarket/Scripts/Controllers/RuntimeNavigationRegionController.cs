using System;
using Godot;

namespace EscapeFromSupermarket.Controllers
{
    public partial class RuntimeNavigationRegionController : NavigationRegion3D
    {
        [Export] public NodePath BakeRootNode { get; set; }

        public override void _Ready()
        {
            if (NavigationMesh == null)
            {
                throw new InvalidOperationException($"{Name}: NavigationMesh is not assigned.");
            }
            if (BakeRootNode == null || BakeRootNode.IsEmpty)
            {
                throw new InvalidOperationException($"{Name}: BakeRootNode is not assigned.");
            }

            var root = GetNode<Node>(BakeRootNode);
            Configure(NavigationMesh);
            var sourceGeometry = new NavigationMeshSourceGeometryData3D();
            NavigationServer3D.ParseSourceGeometryData(NavigationMesh, sourceGeometry, root, default);
            NavigationServer3D.BakeFromSourceGeometryData(NavigationMesh, sourceGeometry, default);
        }

        private static void Configure(NavigationMesh navigationMesh)
        {
            navigationMesh.GeometryParsedGeometryType = NavigationMesh.ParsedGeometryType.StaticColliders;
            navigationMesh.GeometrySourceGeometryMode = NavigationMesh.SourceGeometryMode.RootNodeChildren;
            navigationMesh.CellSize = 0.25f;
            navigationMesh.CellHeight = 0.25f;
            navigationMesh.AgentHeight = 1.75f;
            navigationMesh.AgentRadius = 0.5f;
            navigationMesh.AgentMaxClimb = 0.25f;
            navigationMesh.AgentMaxSlope = 45.0f;
        }
    }
}
