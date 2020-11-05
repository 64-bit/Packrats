using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Unity.Jobs;

namespace Packrats
{
    /// <summary>
    /// Class for the overall cave system (the big pit of garbage the rats live in)
    /// </summary>

    public class CaveSystem : MonoBehaviour
    {

        public int CurrentFloors => _caveFloors.Count;

        private CaveSystemSettings _caveSettings;
        private CaveSystemData _caveSystemData;

        private readonly List<CaveFloor> _caveFloors = new List<CaveFloor>();

        public IReadOnlyList<CaveFloor> CaveFloors => _caveFloors;

        private bool _isCreated = false;

        public Material CaveMaterial;

        public void CreateCaveSystem(CaveSystemSettings systemSettings)
        {
            if (_isCreated)
            {
                throw new InvalidOperationException("Cave system is already created");
            }

            _caveSettings = systemSettings;
            _caveSystemData = new CaveSystemData(_caveSettings);
            _isCreated = true;
        }

        /// <summary>
        /// Create a new floor one down
        /// </summary>
        public CaveFloor DigNewFloor()
        {
            var floorIndex = _caveFloors.Count;
            var floorGameObject = new GameObject($"Floor {floorIndex + 1}");

            //Parent to system and position at correct height
            floorGameObject.transform.parent = transform;
            floorGameObject.transform.localPosition = new Vector3(0.0f, -_caveSettings.FloorHeight * floorIndex);

            var floor = floorGameObject.AddComponent<CaveFloor>();
            floor.InitFloor(this, _caveSettings, _caveSystemData, floorIndex);

            _caveFloors.Add(floor);

            return floor;
        }

        void OnDestroy()
        {
            _caveSystemData.Dispose();
        }
    }

    [Serializable]
    public struct CaveSystemSettings
    {

        public float FloorHeight;
        public float FloorThickness;
        public float RoomHeight => FloorHeight - FloorThickness;

        public float InnerRadius;
        public float OuterRadius;

        public int RadialSegments;
        public int DepthSegments;
    }

    //Data representation of cave system.
    public class CaveSystemData
    {
        /// <summary>
        /// Indexed like a 2D array, with all the segments for a single floor in a line, and then 
        /// </summary>
        public NativeList<int> RadialSegmentDepth;

        public JobHandle LastJob;

        public CaveSystemData(CaveSystemSettings caveSystemSettings)
        {
            RadialSegmentDepth = new NativeList<int>(Allocator.Persistent);
            LastJob = default;
        }

        public void Dispose()
        {
            RadialSegmentDepth.Dispose(LastJob);
        }
    }
}