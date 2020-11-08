using System;

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
    }
}