using System.Collections.Generic;
using UnityEngine;
using AK.Wwise;

namespace AudioStudio.Components
{
    [AddComponentMenu("AudioStudio/Surface Reflector")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter))]
    public class SurfaceReflector : AsComponent
    {
        public AcousticTexture AcousticTexture;
        public bool EnableDiffraction;
        public bool EnableDiffractionOnBoundaryEdges;

        private MeshFilter _meshFilter;

        protected override void HandleEnableEvent()
        {
            var mesh = _meshFilter.sharedMesh;
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;

            // Remove duplicate vertices
            var verticesRemap = new int[vertices.Length];
            var uniqueVertices = new List<Vector3>();
            var verticesDict = new Dictionary<Vector3, int>();

            for (var i = 0; i < vertices.Length; ++i)
            {
                int vertexIndex;
                if (!verticesDict.TryGetValue(vertices[i], out vertexIndex))
                {
                    vertexIndex = uniqueVertices.Count;
                    uniqueVertices.Add(vertices[i]);
                    verticesDict.Add(vertices[i], vertexIndex);
                }
                verticesRemap[i] = vertexIndex;
            }

            var vertexCount = uniqueVertices.Count;
            var surfaceArray = new AkAcousticSurfaceArray(1);
            var surface = surfaceArray[0];
            surface.textureID = AcousticTexture.Id;
            //surface.reflectorChannelMask = unchecked((uint) -1);
            surface.strName = _meshFilter.gameObject.name;

            //var vertexArray = new AkVertexArray(vertexCount);
            //for (var v = 0; v < vertexCount; ++v)
            //{
            //    var point = _meshFilter.transform.TransformPoint(uniqueVertices[v]);
            //    using (var akVert = vertexArray[v])
            //    {
            //        akVert.X = point.x;
            //        akVert.Y = point.y;
            //        akVert.Z = point.z;
            //    }
            //}

            //var numTriangles = triangles.Length / 3;
            //var triangleArray = new AkTriangleArray(numTriangles);
            //for (var i = 0; i < numTriangles; ++i)
            //{
            //    triangleArray[i].point0 = (ushort) verticesRemap[triangles[3 * i + 0]];
            //    triangleArray[i].point1 = (ushort) verticesRemap[triangles[3 * i + 1]];
            //    triangleArray[i].point2 = (ushort) verticesRemap[triangles[3 * i + 2]];
            //    triangleArray[i].surface = 0;
            //}
            //AudioStudioWrapper.SetGeometry((ulong) _meshFilter.GetInstanceID(), triangleArray, (uint) numTriangles, vertexArray, (uint) vertexCount, surfaceArray,
            //    1, EnableDiffraction, EnableDiffractionOnBoundaryEdges);
        }

        protected override void HandleDisableEvent()
        {
            AudioStudioWrapper.RemoveGeometry((ulong) _meshFilter.GetInstanceID());
        }

        public override bool IsValid()
        {
            _meshFilter = GetComponent<MeshFilter>();
            return _meshFilter != null;
        }
    }
}