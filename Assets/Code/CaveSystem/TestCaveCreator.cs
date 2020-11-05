using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Packrats
{
    /// <summary>
    /// Test class to init a cave into a particular state
    /// </summary>
    public class TestCaveCreator : MonoBehaviour
    {

        public CaveSystemSettings CaveSettings;

        public int Floors;

        private void Awake()
        {
            var caveSystem = GetComponent<CaveSystem>();
            caveSystem.CreateCaveSystem(CaveSettings);

            for (int i = 0; i < Floors; i++)
            {
                var floor = caveSystem.DigNewFloor();
            }

        }
    }
}
