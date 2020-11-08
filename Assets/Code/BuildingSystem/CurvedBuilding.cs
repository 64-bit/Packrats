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

        private Mesh _gizmoMesh;

        public void BendCards(CaveSystemSettings caveSettings)
        {

        }

        public void UnBend()
        {

        }

        public void OnEdited()
        {
            _gizmoMesh = GenerateGizmoMesh();
        }

        private Mesh GenerateGizmoMesh()
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireMesh(_gizmoMesh, transform.position, transform.rotation);
        }
    }
}