using System;
using System.ComponentModel;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Packrats
{
    [Serializable]
    public struct CylinderMeshSegmentArgs
    {
        public float Radians;
        public float Radius;
        public float Height;
        public int Segments;

        public void ValidateArguments()
        {
            if (Radians <= 0.0f)
            {
                throw new InvalidEnumArgumentException(nameof(Radians));
            }

            if (Radians > 2.0f * math.PI)
            {
                throw new InvalidEnumArgumentException(nameof(Radians));
            }

            if (Radius <= 0.0f)
            {
                throw new InvalidEnumArgumentException(nameof(Radius));
            }

            if (Height <= 0.0f)
            {
                throw new InvalidEnumArgumentException(nameof(Height));
            }

            if (Segments <= 0.0)
            {
                throw new InvalidEnumArgumentException(nameof(Segments));
            }
        }
    }

    public static class CylinderMeshSegmentGenerator
    {
        private const int INDICES_PER_SEGMENT = 6;

        public static Mesh GenerateMesh(CylinderMeshSegmentArgs settings)
        {
            settings.ValidateArguments();

            var positionBuffer = new Vector3[GetVertexCount(settings.Segments)];
            var normalBuffer = new Vector3[GetVertexCount(settings.Segments)];
            var uvBuffer = new Vector2[GetVertexCount(settings.Segments)];

            var indexBuffer = new int[GetIndexCount(settings.Segments)];

            FillVertexBuffers(settings, positionBuffer, normalBuffer, uvBuffer);
            FillIndexBuffer(settings, indexBuffer);

            var resultMesh = new Mesh();

            resultMesh.vertices = positionBuffer;
            resultMesh.uv = uvBuffer;
            resultMesh.normals = normalBuffer;
            resultMesh.triangles = indexBuffer;

            //TODO:Manually do this for smoothing
            resultMesh.RecalculateNormals();
            resultMesh.RecalculateTangents();
            resultMesh.UploadMeshData(false);

            return resultMesh;
        }

        private static void FillVertexBuffers(CylinderMeshSegmentArgs settings, Vector3[] positions, Vector3[] normals, Vector2[] uvs)
        {
            //In increasing 'radians' position, sweep around creating collumns, with the top vertex being first.
            float halfHeight = settings.Height * 0.5f;
            int columns = settings.Segments + 1;
            float radianStepSize = settings.Radians / (columns - 1);

            for (int i = 0; i < columns; i++)
            {
                float radianPosition = radianStepSize * i;

                //Unit cords position
                float3 centerPoint = new float3(
                    math.cos(radianPosition),
                    0.0f,
                    math.sin(radianPosition));

                //Compute normal as opposit of position from center
                float3 normal = math.normalize(-centerPoint);

                //Displace outwards by radius
                centerPoint *= settings.Radius;

                //Write out position
                centerPoint.y = halfHeight;
                positions[i * 2 + 0] = centerPoint;

                centerPoint.y = -halfHeight;
                positions[i * 2 + 1] = centerPoint;

                //Writ out normals
                normals[i * 2 + 0] = normal;
                normals[i * 2 + 1] = normal;

                //Write out UV
                float uvX = ((float)i / (float)settings.Segments);
                uvs[i * 2 + 0] = new Vector2(uvX, 0.0f);
                uvs[i * 2 + 1] = new Vector2(uvX, 1.0f);
            }
        }

        private static void FillIndexBuffer(CylinderMeshSegmentArgs settings, int[] indicies)
        {
            for (int i = 0; i < settings.Segments; i++)
            {
                var writePosition = i * INDICES_PER_SEGMENT;
                var readOffset = i * 2;

                WriteIndex(ref writePosition, readOffset, indicies, 0);
                WriteIndex(ref writePosition, readOffset, indicies, 1);
                WriteIndex(ref writePosition, readOffset, indicies, 2);

                WriteIndex(ref writePosition, readOffset, indicies, 2);
                WriteIndex(ref writePosition, readOffset, indicies, 1);
                WriteIndex(ref writePosition, readOffset, indicies, 3);
            }
        }

        private static void WriteIndex(ref int writePosition, int readOffset, int[] indicies, int vertex)
        {
            int index = readOffset + vertex;
            indicies[writePosition++] = index;
        }


        private static int GetVertexCount(int segments)
        {
            return 2 + segments * 2;//1 is 4, 2 is 6 3 is 8.
        }

        private static int GetIndexCount(int segments)
        {
            return segments * INDICES_PER_SEGMENT;//2 Triangles per segment
        }
    }



    public static class CylinderMesher
    {
        private const int INDICES_PER_WALL_SEGMENT = 6;



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
