using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Packrats
{
    public class NativeMeshTransformer
    {
        /// <summary>
        /// DO NOT USE A NON-UNIFORM SCALE
        /// </summary>
        public static JobHandle TransformMesh(
            ref NativeMeshData nativeMesh,
            ref float4x4 transform,
            JobHandle lastJobHandle)
        {
            var job = new TransformMeshJob()
            {
                Positions = nativeMesh.Positions.AsDeferredJobArray(),
                Normals = nativeMesh.Normals.AsDeferredJobArray(),
                Transform = transform
            };

            return job.Schedule(lastJobHandle);
        }

        [BurstCompile]
        private struct TransformMeshJob : IJob //IJob, because otherwise we have to stall to scheudle proper length
        {
            public NativeArray<float3> Positions;
            public NativeArray<float3> Normals;

            public float4x4 Transform;

            public void Execute()
            {
                for (int i = 0; i < Positions.Length; i++)
                {
                    Positions[i] = math.mul(Transform, new float4(Positions[i], 1.0f)).xyz;
                    Normals[i] = math.mul(Transform, new float4(Normals[i], 0.0f)).xyz;
                }
            }
        }
    }
}