using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Packrats
{
    /// <summary>
    /// Class for the overall cave system (the big pit of garbage the rats live in)
    /// </summary>

    public class CaveSystem : MonoBehaviour
    {
        public const int NULL_BUILDING_ID = 0;

        public int CurrentFloors => _caveFloors.Count;

        private CaveSystemSettings _caveSettings;
        private CaveSystemData _caveSystemData;

        private readonly List<CaveFloor> _caveFloors = new List<CaveFloor>();

        private readonly Dictionary<int, CurvedBuilding> _buildingIDToBuilding = new Dictionary<int, CurvedBuilding>();


        public CaveRaycaster CaveRaycaster { get; private set; }

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

            CaveRaycaster = gameObject.AddComponent<CaveRaycaster>();
            CaveRaycaster.Init(this, _caveSettings);

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

        public bool TryConstructBuilding(CurvedBuilding buildingInstance, int floorIndex, int segment)
        {
            if (!CanConstructBuildingAt(buildingInstance, floorIndex, segment))
            {
                return false;
            }

            _caveSettings.GetBuildingPosition(-floorIndex, segment, out var buildingPosition, out var buildingRotation);
            
            //Parent to floor for ease of editor use
            var floor = _caveFloors[floorIndex];
            buildingInstance.transform.parent = floor.transform;

            //Allocate building ID, and set that in the dictionary
            var buildingID = _caveSystemData.GetNextBuildingID();
            _buildingIDToBuilding[buildingID] = buildingInstance;

            //Init building
            buildingInstance.InitBuilding(buildingID, floorIndex, segment);

            //Add building to occupancy array
            AddBuildingToOccupancy(buildingInstance, floorIndex, segment);

            //Add building to pathfinding grid //TODO:
                //And interactive parts of the building to whatever manages that stuff
                   
            return true;
        }

        private void AddBuildingToOccupancy(CurvedBuilding building, int floorIndex, int segment)
        {
            _caveSystemData.LastJob.Complete();

            for (int i = 0; i < building.BuildingSizeInSegments; i++)
            {
                int wrappedSegment = (i + segment) % _caveSettings.RadialSegments;
                int index = _caveSettings.GetSegmentIndex(floorIndex, wrappedSegment);
                _caveSystemData.BuildingBySegment[index] = building.BuildingID;
            }
        }

        public bool CanConstructBuildingAt(CurvedBuilding buildingPrefab, int floorIndex, int segment)
        {
            if (floorIndex < 0 || floorIndex >= _caveFloors.Count)
            {
                return false;
            }

            _caveSystemData.LastJob.Complete();

            for (int i = 0; i < buildingPrefab.BuildingSizeInSegments; i++)
            {
                int wrappedSegment = (i + segment) % _caveSettings.RadialSegments;
                int index = _caveSettings.GetSegmentIndex(floorIndex, wrappedSegment);

                //Check if rock occupies the desired position
                if (_caveSystemData.RadialSegmentDepth[index] < buildingPrefab.BuildingDepthInSegments)
                {
                    return false;
                }

                //Check if a building occupies the desired position
                if (_caveSystemData.BuildingBySegment[index] != NULL_BUILDING_ID)
                {
                    return false;
                }
            }
            return true;
        }

        void OnDestroy()
        {
            _caveSystemData.Dispose();
        }
    }

    //Data representation of cave system.
    public class CaveSystemData
    {
        /// <summary>
        /// Indexed like a 2D array, with all the segments for a single floor in a line, and then 
        /// </summary>
        public NativeList<int> RadialSegmentDepth;

        public NativeList<int> BuildingBySegment;

        public JobHandle LastJob;

        private int _nextBuildingID = CaveSystem.NULL_BUILDING_ID + 1;

        public CaveSystemData(CaveSystemSettings caveSystemSettings)
        {
            RadialSegmentDepth = new NativeList<int>(Allocator.Persistent);
            BuildingBySegment = new NativeList<int>(Allocator.Persistent);

            LastJob = default;
        }

        public int GetNextBuildingID()
        {
            return _nextBuildingID++;
        }

        public void Dispose()
        {
            RadialSegmentDepth.Dispose(LastJob);
            BuildingBySegment.Dispose(LastJob);
        }
    }
}