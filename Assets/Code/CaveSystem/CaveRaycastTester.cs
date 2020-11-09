using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Packrats
{
    public class CaveRaycastTester : MonoBehaviour
    {
        public Camera Camera;
        public CaveSystem CaveSystem;

        private CaveRaycaster _caveRaycaster;

        public GameObject HitFace;
        public GameObject HitCenter;

        public GameObject BuildingPrefab;

        private CurvedBuilding _currentBuildingToPlace;
        private int _currentBuildingIndex = -1;

        public GameObject[] Buildings;

        void Start()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            _caveRaycaster = CaveSystem.CaveRaycaster;
        }

        void Update()
        {
            Ray ray = Camera.ScreenPointToRay(Input.mousePosition);

            if (_caveRaycaster.RaycastCaveSegment(ray, out var raycastCaveResults))
            {
                HitFace.transform.position = raycastCaveResults.SegmentFace;
                HitCenter.transform.position = raycastCaveResults.SegmentCenter;

                if (Input.GetMouseButtonDown(0) && _currentBuildingToPlace == null)
                {
                    if (raycastCaveResults.FloorIndex >= 0 &&
                        raycastCaveResults.FloorIndex < CaveSystem.CaveFloors.Count)
                    {
                        var floor = CaveSystem.CaveFloors[raycastCaveResults.FloorIndex];
                        floor.SetSegmentDepth(raycastCaveResults.Segment, 1);
                    }
                }

                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    TryEquipBuilding(0);
                }

                SetBuildingPosition(raycastCaveResults);
                UpdateBuildingHighlight(raycastCaveResults);

                if (Input.GetMouseButtonDown(0) && _currentBuildingToPlace != null)
                {
                    TryPlaceBuilding(raycastCaveResults);
                }
            }
        }


        private void UpdateBuildingHighlight(RaycastCaveResult result)
        {
            if (_currentBuildingToPlace == null)
            {
                return;
            }

            var canPlaceBuilding =
                CaveSystem.CanConstructBuildingAt(_currentBuildingToPlace, result.FloorIndex, result.Segment);

            _currentBuildingToPlace.SetColor(canPlaceBuilding ? Color.green : Color.red);
        }

        private void TryPlaceBuilding(RaycastCaveResult result)
        {
            if (CaveSystem.TryConstructBuilding(_currentBuildingToPlace, result.FloorIndex, result.Segment))
            {
                _currentBuildingToPlace.ResetColor();
                _currentBuildingToPlace = null;
                _currentBuildingIndex = -1;
            }
        }

        private void TryEquipBuilding(int index)
        {
            if (index < 0 || index >= Buildings.Length)
            {
                return;
            }

            if (index == _currentBuildingIndex)
            {
                //Remove
                _currentBuildingIndex = -1;
                Destroy(_currentBuildingToPlace.gameObject);
                return;
            }

            if (index != _currentBuildingIndex && _currentBuildingToPlace != null)
            {
                //Remove
                Destroy(_currentBuildingToPlace.gameObject);
            }

            var obj = Instantiate(Buildings[index]);
            _currentBuildingToPlace = obj.GetComponent<CurvedBuilding>();
            _currentBuildingIndex = index;
        }

        private void SetBuildingPosition(RaycastCaveResult result)
        {
            if (_currentBuildingToPlace == null)
            {
                return;
            }

            _currentBuildingToPlace.transform.position = result.SegmentBuildingOrigin;
            _currentBuildingToPlace.transform.rotation = result.SegmentBuildingRotation;
        }

    }
}
