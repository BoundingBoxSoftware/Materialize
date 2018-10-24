
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

namespace UnityExtension
{
    public static class MeshExt
    {
        //------------------------------------------------------------------------------------------------------------
        public static void RecalculateTangents(this Mesh lMesh)
        {
            //speed up math by copying the mesh arrays
            int[] triangles = lMesh.triangles;
            Vector3[] vertices = lMesh.vertices;
            Vector2[] uv = lMesh.uv;
            Vector3[] normals = lMesh.normals;

            //variable definitions
            int triangleCount = triangles.Length;
            int vertexCount = vertices.Length;

            Vector3[] tan1 = new Vector3[vertexCount];
            Vector3[] tan2 = new Vector3[vertexCount];

            Vector4[] tangents = new Vector4[vertexCount];

            for (long a = 0; a < triangleCount; a += 3)
            {
                long i1 = triangles[a + 0];
                long i2 = triangles[a + 1];
                long i3 = triangles[a + 2];

                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];
                Vector3 v3 = vertices[i3];

                Vector2 w1 = uv[i1];
                Vector2 w2 = uv[i2];
                Vector2 w3 = uv[i3];

                float x1 = v2.x - v1.x;
                float x2 = v3.x - v1.x;
                float y1 = v2.y - v1.y;
                float y2 = v3.y - v1.y;
                float z1 = v2.z - v1.z;
                float z2 = v3.z - v1.z;

                float s1 = w2.x - w1.x;
                float s2 = w3.x - w1.x;
                float t1 = w2.y - w1.y;
                float t2 = w3.y - w1.y;

                float r = 1.0f / (s1 * t2 - s2 * t1);

                Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;

                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;
            }


            for (long a = 0; a < vertexCount; ++a)
            {
                Vector3 n = normals[a];
                Vector3 t = tan1[a];

                //Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
                //tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);
                Vector3.OrthoNormalize(ref n, ref t);
                tangents[a].x = t.x;
                tangents[a].y = t.y;
                tangents[a].z = t.z;

                tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
            }

            lMesh.tangents = tangents;
        }

        //------------------------------------------------------------------------------------------------------------
        public static void LoadOBJ(this Mesh lMesh, OBJData lData)
        {
            List<Vector3> lVertices = new List<Vector3>();
            List<Vector3> lNormals = new List<Vector3>();
            List<Vector2> lUVs = new List<Vector2>();
            List<int>[] lIndices = new List<int>[lData.m_Groups.Count];
            Dictionary<OBJFaceVertex, int> lVertexIndexRemap = new Dictionary<OBJFaceVertex, int>();
            bool lHasNormals = lData.m_Normals.Count > 0;
            bool lHasUVs = lData.m_UVs.Count > 0;

            lMesh.subMeshCount = lData.m_Groups.Count;
            for (int lGCount = 0; lGCount < lData.m_Groups.Count; ++lGCount)
            {
                OBJGroup lGroup = lData.m_Groups[lGCount];
                lIndices[lGCount] = new List<int>();

                for (int lFCount = 0; lFCount < lGroup.Faces.Count; ++lFCount)
                {
                    OBJFace lFace = lGroup.Faces[lFCount];

                    // Unity3d doesn't support non-triangle faces
                    // so we do simple fan triangulation
                    for (int lVCount = 1; lVCount < lFace.Count - 1; ++lVCount)
                    {
                        foreach (int i in new int[]{0, lVCount, lVCount + 1})
                        {
                            OBJFaceVertex lFaceVertex = lFace[i];
                            int lVertexIndex = -1;

                            if (!lVertexIndexRemap.TryGetValue(lFaceVertex, out lVertexIndex)) {
                                lVertexIndexRemap[lFaceVertex] = lVertices.Count;
                                lVertexIndex = lVertices.Count;

                                lVertices.Add(lData.m_Vertices[lFaceVertex.m_VertexIndex]);
                                if (lHasUVs)
                                {
                                    lUVs.Add(lData.m_UVs[lFaceVertex.m_UVIndex]);
                                }
                                if (lHasNormals)
                                {
                                    lNormals.Add(lData.m_Normals[lFaceVertex.m_NormalIndex]);
                                }
                            }

                            lIndices[lGCount].Add(lVertexIndex);
                        }
                    }
                }
            }

            lMesh.triangles = new int[]{ };
            lMesh.vertices = lVertices.ToArray();
            lMesh.uv = lUVs.ToArray();
            lMesh.normals = lNormals.ToArray();
            if (!lHasNormals)
            {
                lMesh.RecalculateNormals();
            }

            lMesh.RecalculateTangents();

            for (int lGCount = 0; lGCount < lData.m_Groups.Count; ++lGCount)
            {
                lMesh.SetTriangles(lIndices[lGCount].ToArray(), lGCount);
            }
        }

