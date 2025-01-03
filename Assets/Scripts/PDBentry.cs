using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PDBentry : MonoBehaviour
{
    public struct Point3D
    {
        public double x, y, z;
        public override readonly string ToString() => $"({x},{y},{z})";
    };

    public struct FPoint3D
    {
        public float x, y, z;
        public override readonly string ToString() => $"({x},{y},{z})";
    };
    public struct CubeSphereGenerator
	{
        public int gridSize;

        public float radius;

        private Mesh mesh;
        private Vector3[] vertices;
        private Vector3[] normals;
        private Color32[] cubeUV;

        public Mesh Generate (string name) {
            mesh = new Mesh();
            mesh.name = name;
            CreateVertices();
            CreateTriangles();
            return mesh;
        }

        private void CreateVertices () {
            int cornerVertices = 8;
            int edgeVertices = (gridSize + gridSize + gridSize - 3) * 4;
            int faceVertices = (
                (gridSize - 1) * (gridSize - 1) +
                (gridSize - 1) * (gridSize - 1) +
                (gridSize - 1) * (gridSize - 1)) * 2;
            vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
            normals = new Vector3[vertices.Length];
            cubeUV = new Color32[vertices.Length];

            int v = 0;
            for (int y = 0; y <= gridSize; y++) {
                for (int x = 0; x <= gridSize; x++) {
                    SetVertex(v++, x, y, 0);
                }
                for (int z = 1; z <= gridSize; z++) {
                    SetVertex(v++, gridSize, y, z);
                }
                for (int x = gridSize - 1; x >= 0; x--) {
                    SetVertex(v++, x, y, gridSize);
                }
                for (int z = gridSize - 1; z > 0; z--) {
                    SetVertex(v++, 0, y, z);
                }
            }
            for (int z = 1; z < gridSize; z++) {
                for (int x = 1; x < gridSize; x++) {
                    SetVertex(v++, x, gridSize, z);
                }
            }
            for (int z = 1; z < gridSize; z++) {
                for (int x = 1; x < gridSize; x++) {
                    SetVertex(v++, x, 0, z);
                }
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.colors32 = cubeUV;
        }

        private void SetVertex (int i, int x, int y, int z) {
            Vector3 v = new Vector3(x, y, z) * 2f / gridSize - Vector3.one;
            float x2 = v.x * v.x;
            float y2 = v.y * v.y;
            float z2 = v.z * v.z;
            Vector3 s;
            s.x = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
            s.y = v.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
            s.z = v.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);
            normals[i] = s;
            vertices[i] = normals[i] * radius;
            cubeUV[i] = new Color32((byte)x, (byte)y, (byte)z, 0);
        }

        private void CreateTriangles () {
            int[] trianglesZ = new int[(gridSize * gridSize) * 12];
            int[] trianglesX = new int[(gridSize * gridSize) * 12];
            int[] trianglesY = new int[(gridSize * gridSize) * 12];
            int ring = (gridSize + gridSize) * 2;
            int tZ = 0, tX = 0, tY = 0, v = 0;

            for (int y = 0; y < gridSize; y++, v++) {
                for (int q = 0; q < gridSize; q++, v++) {
                    tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
                }
                for (int q = 0; q < gridSize; q++, v++) {
                    tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
                }
                for (int q = 0; q < gridSize; q++, v++) {
                    tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
                }
                for (int q = 0; q < gridSize - 1; q++, v++) {
                    tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
                }
                tX = SetQuad(trianglesX, tX, v, v - ring + 1, v + ring, v + 1);
            }

            tY = CreateTopFace(trianglesY, tY, ring);
            tY = CreateBottomFace(trianglesY, tY, ring);

            mesh.subMeshCount = 3;
            mesh.SetTriangles(trianglesZ, 0);
            mesh.SetTriangles(trianglesX, 1);
            mesh.SetTriangles(trianglesY, 2);
        }

        private int CreateTopFace (int[] triangles, int t, int ring) {
            int v = ring * gridSize;
            for (int x = 0; x < gridSize - 1; x++, v++) {
                t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
            }
            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

            int vMin = ring * (gridSize + 1) - 1;
            int vMid = vMin + 1;
            int vMax = v + 2;

            for (int z = 1; z < gridSize - 1; z++, vMin--, vMid++, vMax++) {
                t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + gridSize - 1);
                for (int x = 1; x < gridSize - 1; x++, vMid++) {
                    t = SetQuad(
                        triangles, t,
                        vMid, vMid + 1, vMid + gridSize - 1, vMid + gridSize);
                }
                t = SetQuad(triangles, t, vMid, vMax, vMid + gridSize - 1, vMax + 1);
            }

            int vTop = vMin - 2;
            t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
            for (int x = 1; x < gridSize - 1; x++, vTop--, vMid++) {
                t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
            }
            t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);

            return t;
        }

        private int CreateBottomFace (int[] triangles, int t, int ring) {
            int v = 1;
            int vMid = vertices.Length - (gridSize - 1) * (gridSize - 1);
            t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
            for (int x = 1; x < gridSize - 1; x++, v++, vMid++) {
                t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
            }
            t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

            int vMin = ring - 2;
            vMid -= gridSize - 2;
            int vMax = v + 2;

            for (int z = 1; z < gridSize - 1; z++, vMin--, vMid++, vMax++) {
                t = SetQuad(triangles, t, vMin, vMid + gridSize - 1, vMin + 1, vMid);
                for (int x = 1; x < gridSize - 1; x++, vMid++) {
                    t = SetQuad(
                        triangles, t,
                        vMid + gridSize - 1, vMid + gridSize, vMid, vMid + 1);
                }
                t = SetQuad(triangles, t, vMid + gridSize - 1, vMax + 1, vMid, vMax);
            }

            int vTop = vMin - 1;
            t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
            for (int x = 1; x < gridSize - 1; x++, vTop--, vMid++) {
                t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
            }
            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

            return t;
        }

        private static int SetQuad (int[] triangles, int i, int v00, int v10, int v01, int v11) {
            triangles[i] = v00;
            triangles[i + 1] = triangles[i + 4] = v01;
            triangles[i + 2] = triangles[i + 3] = v10;
            triangles[i + 5] = v11;
            return i + 6;
        }

    }

    /// copied from https://github.com/lazysquirrellabs/sphere_generator in IcosphereGenerator.cs
    /// <summary>
    /// Vertices of a regular icosahedron, obtained via experimentation.
    /// </summary>
    private static readonly Vector3[] IcosphereVertices =
    {
        new(0.8506508f,           0.5257311f,         0f),            // 0
        new(0.000000101405476f,   0.8506507f,        -0.525731f),     // 1
        new(0.000000101405476f,   0.8506506f,         0.525731f),     // 2
        new(0.5257309f,          -0.00000006267203f, -0.85065067f),   // 3
        new(0.52573115f,         -0.00000006267203f,  0.85065067f),   // 4
        new(0.8506508f,          -0.5257311f,         0f),            // 5
        new(-0.52573115f,         0.00000006267203f, -0.85065067f),   // 6
        new(-0.8506508f,          0.5257311f,         0f),            // 7
        new(-0.5257309f,          0.00000006267203f,  0.85065067f),   // 8
        new(-0.000000101405476f, -0.8506506f,        -0.525731f),     // 9
        new(-0.000000101405476f, -0.8506507f,         0.525731f),     // 10
        new(-0.8506508f,         -0.5257311f,         0f)             // 11
    };

    /// <summary>
    /// Indices of the triangles of a regular icosahedron, obtained via experimentation.
    /// </summary>
    private static readonly int[] IcosphereIndices =
    {
         0,  1,  2,
         0,  3,  1,
         0,  2,  4,
         3,  0,  5,
         0,  4,  5,
         1,  3,  6,
         1,  7,  2,
         7,  1,  6,
         4,  2,  8,
         7,  8,  2,
         9,  3,  5,
         6,  3,  9,
         5,  4, 10,
         4,  8, 10,
         9,  5, 10,
         7,  6, 11,
         7, 11,  8,
        11,  6,  9,
         8, 11, 10,
        10, 11,  9
    };

    public delegate void GL_atmDelegate(FPoint3D A, short layer, ushort group, ushort atom, byte chain, long color);
    public delegate void GL_bndDelegate(FPoint3D A, FPoint3D B, short layer, ushort group, ushort atomA, ushort atomB, byte chain, long color);
    public delegate void GL_Atom_ColorDelegate(IntPtr name, double red, double green, double blue, Boolean isMetallic);
    public delegate void GL_All_InvisibleDelegate(short layer);
    public delegate void GL_ForceRebuildDelegate(short layer);
    public delegate void GL_New_ShapeDelegate();
    public delegate void GL_Add_VertexDelegate(Point3D V, Point3D N, uint color);
    public delegate void GL_Draw_ShapeDelegate(short layer, byte type);
    public struct SwiftCallbacks
    {
        public GL_atmDelegate GL_atm;
        public GL_bndDelegate GL_bnd;
        public GL_Atom_ColorDelegate GL_Atom_Color;
        public GL_All_InvisibleDelegate GL_All_Invisible;
        public GL_ForceRebuildDelegate GL_ForceRebuild;
        public GL_New_ShapeDelegate GL_New_Shape;
        public GL_Add_VertexDelegate GL_Add_Vertex;
        public GL_Draw_ShapeDelegate GL_Draw_Shape;
    };

    [DllImport("PMPDBVDLL")]
    public static extern void DebugCBSetup(CallbackDebugDelegate callback);
    [DllImport("PMPDBVDLL")]
    public static extern short InitGlobals();
    [DllImport("PMPDBVDLL")]
    public static extern void doPDBinput(string path, string name);
    [DllImport("PMPDBVDLL")]
    public static extern void SwiftCBSetup(SwiftCallbacks callbacks);
    [DllImport("PMPDBVDLL")]
    public static extern void GLRender();

    public delegate void CallbackDebugDelegate(IntPtr msg);
    private static CallbackDebugDelegate debugDelegateInstance;
    private static SwiftCallbacks callbacksInstance;
    private static List<CombineInstance> meshList;

    public string entry = "1crn";
    private int nb = 0;
    void Start()
    {
        debugDelegateInstance = DebugDelegate;
        DebugCBSetup(debugDelegateInstance);
        callbacksInstance = new SwiftCallbacks
        {
            GL_atm = GL_atm,
            GL_bnd = GL_bnd,
            GL_Atom_Color = GL_Atom_Color,
            GL_All_Invisible = GL_All_Invisible,
            GL_ForceRebuild = GL_ForceRebuild,
            GL_New_Shape = GL_New_Shape,
            GL_Add_Vertex = GL_Add_Vertex,
            GL_Draw_Shape = GL_Draw_Shape
        };
        SwiftCBSetup(callbacksInstance);
        meshList = new List<CombineInstance>();
        Debug.Log("InitGlobals() returned " +  InitGlobals()); 
        // Get entry
        StartCoroutine(GetText(entry, (path) => { EntryLoaded(path); }));
    }

    void Update()
    {
        if (meshList.Count !=  nb)
        {
            nb = meshList.Count;
            Mesh mesh = new Mesh();
            // Uncomment this if there are more than 65k vertices per mesh...
            //mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.CombineMeshes(meshList.ToArray());
            Debug.Log("Creating new mesh with " + mesh.vertexCount + " vertices");
            // if we do not combine the submeshes, we have to set 1 material per submesh
            //mesh.CombineMeshes(meshList.ToArray(), false);
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }
    }

    private void EntryLoaded(string path)
    {
        Debug.Log("We have received entry " + entry + " at " + path);
        doPDBinput(path, entry);
        Debug.Log("We have loaded entry " + entry);
        meshList.Clear();
        GLRender();
    }

    IEnumerator GetText(string entry_name, Action<string> loaded)
    {
        string url = "https://files.rcsb.org/download/" + entry_name + ".pdb";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            switch (www.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(entry_name + ": Error: " + www.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(entry_name + ": HTTP Error: " + www.error);
                    break;
                case UnityWebRequest.Result.Success:
                    string savePath = string.Format("{0}/{1}.pdb", Application.persistentDataPath, entry_name);
                    Debug.Log("Saving " + entry_name + " into " + savePath);
                    System.IO.File.WriteAllText(savePath, www.downloadHandler.text);
                    Debug.Log(entry_name + ":\nReceived: " + www.downloadHandler.text);
                    loaded(savePath);
                    break;
            }
        }
    }

    public static void DebugDelegate(IntPtr c)
    {
        string s = Marshal.PtrToStringAnsi(c);
        Debug.Log("From DLL: " + s);
    }
    public static void GL_atm(FPoint3D A, short layer, ushort group, ushort atom, byte chain, long color)
    {
        char c = Convert.ToChar(chain);
        //Debug.Log("GL_atm(" + A + ", " + layer + ", " + group + ", " + atom + ", " + c + ", " + color + ")");
        /// the cubeSpheres might look nicer but use a lot of vertices...  Use an icosahedron for now
        //CubeSphereGenerator gen = new CubeSphereGenerator
        //{
        //    radius = 1f,
        //    gridSize = 5
        //};
        Mesh mesh = new Mesh();
        // Not sure what to do with the name atm
        mesh.name = "name";
        mesh.SetVertices(IcosphereVertices);
        mesh.SetIndices(IcosphereIndices, MeshTopology.Triangles, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        CombineInstance ci = new CombineInstance
        {
            //mesh = gen.Generate("name"),
            mesh = mesh,
            transform = new Matrix4x4(new Vector4(1, 0, 0, 0),
                                      new Vector4(0, 1, 0, 0),
                                      new Vector4(0, 0, 1, 0),
                                      new Vector4(A.x, A.y, A.z, 1))
        };
        meshList.Add(ci);
    }
    public static void GL_bnd(FPoint3D A, FPoint3D B, short layer, ushort group, ushort atomA, ushort atomB, byte chain, long color)
    {
        char c = Convert.ToChar(chain);
        Debug.Log("GL_bnd(" + A + ", " + B + ", " + layer + ", " + group + ", " + atomA + ", " + atomB + ", " + c + ", " + color + ")");
    }
    public static void GL_Atom_Color(IntPtr name, double red, double green, double blue, Boolean isMetallic)
    {
        string s = Marshal.PtrToStringAnsi(name);
        Debug.Log("GL_Atom_Color(" + s + ", " + red + ", " + green + ", " + blue + ", " + isMetallic + ")");
    }
    public static void GL_All_Invisible(short layer)
    {
        Debug.Log("GL_All_Invisible(" + layer + ")");
    }
    public static void GL_ForceRebuild(short layer)
    {
        Debug.Log("GL_ForceRebuild(" + layer + ")");
    }
    public static void GL_New_Shape()
    {
        Debug.Log("GL_New_Shape()");
    }
    public static void GL_Add_Vertex(Point3D V, Point3D N, uint color)
    {
        Debug.Log("GL_Add_Vertex(" + V.ToString() + ", " + N.ToString() + ", " + color + ")");
    }
    public static void GL_Draw_Shape(short layer, byte type)
    {
        Debug.Log("GL_Draw_Shape(" + layer + ", " + type + ")");
    }
}
