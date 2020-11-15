using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Packrats
{
    public class NavGraph
    {
        public const int MaxEdgesPerNode = 8;

        private NavGraphData _navGraphData;
        private JobHandle _lastJobForNavgraph;

        public int NodeCount { get; private set; }

        //Need some sort of way to find the nearest node.

        public struct NavGraphData
        {
            public NativeArray<float3> NodePositions;
            public NativeArray<int> NodeEdges;
            public NativeArray<byte> NodeEdgeCosts;

            public int NodeCount => NodePositions.Length;

            public NavGraphData(int size)
            {
                NodePositions = new NativeArray<float3>(size, Allocator.Persistent);
                NodeEdges = new NativeArray<int>(size * MaxEdgesPerNode, Allocator.Persistent);
                NodeEdgeCosts = new NativeArray<byte>(size * MaxEdgesPerNode, Allocator.Persistent);
            }

            public float3 GetNodePosition(int nodeIndex)
            {
                return NodePositions[nodeIndex];
            }

            public int GetNodeEdge(int nodeIndex, int edgeIndex)
            {
                return NodeEdges[nodeIndex * MaxEdgesPerNode + edgeIndex];
            }

            public byte GetNodeEdgeCost(int nodeIndex, int edgeIndex)
            {
                return NodeEdgeCosts[nodeIndex * MaxEdgesPerNode + edgeIndex];
            }

            public void Dispose(JobHandle jobToWaitOn = default)
            {
                NodePositions.Dispose(jobToWaitOn);
                NodeEdges.Dispose(jobToWaitOn);
                NodeEdgeCosts.Dispose(jobToWaitOn);
            }
        }

        public void ExpandNavGraph(int newSize)
        {
            if (newSize <= NodeCount)
            {
                throw new ArgumentOutOfRangeException(nameof(newSize), "The new size must be greater than the old one");
            }

            var newData = new NavGraphData(newSize);

            var positionJob = _navGraphData.NodePositions.CopyTo(newData.NodePositions, _lastJobForNavgraph);
            var edgeJob = _navGraphData.NodeEdges.CopyTo(newData.NodeEdges, _lastJobForNavgraph);
            var edgeCostJob = _navGraphData.NodeEdgeCosts.CopyTo(newData.NodeEdgeCosts, _lastJobForNavgraph);

            int oldSize = _navGraphData.NodeCount;
            int newNodes = newSize - oldSize;


            _lastJobForNavgraph = JobHandle.CombineDependencies(positionJob, edgeJob, edgeCostJob);

            _navGraphData.Dispose(_lastJobForNavgraph);
            _navGraphData = newData;
        }

    }

    public struct NavGraphNode
    {
        public float3 CenterPosition;

        public int[] Edges;
        public float[] EdgeCosts;//Could probably be some sort of fixed point byte thing

        //Edges ?
        //Edge Costs ?

        //Edge indicies ?
        //Implicit, via EDGE MASK ?

        //What else could be linked besides up,down, lrfb
        //A hole in the back wall is STILL BACK

        //How is bullshit like elevators factored into this

        //Nodes need to be dynamically modifiable
        //Nodes should be memory access friendly
    }

    public struct PathfindingHeapItem
    {
        public float Distance;
        public int Node;
    }

}