        //------------------------------------------------------------------------------------------------------------
        public static OBJData EncodeOBJ(this Mesh lMesh)
        {
            OBJData lData = new OBJData
            {
                m_Vertices = new List<Vector3>(lMesh.vertices),
                m_UVs = new List<Vector2>(lMesh.uv),
                m_Normals = new List<Vector3>(lMesh.normals),
                m_UV2s = new List<Vector2>(lMesh.uv2),
                m_Colors = new List<Color>(lMesh.colors)
            };

            int[] lIndices = null;
            OBJGroup lGroup = null;
            OBJFace lFace = null;
            OBJFaceVertex lFaceVertex = null;

            for (int lMCount = 0; lMCount < lMesh.subMeshCount; ++lMCount)
            {
                lIndices = lMesh.GetTriangles(lMCount);
                lGroup = new OBJGroup(lMesh.name + "_" + lMCount.ToString());

                for (int lCount = 0; lCount < lIndices.Length; lCount += 3)
                {
                    lFace = new OBJFace();

                    lFaceVertex = new OBJFaceVertex();
                    lFaceVertex.m_VertexIndex = lData.m_Vertices.Count > 0 ? lIndices[lCount] : -1;
                    lFaceVertex.m_UVIndex = lData.m_UVs.Count > 0 ? lIndices[lCount] : -1;
                    lFaceVertex.m_NormalIndex = lData.m_Normals.Count > 0 ? lIndices[lCount] : -1;
                    lFaceVertex.m_UV2Index = lData.m_UV2s.Count > 0 ? lIndices[lCount] : -1;
                    lFaceVertex.m_ColorIndex = lData.m_Colors.Count > 0 ? lIndices[lCount] : -1;
                    lFace.AddVertex(lFaceVertex);

                    lFaceVertex = new OBJFaceVertex();
                    lFaceVertex.m_VertexIndex = lData.m_Vertices.Count > 0 ? lIndices[lCount + 1] : -1;
                    lFaceVertex.m_UVIndex = lData.m_UVs.Count > 0 ? lIndices[lCount + 1] : -1;
                    lFaceVertex.m_NormalIndex = lData.m_Normals.Count > 0 ? lIndices[lCount + 1] : -1;
                    lFaceVertex.m_UV2Index = lData.m_UV2s.Count > 0 ? lIndices[lCount + 1] : -1;
                    lFaceVertex.m_ColorIndex = lData.m_Colors.Count > 0 ? lIndices[lCount + 1] : -1;
                    lFace.AddVertex(lFaceVertex);

                    lFaceVertex = new OBJFaceVertex();
                    lFaceVertex.m_VertexIndex = lData.m_Vertices.Count > 0 ? lIndices[lCount + 2] : -1;
                    lFaceVertex.m_UVIndex = lData.m_UVs.Count > 0 ? lIndices[lCount + 2] : -1;
                    lFaceVertex.m_NormalIndex = lData.m_Normals.Count > 0 ? lIndices[lCount + 2] : -1;
                    lFaceVertex.m_UV2Index = lData.m_UV2s.Count > 0 ? lIndices[lCount + 2] : -1;
                    lFaceVertex.m_ColorIndex = lData.m_Colors.Count > 0 ? lIndices[lCount + 2] : -1;
                    lFace.AddVertex(lFaceVertex);

                    lGroup.AddFace(lFace);
                }

                lData.m_Groups.Add(lGroup);
            }

            return lData;
        }

        //------------------------------------------------------------------------------------------------------------
        internal const int MESH_BINARY_HEADER_SIZE = 20;
        internal const short MESH_BINARY_SIGNATURE = 0xF5;
        internal const short MESH_BINARY_VERSION = 1;

