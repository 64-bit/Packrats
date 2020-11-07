using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Packrats
{
    public static class CylinderMesher
    {
        public static void AppendWalls(ref NativeMeshData meshData, CaveSystemSettings systemSettings, int startSegment, int segmentCount, float radius, float yPosition, float height)
        {
            int indexOffset = meshData.Positions.Length;

            //In increasing 'radians' position, sweep around creating collumns, with the top vertex being first.
            int columns = segmentCount + 1;
            float radianStepSize = ((float)segmentCount / systemSettings.RadialSegments) / (columns - 1);

            radianStepSize *= math.PI * 2.0f;

            float initialAngle = startSegment * radianStepSize;

            for (int i = 0; i < columns; i++)
            {
                float radianPosition = initialAngle + radianStepSize * i;

                //Unit cords position
                float3 centerPoint = new float3(
                    math.cos(radianPosition),
                    0.0f,
                    math.sin(radianPosition));

                //Compute normal as opposit of position from center
                float3 normal = math.normalize(-centerPoint);

                //Displace outwards by radius
                centerPoint *= radius;

                //Write out position
                centerPoint.y = yPosition + height;
                meshData.Positions.Add(centerPoint);

                centerPoint.y = yPosition;
                meshData.Positions.Add(centerPoint);

                //Writ out normals
                meshData.Normals.Add(normal);
                meshData.Normals.Add(normal);

                //Write out UV
                float uvX = ((float)i / (float)systemSettings.RadialSegments);
                meshData.UVs.Add(new float2(uvX, 0.0f));
                meshData.UVs.Add(new float2(uvX, 1.0f));
            }

            void WriteIndex(ref NativeList<int> indicies, int readOffset, int vertex)
            {
                var index = readOffset + vertex;
                indicies.Add(index);
            }

            for (var i = 0; i < segmentCount; i++)
            {
                var readOffset = indexOffset + i * 2;

                WriteIndex(ref meshData.Indicies, readOffset, 0);
                WriteIndex(ref meshData.Indicies, readOffset, 1);
                WriteIndex(ref meshData.Indicies, readOffset, 2);

                WriteIndex(ref meshData.Indicies, readOffset, 2);
                WriteIndex(ref meshData.Indicies, readOffset, 1);
                WriteIndex(ref meshData.Indicies, readOffset, 3);
            }
        }

        public static void LinkWalls(ref NativeMeshData meshData, CaveSystemSettings systemSettings, int segmentIndex,
            float startRadius, float endRadius, float yPosition, float height)
        {
            int indexOffset = meshData.Positions.Length;

            //In increasing 'radians' position, sweep around creating collumns, with the top vertex being first.
            float radianStepSize = (1.0f / systemSettings.RadialSegments);

            radianStepSize *= math.PI * 2.0f;

            float radianPosition = radianStepSize * segmentIndex;

            //Unit cords position
            float3 startPoint = new float3(
                math.cos(radianPosition),
                0.0f,
                math.sin(radianPosition));
            float3 endPoint = startPoint;


            //Compute normal as opposit of position from center
            float3 normal = math.cross(startPoint, new float3(0.0f, 1.0f, 0.0f));

            //Displace outwards by radius
            startPoint *= startRadius;
            endPoint *= endRadius;

            //Write out position
            startPoint.y = yPosition;
            endPoint.y = yPosition;
            meshData.Positions.Add(startPoint);
            meshData.Positions.Add(endPoint);

            startPoint.y += height;
            endPoint.y += height;
            meshData.Positions.Add(startPoint);
            meshData.Positions.Add(endPoint);

            //Writ out normals
            meshData.Normals.Add(normal);
            meshData.Normals.Add(normal);
            meshData.Normals.Add(normal);
            meshData.Normals.Add(normal);

            //Write out UV
            meshData.UVs.Add(new float2(0.0f, 0.0f));
            meshData.UVs.Add(new float2(0.0f, 1.0f));
            meshData.UVs.Add(new float2(1.0f, 0.0f));
            meshData.UVs.Add(new float2(1.0f, 1.0f));

            void WriteIndex(ref NativeList<int> indicies, int readOffset, int vertex)
            {
                var index = readOffset + vertex;
                indicies.Add(index);
            }

            WriteIndex(ref meshData.Indicies, indexOffset, 0);
            WriteIndex(ref meshData.Indicies, indexOffset, 1);
            WriteIndex(ref meshData.Indicies, indexOffset, 2);

            WriteIndex(ref meshData.Indicies, indexOffset, 2);
            WriteIndex(ref meshData.Indicies, indexOffset, 1);
            WriteIndex(ref meshData.Indicies, indexOffset, 3);
        }


        public static void AppendCap(ref NativeMeshData meshData, CaveSystemSettings systemSettings, int startSegment, int segmentCount, float innerRadius, float outerRadius, float yPosition, bool facesUp)
        {
            int indexOffset = meshData.Positions.Length;

            //In increasing 'radians' position, sweep around creating collumns, with the top vertex being first.
            int columns = segmentCount + 1;
            float radianStepSize = ((float)segmentCount / systemSettings.RadialSegments) / (columns - 1);

            radianStepSize *= math.PI * 2.0f;

            float initialAngle = startSegment * radianStepSize;

            for (int i = 0; i < columns; i++)
            {
                float radianPosition = initialAngle + radianStepSize * i;

                //Unit cords position
                float3 innerPoint = new float3(
                    math.cos(radianPosition),
                    0.0f,
                    math.sin(radianPosition));
                float3 outerPoint = innerPoint;


                //Compute normal as opposit of position from center
                float3 normal = new float3(0.0f, facesUp ? 1.0f : -1.0f, 0.0f);

                //Displace outwards by radius
                innerPoint *= innerRadius;
                outerPoint *= outerRadius;

                innerPoint.y = yPosition;
                outerPoint.y = yPosition;

                //Write out position
                meshData.Positions.Add(innerPoint);
                meshData.Positions.Add(outerPoint);

                //Writ out normals
                meshData.Normals.Add(normal);
                meshData.Normals.Add(normal);

                //Write out UV
                float uvX = ((float)i / (float)systemSettings.RadialSegments);
                meshData.UVs.Add(new float2(uvX, 0.0f));
                meshData.UVs.Add(new float2(uvX, 1.0f));
            }

            void WriteIndex(ref NativeList<int> indicies, int readOffset, int vertex)
            {
                var index = readOffset + vertex;
                indicies.Add(index);
            }

            if (facesUp)//Flip winding order for backface culling
            {
                for (var i = 0; i < segmentCount; i++)
                {
                    var readOffset = indexOffset + i * 2;

                    WriteIndex(ref meshData.Indicies, readOffset, 0);
                    WriteIndex(ref meshData.Indicies, readOffset, 2);
                    WriteIndex(ref meshData.Indicies, readOffset, 1);

                    WriteIndex(ref meshData.Indicies, readOffset, 2);
                    WriteIndex(ref meshData.Indicies, readOffset, 3);
                    WriteIndex(ref meshData.Indicies, readOffset, 1);
                }
            }
            else
            {
                for (var i = 0; i < segmentCount; i++)
                {
                    var readOffset = indexOffset + i * 2;

                    WriteIndex(ref meshData.Indicies, readOffset, 0);
                    WriteIndex(ref meshData.Indicies, readOffset, 1);
                    WriteIndex(ref meshData.Indicies, readOffset, 2);

                    WriteIndex(ref meshData.Indicies, readOffset, 2);
                    WriteIndex(ref meshData.Indicies, readOffset, 1);
                    WriteIndex(ref meshData.Indicies, readOffset, 3);
                }
            }
        }

    }
}
