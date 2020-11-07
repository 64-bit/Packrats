using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Packrats
{
    public class CaveRaycaster : MonoBehaviour
    {

        private CaveSystemSettings _caveSettings;
        private MeshCollider _collider;

        public void Init(CaveSystem parentSystem, CaveSystemSettings caveSettings)
        {
            _caveSettings = caveSettings;

            _collider = gameObject.AddComponent<MeshCollider>();
            _collider.convex = false;
            _collider.sharedMesh = GenerateCollisionMesh();
            //_collider.isTrigger = true;
            gameObject.layer = 16;
        }

        private Mesh GenerateCollisionMesh()
        {
            var nativeMeshData = new NativeMeshData(Allocator.Persistent);

            CylinderMesher.AppendWalls(ref nativeMeshData, _caveSettings, 0, _caveSettings.RadialSegments, _caveSettings.InnerRadius, -1000.0f, 2000.0f);//TODO:

            var result = new Mesh();
            nativeMeshData.ApplyToMesh(result);
            nativeMeshData.Dispose();

            return result;
        }

        public bool RaycastCaveSegment(Ray ray, out RaycastCaveResult raycastCaveResult)
        {
            if (Physics.Raycast(ray, out var raycastHit))
            {
                raycastCaveResult = ComputeCaveHitResult(raycastHit);
                return true;
            }

            raycastCaveResult = default;
            return false;
        }

        private RaycastCaveResult ComputeCaveHitResult(RaycastHit hit)
        {
            var floor = CalculateFloor(hit);
            CalculateSegment(hit, floor, out var segment, out var segmentCenter, out var segmentFace);

            return new RaycastCaveResult()
            {
                RaycastHit = hit,
                Floor = floor,
                FloorIndex = -floor,//TODO:Baesd on cave system positive floors ?
                Segment = segment,
                SegmentCenter = segmentCenter,
                SegmentFace = segmentFace 
            };
        }

        private int CalculateFloor(RaycastHit hit)
        {
            float height = hit.point.y;

            float raw_floor = height / _caveSettings.FloorHeight;
            raw_floor = math.floor(raw_floor);

            return (int) raw_floor;
        }

        private void CalculateSegment(RaycastHit hit, int floor, out int segmentOut, out float3 segmentCenterOut, out float3 segmentFaceOut)
        {
            float radians = math.atan2(hit.point.z, hit.point.x);
            if (radians < 0.0f)
            {
                radians = math.PI + (math.PI + radians);//atan2 returns a value from -pi to pi
            }

            float radianPercentage = radians / (math.PI * 2.0f);

            int segment = (int)math.floor(radianPercentage * _caveSettings.RadialSegments);

            segmentOut = segment;

            float segmentPercentage = ((segment + 0.5f) / (float)_caveSettings.RadialSegments);
            float segmentCenterRadians = segmentPercentage * math.PI * 2.0f;

            float3 toSegmentCenter = new float3(math.cos(segmentCenterRadians), 0.0f, math.sin(segmentCenterRadians));
            float segmentcenterHeight = floor * _caveSettings.FloorHeight + 0.5f * _caveSettings.FloorHeight;

            segmentCenterOut = toSegmentCenter * (0.5f * (_caveSettings.InnerRadius + _caveSettings.OuterRadius));
            segmentCenterOut.y = segmentcenterHeight;

            segmentFaceOut = toSegmentCenter * _caveSettings.InnerRadius;
            segmentFaceOut.y = segmentcenterHeight;
        }
    }

    public struct RaycastCaveResult
    {
        public RaycastHit RaycastHit;

        public int Floor;
        public int FloorIndex;
        public int Segment;
        public float3 SegmentFace;
        public float3 SegmentCenter;


    }
}