        //------------------------------------------------------------------------------------------------------------
        public static bool LoadBinary(this Mesh lMesh, byte[] lData)
        {
            int lSizeOfVector2 = Marshal.SizeOf(typeof(Vector2));
            int lSizeOfVector3 = Marshal.SizeOf(typeof(Vector3));
            int lSizeOfVector4 = Marshal.SizeOf(typeof(Vector4));
            int lSizeOfMatrix4x4 = Marshal.SizeOf(typeof(Matrix4x4));
            int lSizeOfBoneWeight = Marshal.SizeOf(typeof(BoneWeight));
            int lSizeOfColor = Marshal.SizeOf(typeof(Color));

            int lDataOffset = MESH_BINARY_HEADER_SIZE;
            int lDeltaOffset = 0;

            if (lData == null ||
                lData.Length < MESH_BINARY_HEADER_SIZE)
            {
                return false;
            }

            //  Header
            short lSignature = BitConverter.ToInt16(lData, 0);
            short lVersion = BitConverter.ToInt16(lData, 2);

            if (lSignature != MESH_BINARY_SIGNATURE ||
                lVersion != MESH_BINARY_VERSION)
            {
                return false;
            }

            lMesh.Clear();

            int lVertexCount = BitConverter.ToInt32(lData, 4);
            int lIndexCount = BitConverter.ToInt32(lData, 8);
            int lSubMeshCount = BitConverter.ToInt32(lData, 12);

            byte lFlags = lData[16];

            bool lUVFlag = (lFlags & 1) > 0;
            bool lUV1Flag = (lFlags & 2) > 0;
            bool lUV2Flag = (lFlags & 4) > 0;
            bool lNormalFlag = (lFlags & 8) > 0;
            bool lTangentFlag = (lFlags & 16) > 0;
            bool lColorFlag = (lFlags & 32) > 0;
            bool lBindPoseFlag = (lFlags & 64) > 0;
            bool lBoneWeightFlag = (lFlags & 128) > 0;

            //  Vertices
            Vector3[] lVertices = new Vector3[lVertexCount];
            lDeltaOffset = lVertices.Length * lSizeOfVector3;
            GCHandle lHandle = GCHandle.Alloc(lVertices, GCHandleType.Pinned);
            Marshal.Copy(lData, lDataOffset, lHandle.AddrOfPinnedObject(), lDeltaOffset);
            lHandle.Free();
            lDataOffset += lDeltaOffset;
            lMesh.vertices = lVertices;
            lVertices = null;

            //  UV Channel 0
            Vector2[] lUVs = null;
            if (lUVFlag == true)
            {
                lUVs = new Vector2[lVertexCount];
                lDeltaOffset = lUVs.Length * lSizeOfVector2;
                lHandle = GCHandle.Alloc(lUVs, GCHandleType.Pinned);
                Marshal.Copy(lData, lDataOffset, lHandle.AddrOfPinnedObject(), lDeltaOffset);
                lHandle.Free();
                lDataOffset += lDeltaOffset;
                lMesh.uv = lUVs;
                Debug.Log("UV Count : " + lUVs.Length);
                lUVs = null;
            }

            //  UV Channel 1
            if (lUV1Flag == true)
            {
                lUVs = new Vector2[lVertexCount];
                lDeltaOffset = lUVs.Length * lSizeOfVector2;
                lHandle = GCHandle.Alloc(lUVs, GCHandleType.Pinned);
                Marshal.Copy(lData, lDataOffset, lHandle.AddrOfPinnedObject(), lDeltaOffset);
                lHandle.Free();
                lDataOffset += lDeltaOffset;
                lMesh.uv2 = lUVs;
                Debug.Log("UV1 Count : " + lUVs.Length);
                lUVs = null;
            }

            //  UV Channel 2
            if (lUV2Flag == true)
            {
                lUVs = new Vector2[lVertexCount];
                lDeltaOffset = lUVs.Length * lSizeOfVector2;
                lHandle = GCHandle.Alloc(lUVs, GCHandleType.Pinned);
                Marshal.Copy(lData, lDataOffset, lHandle.AddrOfPinnedObject(), lDeltaOffset);
                lHandle.Free();
                lDataOffset += lDeltaOffset;
                lMesh.uv2 = lUVs;
                Debug.Log("UV2 Count : " + lUVs.Length);
                lUVs = null;
            }

            //  Normals
            if (lNormalFlag == true)
            {
                Vector3[] lNormals = new Vector3[lVertexCount];
                lDeltaOffset = lNormals.Length * lSizeOfVector3;
                lHandle = GCHandle.Alloc(lNormals, GCHandleType.Pinned);
                Marshal.Copy(lData, lDataOffset, lHandle.AddrOfPinnedObject(), lDeltaOffset);
                lHandle.Free();
                lDataOffset += lDeltaOffset;
                lMesh.normals = lNormals;
                Debug.Log("Normal Count : " + lNormals.Length);
                lNormals = null;
            }

            //  Tangents
            if (lTangentFlag == true)
            {
                Vector4[] lTangents = new Vector4[lVertexCount];
                lDeltaOffset = lTangents.Length * lSizeOfVector4;
                lHandle = GCHandle.Alloc(lTangents, GCHandleType.Pinned);
                Marshal.Copy(lData, lDataOffset, lHandle.AddrOfPinnedObject(), lDeltaOffset);
                lHandle.Free();
                lDataOffset += lDeltaOffset;
                lMesh.tangents = lTangents;
                Debug.Log("Tangents Count : " + lTangents.Length);
                lTangents = null;
            }

            //  Colors
            if (lColorFlag == true)
            {
                Color[] lColors = new Color[lVertexCount];
                lDeltaOffset = lColors.Length * lSizeOfColor;
                lHandle = GCHandle.Alloc(lColors, GCHandleType.Pinned);
                Marshal.Copy(lData, lDataOffset, lHandle.AddrOfPinnedObject(), lDeltaOffset);
                lHandle.Free();
                lDataOffset += lDeltaOffset;
                lMesh.colors = lColors;
                lColors = null;
            }

            //  Bind Poses
            if (lBindPoseFlag == true)
            {
                Matrix4x4[] lBindPoses = new Matrix4x4[lVertexCount];
                lDeltaOffset = lBindPoses.Length * lSizeOfMatrix4x4;
                lHandle = GCHandle.Alloc(lBindPoses, GCHandleType.Pinned);
                Marshal.Copy(lData, lDataOffset, lHandle.AddrOfPinnedObject(), lDeltaOffset);
                lHandle.Free();
                lDataOffset += lDeltaOffset;
                lMesh.bindposes = lBindPoses;
                lBindPoses = null;
            }

            //  Bone Weights
            if (lBoneWeightFlag == true)
            {
                BoneWeight[] lBoneWeight = new BoneWeight[lVertexCount];
                lDeltaOffset = lBoneWeight.Length * lSizeOfBoneWeight;
                lHandle = GCHandle.Alloc(lBoneWeight, GCHandleType.Pinned);
                Marshal.Copy(lData, lDataOffset, lHandle.AddrOfPinnedObject(), lDeltaOffset);
                lHandle.Free();
                lDataOffset += lDeltaOffset;
                lMesh.boneWeights = lBoneWeight;
                lBoneWeight = null;
            }

            //  Indices
            int[] lIndices = new int[lIndexCount];
            lDeltaOffset = lIndices.Length * sizeof(int);
            Buffer.BlockCopy(lData, lDataOffset, lIndices, 0, lDeltaOffset);
            lDataOffset += lDeltaOffset;
            lMesh.triangles = lIndices;
            lIndices = null;

            //  SubMesh Indices
            for (int lSubMeshIndex = 0; lSubMeshIndex < lSubMeshCount; ++lSubMeshIndex)
            {
                int lSubMeshIndexCount = BitConverter.ToInt32(lData, lDataOffset);
                lDataOffset += sizeof(int);

                lIndices = new int[lSubMeshIndexCount];
                lDeltaOffset = lIndices.Length * sizeof(int);
                Buffer.BlockCopy(lData, lDataOffset, lIndices, 0, lDeltaOffset);
                lDataOffset += lDeltaOffset;

                if (lIndices.Length > 0 &&
                    lIndices.Length % 3 == 0)
                {
                    lMesh.SetTriangles(lIndices, lSubMeshIndex);
                }

                lIndices = null;
            }

            return true;
        }

