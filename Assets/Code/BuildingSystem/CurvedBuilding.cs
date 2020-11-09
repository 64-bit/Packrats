using System.Collections;
using System.Collections.Generic;
using Packrats;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Packrats
{
    public class CurvedBuilding : MonoBehaviour
    {
        public int BuildingSizeInSegments = 4;
        public int BuildingDepthInSegments = 1;

        public int BuildingID { get; private set; }

        public int FloorIndex { get; private set; }

        /// <summary>
        /// Index of the segment this building occupies with the lowest radial polar co-oridinate
        /// </summary>
        public int InitialSegment { get; private set; }

        private Mesh _mesh;

        private BuildingCard[] _buildingCards;

        public void OnEnable()
        {
            _buildingCards = GetComponentsInChildren<BuildingCard>();
        }

        public void InitBuilding(int buildingID, int floorIndex, int initialSegment)
        {
            BuildingID = buildingID;
            FloorIndex = floorIndex;
            BuildingID = initialSegment;
        }

        public void OnEdited()
        {
            _mesh = GenerateMesh();
        }

        private Mesh GenerateMesh()
        {
            var nativeMesh = new NativeMeshData(Allocator.TempJob);

            var caveSettings = CaveSystemSettings.DefaultSettings;

            CylinderMesher.AppendWalls(ref nativeMesh, caveSettings, 0, BuildingSizeInSegments, caveSettings.InnerRadius, 0.0f, caveSettings.RoomHeight);
            CylinderMesher.AppendWalls(ref nativeMesh, caveSettings, 0, BuildingSizeInSegments, caveSettings.OuterRadius, 0.0f, caveSettings.RoomHeight);

            CylinderMesher.AppendCap(ref nativeMesh, caveSettings, 0, BuildingSizeInSegments, caveSettings.InnerRadius, caveSettings.OuterRadius, 0.0f, false);
            CylinderMesher.AppendCap(ref nativeMesh, caveSettings, 0, BuildingSizeInSegments, caveSettings.InnerRadius, caveSettings.OuterRadius, caveSettings.RoomHeight, true);

            float4x4 transformMatrix = Matrix4x4.Translate(new Vector3(-caveSettings.InnerRadius, 0.0f, 0.0f));

            NativeMeshTransformer.TransformMesh(ref nativeMesh, ref transformMatrix, default).Complete();

            var mesh = new Mesh();
            nativeMesh.ApplyToMesh(mesh);
            nativeMesh.Dispose();

            return mesh;
        }

        public void SetColor(Color color)
        {
            foreach (var card in _buildingCards)
            {
                card.SetColor(color);
            }
        }

        public void ResetColor()
        {
            foreach (var card in _buildingCards)
            {
                card.Resetcolor();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireMesh(_mesh, transform.position, transform.rotation);
        }
    }
}