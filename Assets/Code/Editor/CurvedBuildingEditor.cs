using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Packrats
{
    [CustomEditor(typeof(CurvedBuilding))]
    public class CurvedBuildingEditor : Editor
    {
        void OnEnable()
        {

        }

        public override void OnInspectorGUI()
        {
            var targetCard = target as CurvedBuilding;
            DrawDefaultInspector();
            targetCard.OnEdited();
        }
    }
}