using System.Collections;
using System.Collections.Generic;
using Packrats;
using System;
using UnityEngine;

namespace Packrats
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class CardWrapTest : MonoBehaviour
    {
        public CylinderMeshSegmentArgs CylenderSettings;
        

        // Start is called before the first frame update
        void Start()
        {
            var meshFilter = GetComponent<MeshFilter>();
            var mesh = CylinderMeshSegmentGenerator.GenerateMesh(CylenderSettings);

            meshFilter.mesh = mesh;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
