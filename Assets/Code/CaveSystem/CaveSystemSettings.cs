using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace Packrats
{
    [CreateAssetMenu(fileName = DEFAULT_RESOURCE_NAME, menuName = "Packrats/CaveSystemSettings", order = 1)]
    public class CaveSystemSettings : ScriptableObject
    {
        public const string DEFAULT_RESOURCE_NAME = "DefaultCaveSetttings";
        private static CaveSystemSettings _defaultSettings;

        public static CaveSystemSettings DefaultSettings
        {
            get
            {
                if (_defaultSettings == null)
                {
                    _defaultSettings = Resources.Load<CaveSystemSettings>(DEFAULT_RESOURCE_NAME);
                }
                return _defaultSettings;
            }
        }

        public float FloorHeight;
        public float FloorThickness;
        public float RoomHeight => FloorHeight - FloorThickness;

        public float InnerRadius;
        public float OuterRadius;

        public int RadialSegments;
        public float SegmentSizeRadians => (math.PI * 2.0f) / RadialSegments;

        public int DepthSegments;

        public void GetBuildingPosition(int floor, int segment, out float3 positionOut, out Quaternion rotationOut)
        {
            float radial = segment * SegmentSizeRadians;
            float floorY = (floor * FloorHeight) + FloorThickness;

            positionOut = new float3(
                math.cos(radial) * InnerRadius,
                floorY,
                math.sin(radial) * InnerRadius);

            rotationOut = Quaternion.Euler(0.0f, Mathf.Rad2Deg * -radial, 0.0f);
        }

        public CaveSystemSettingsStruct AsStruct()
        {
            return new CaveSystemSettingsStruct()
            {
                FloorHeight = FloorHeight,
                FloorThickness = FloorThickness,
                InnerRadius = InnerRadius,
                OuterRadius = OuterRadius,
                RadialSegments = RadialSegments,
                DepthSegments = DepthSegments
            };
        }

        public static implicit operator CaveSystemSettingsStruct(CaveSystemSettings self)
        {
            return self.AsStruct();
        }
    }

    public struct CaveSystemSettingsStruct //Because Unity is a hateful thing (only structs in jobs)
    {
        public float FloorHeight;
        public float FloorThickness;
        public float RoomHeight => FloorHeight - FloorThickness;

        public float InnerRadius;
        public float OuterRadius;

        public int RadialSegments;
        public float SegmentSizeRadians => (math.PI * 2.0f) / RadialSegments;

        public int DepthSegments;
    }

    public static class CaveSystemSettingsExtensions
    {
        public static int GetSegmentIndex(this CaveSystemSettings caveSettings, int floorIndex, int segment)
        {
            return segment + floorIndex * caveSettings.RadialSegments;
        }

        public static int GetSegmentIndex(this CaveSystemSettingsStruct caveSettings, int floorIndex, int segment)
        {
            return segment + floorIndex * caveSettings.RadialSegments;
        }
    }
}