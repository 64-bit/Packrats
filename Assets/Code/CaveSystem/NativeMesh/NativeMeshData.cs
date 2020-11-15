using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Packrats
{
    public struct NativeMeshData
    {
        public NativeList<float3> Positions;
        public NativeList<float3> Normals;
        public NativeList<float2> UVs;

        public NativeList<int> Indicies;

        public NativeMeshData(Allocator allocator)
        {
            Positions = new NativeList<float3>(allocator);
            Normals = new NativeList<float3>(allocator);
            UVs = new NativeList<float2>(allocator);
            Indicies = new NativeList<int>(allocator);
        }

        public void Dispose()
        {
            Positions.Dispose();
            Normals.Dispose();
            UVs.Dispose();
            Indicies.Dispose();
        }

        public void ApplyToMesh(Mesh targetMesh)
        {
            targetMesh.SetVertices(Positions.AsArray());
            targetMesh.SetUVs(0, UVs.AsArray());
            targetMesh.SetNormals(Normals.AsArray());
            targetMesh.SetIndices(Indicies.AsArray(), MeshTopology.Triangles, 0);

            targetMesh.RecalculateTangents();
            targetMesh.UploadMeshData(true);
        }
    }
}
