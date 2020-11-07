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

                if (Input.GetMouseButtonDown(0))
                {
                    if (raycastCaveResults.FloorIndex >= 0 &&
                        raycastCaveResults.FloorIndex < CaveSystem.CaveFloors.Count)
                    {
                        var floor = CaveSystem.CaveFloors[raycastCaveResults.FloorIndex];
                        floor.SetSegmentDepth(raycastCaveResults.Segment, 1);
                    }
                }
            }
        }
    }
}
