using System;
using System.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Packrats
{
    /// <summary>
    /// Represents a single floor in the cave systen.
    ///
    /// Probably mostly used as a interface to modify underlying simulation data
    /// </summary>
    public class CaveFloor : MonoBehaviour
    {

        private CaveSystem _parentSystem;
        private CaveSystemSettings _caveSettings;
        private CaveSystemData _caveData;

        private int _floorIndex;
        private bool _isInit = false;

        private MeshRenderer _floorRenderer;
        private MeshFilter _floorFilter;

        public void InitFloor(CaveSystem parentSystem, CaveSystemSettings systemSettings, CaveSystemData caveData, int floorIndex)
        {
            if (_isInit)
            {
                throw new InvalidOperationException("Floor is already initialized");
            }

            _floorRenderer = gameObject.AddComponent<MeshRenderer>();
            _floorRenderer.material = parentSystem.CaveMaterial;
            _floorFilter = gameObject.AddComponent<MeshFilter>();

            _parentSystem = parentSystem;
            _caveSettings = systemSettings;
            _floorIndex = floorIndex;
            _caveData = caveData;

            AddFloorToCaveData();
            RemeshFloor();

            _isInit = true;
        }

        private void AddFloorToCaveData()
        {
            _caveData.LastJob.Complete();
            for (int i = 0; i < _caveSettings.RadialSegments; i++)
            {
                _caveData.RadialSegmentDepth.Add(0);
            }
        }

        private void RemeshFloor()
        {
            var computeMeshJob = new MeshFloorJob()
            {
                RadialSegmentDepth = _caveData.RadialSegmentDepth,
                FloorIndex = _floorIndex,
                MeshData = new NativeMeshData(Allocator.TempJob),
                CaveSettings = _caveSettings
            };

            var computeMeshHandle = computeMeshJob.Schedule(_caveData.LastJob);
            _caveData.LastJob = computeMeshHandle;

            StartCoroutine(ApplyMeshUpdateWhenReady(computeMeshJob, computeMeshHandle));
        }

        public void SetSegmentDepth(int segment, int segmetnDepth)
        {
            _caveData.LastJob.Complete();
            int index = _caveSettings.RadialSegments * _floorIndex + segment;
            _caveData.RadialSegmentDepth[index] = segmetnDepth;
            RemeshFloor();
        }

        private IEnumerator ApplyMeshUpdateWhenReady(MeshFloorJob job, JobHandle jobHandle)
        {
            while (!jobHandle.IsCompleted)
            {
                yield return null;
            }
            jobHandle.Complete();

            var resultMesh = new Mesh();
            job.MeshData.ApplyToMesh(resultMesh);
            _floorFilter.mesh = resultMesh;

            job.MeshData.Dispose();
        }

        //[BurstCompile]
        struct MeshFloorJob : IJob
        {
            public NativeArray<int> RadialSegmentDepth;

            public int FloorIndex;

            public NativeMeshData MeshData;

            public CaveSystemSettings CaveSettings;

            public void Execute()
            {
                //Do Floor
                CylinderMesher.AppendCap(ref MeshData, CaveSettings, 0, CaveSettings.RadialSegments, CaveSettings.InnerRadius, CaveSettings.OuterRadius, 0.0f, false);
                CylinderMesher.AppendWalls(ref MeshData, CaveSettings, 0, CaveSettings.RadialSegments, CaveSettings.InnerRadius, 0.0f, CaveSettings.FloorThickness);

                int position = 0;


                int GetRunLength(int start, CaveSystemSettings settings, int floor,
                    ref NativeArray<int> radialSegmentDepth)
                {
                    int run = 0;
                    int current = radialSegmentDepth[floor * settings.RadialSegments + start];
                    for (int i = start; i < settings.RadialSegments; i++)
                    {
                        if (radialSegmentDepth[floor * settings.RadialSegments + i] != current)
                        {
                            return run;
                        }

                        run += 1;
                    }
                    return run;
                }

                while (position < CaveSettings.RadialSegments)
                {
                    int runSize = GetRunLength(position, CaveSettings, FloorIndex, ref RadialSegmentDepth);

                    int currentRunDepth = RadialSegmentDepth[FloorIndex * CaveSettings.RadialSegments + position];


                    float wallRadius = currentRunDepth == 0 ? CaveSettings.InnerRadius : CaveSettings.OuterRadius;

                    CylinderMesher.AppendWalls(ref MeshData, CaveSettings, position, runSize, wallRadius, CaveSettings.FloorThickness, CaveSettings.RoomHeight);

                    if (currentRunDepth != 0)
                    {
                        CylinderMesher.AppendCap(ref MeshData, CaveSettings, position, runSize, CaveSettings.InnerRadius, CaveSettings.OuterRadius, CaveSettings.FloorThickness, true);
                    }

                    int nextSegmentAddress = (position + runSize) % CaveSettings.RadialSegments;
                    int nextSegmentDepth =
                        RadialSegmentDepth[FloorIndex * CaveSettings.RadialSegments + nextSegmentAddress];
                    float nextSegmentRadius = nextSegmentDepth == 0 ? CaveSettings.InnerRadius : CaveSettings.OuterRadius;

                    CylinderMesher.LinkWalls(ref MeshData, CaveSettings, position + runSize, wallRadius, nextSegmentRadius, CaveSettings.FloorThickness, CaveSettings.RoomHeight);

                    position += runSize;
                }
            }
        }
    }
}