        //------------------------------------------------------------------------------------------------------------
        public static byte[] EncodeBinary(this Mesh lMesh)
        {
            //  Currently no support for BlendShape

            int lSizeOfVector2 = Marshal.SizeOf(typeof(Vector2));
            int lSizeOfVector3 = Marshal.SizeOf(typeof(Vector3));
            int lSizeOfVector4 = Marshal.SizeOf(typeof(Vector4));
            int lSizeOfMatrix4x4 = Marshal.SizeOf(typeof(Matrix4x4));
            int lSizeOfBoneWeight = Marshal.SizeOf(typeof(BoneWeight));
            int lSizeOfColor = Marshal.SizeOf(typeof(Color));

            int lDataSize = MESH_BINARY_HEADER_SIZE;
            int lDeltaSize = 0;

            bool lUVFlag = false, lUV1Flag = false, lUV2Flag = false,
                lNormalFlag = false, lTangentFlag = false, lColorFlag = false,
                lBindPoseFlag = false, lBoneWeightFlag = false;

            byte[] lData = new byte[lDataSize];

            //  Vertices
            Vector3[] lVertices = lMesh.vertices;
            Int32Converter lVertexCount = lVertices.Length;
            lDeltaSize = lVertices.Length * lSizeOfVector3;
            Array.Resize(ref lData, lDataSize + lDeltaSize);
            GCHandle lHandle = GCHandle.Alloc(lVertices, GCHandleType.Pinned);
            Marshal.Copy(lHandle.AddrOfPinnedObject(), lData, lDataSize, lDeltaSize);
            lHandle.Free();
            lDataSize += lDeltaSize;
            lVertices = null;

            //  UV Channel 0
            Vector2[] lUVs = lMesh.uv;
            if (lUVs.Length > 0)
            {
                lUVFlag = true;
                lDeltaSize = lUVs.Length * lSizeOfVector2;
                Array.Resize(ref lData, lDataSize + lDeltaSize);
                lHandle = GCHandle.Alloc(lUVs, GCHandleType.Pinned);
                Marshal.Copy(lHandle.AddrOfPinnedObject(), lData, lDataSize, lDeltaSize);
                lHandle.Free();
                lDataSize += lDeltaSize;
            }
            lUVs = null;

            //  UV Channel 1
            lUVs = lMesh.uv2;
            if (lUVs.Length > 0)
            {
                lUV1Flag = true;
                lDeltaSize = lUVs.Length * lSizeOfVector2;
                Array.Resize(ref lData, lDataSize + lDeltaSize);
                lHandle = GCHandle.Alloc(lUVs, GCHandleType.Pinned);
                Marshal.Copy(lHandle.AddrOfPinnedObject(), lData, lDataSize, lDeltaSize);
                lHandle.Free();
                lDataSize += lDeltaSize;
            }
            lUVs = null;

            //  UV Channel 2
            lUVs = lMesh.uv2;
            if (lUVs.Length > 0)
            {
                lUV2Flag = true;
                lDeltaSize = lUVs.Length * lSizeOfVector2;
                Array.Resize(ref lData, lDataSize + lDeltaSize);
                lHandle = GCHandle.Alloc(lUVs, GCHandleType.Pinned);
                Marshal.Copy(lHandle.AddrOfPinnedObject(), lData, lDataSize, lDeltaSize);
                lHandle.Free();
                lDataSize += lDeltaSize;
            }
            lUVs = null;

            //  Normals
            Vector3[] lNormals = lMesh.normals;
            if (lNormals.Length > 0)
            {
                lNormalFlag = true;
                lDeltaSize = lNormals.Length * lSizeOfVector3;
                Array.Resize(ref lData, lDataSize + lDeltaSize);
                lHandle = GCHandle.Alloc(lNormals, GCHandleType.Pinned);
                Marshal.Copy(lHandle.AddrOfPinnedObject(), lData, lDataSize, lDeltaSize);
                lHandle.Free();
                lDataSize += lDeltaSize;
            }
            lNormals = null;

            //  Tangents
            Vector4[] lTangents = lMesh.tangents;
            if (lTangents.Length > 0)
            {
                lTangentFlag = true;
                lDeltaSize = lTangents.Length * lSizeOfVector4;
                Array.Resize(ref lData, lDataSize + lDeltaSize);
                lHandle = GCHandle.Alloc(lTangents, GCHandleType.Pinned);
                Marshal.Copy(lHandle.AddrOfPinnedObject(), lData, lDataSize, lDeltaSize);
                lHandle.Free();
                lDataSize += lDeltaSize;
            }
            lTangents = null;

            //  Colors
            Color[] lColors = lMesh.colors;
            if (lColors.Length > 0)
            {
                lColorFlag = true;
                lDeltaSize = lColors.Length * lSizeOfColor;
                Array.Resize(ref lData, lDataSize + lDeltaSize);
                lHandle = GCHandle.Alloc(lColors, GCHandleType.Pinned);
                Marshal.Copy(lHandle.AddrOfPinnedObject(), lData, lDataSize, lDeltaSize);
                lHandle.Free();
                lDataSize += lDeltaSize;
            }
            lColors = null;

            //  BindPoses
            Matrix4x4[] lBindPoses = lMesh.bindposes;
            if (lBindPoses.Length > 0)
            {
                lBindPoseFlag = true;
                lDeltaSize = lBindPoses.Length * lSizeOfMatrix4x4;
                Array.Resize(ref lData, lDataSize + lDeltaSize);
                lHandle = GCHandle.Alloc(lBindPoses, GCHandleType.Pinned);
                Marshal.Copy(lHandle.AddrOfPinnedObject(), lData, lDataSize, lDeltaSize);
                lHandle.Free();
                lDataSize += lDeltaSize;
            }
            lBindPoses = null;

            //  BoneWeight
            BoneWeight[] lBoneWeight = lMesh.boneWeights;
            if (lBoneWeight.Length > 0)
            {
                lBoneWeightFlag = true;
                lDeltaSize = lBoneWeight.Length * lSizeOfBoneWeight;
                Array.Resize(ref lData, lDataSize + lDeltaSize);
                lHandle = GCHandle.Alloc(lBoneWeight, GCHandleType.Pinned);
                Marshal.Copy(lHandle.AddrOfPinnedObject(), lData, lDataSize, lDeltaSize);
                lHandle.Free();
                lDataSize += lDeltaSize;
            }
            lBoneWeight = null;

            //  Indices
            int[] lIndices = lMesh.triangles;
            Int32Converter lIndexCount = lIndices.Length;
            lDeltaSize = lIndices.Length * sizeof(int);
            Array.Resize(ref lData, lDataSize + lDeltaSize);
            Buffer.BlockCopy(lIndices, 0, lData, lDataSize, lDeltaSize);
            lDataSize += lDeltaSize;
            lIndices = null;

            //  SubMesh Indices
            Int32Converter lSubMeshCount = lMesh.subMeshCount;
            for (int lSubMeshIndex = 0; lSubMeshIndex < lSubMeshCount; ++lSubMeshIndex)
            {
                lIndices = lMesh.GetTriangles(lSubMeshIndex);

                Int32Converter lSubMeshIndexCount = lIndices.Length;

                lDeltaSize = sizeof(int) + (lIndices.Length * sizeof(int));
                Array.Resize(ref lData, lDataSize + lDeltaSize);

                lData[lDataSize + 0] = lSubMeshIndexCount.Byte1;
                lData[lDataSize + 1] = lSubMeshIndexCount.Byte2;
                lData[lDataSize + 2] = lSubMeshIndexCount.Byte3;
                lData[lDataSize + 3] = lSubMeshIndexCount.Byte4;

                Buffer.BlockCopy(lIndices, 0, lData, lDataSize, lDeltaSize - sizeof(int));
                lDataSize += lDeltaSize;
            }

            //  Header
            lData[0] = (byte)(MESH_BINARY_SIGNATURE & 0xFF);
            lData[1] = (byte)((MESH_BINARY_SIGNATURE >> 8) & 0xFF);

            lData[2] = (byte)(MESH_BINARY_VERSION & 0xFF);
            lData[3] = (byte)((MESH_BINARY_VERSION >> 8) & 0xFF);

            lData[4] = lVertexCount.Byte1;
            lData[5] = lVertexCount.Byte2;
            lData[6] = lVertexCount.Byte3;
            lData[7] = lVertexCount.Byte4;

            lData[8] = lIndexCount.Byte1;
            lData[9] = lIndexCount.Byte2;
            lData[10] = lIndexCount.Byte3;
            lData[11] = lIndexCount.Byte4;

            lData[12] = lSubMeshCount.Byte1;
            lData[13] = lSubMeshCount.Byte2;
            lData[14] = lSubMeshCount.Byte3;
            lData[15] = lSubMeshCount.Byte4;

            lData[16] = (byte)(
                (lUVFlag ? 1 : 0) |
                (lUV1Flag ? 2 : 0) |
                (lUV2Flag ? 4 : 0) |
                (lNormalFlag ? 8 : 0) |
                (lTangentFlag ? 16 : 0) |
                (lColorFlag ? 32 : 0) |
                (lBindPoseFlag ? 64 : 0) |
                (lBoneWeightFlag ? 128 : 0));

            return lData;
        }
    }
}
