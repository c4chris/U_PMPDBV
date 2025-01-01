using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using static UnityEngine.Rendering.DebugUI;
using Unity.VisualScripting;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PDBentry : MonoBehaviour
{
    public struct Point3D
    {
        double x, y, z;
        public override readonly string ToString() => $"({x},{y},{z})";
    };

    public struct FPoint3D
    {
        float x, y, z;
        public override readonly string ToString() => $"({x},{y},{z})";
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

    public string entry = "1crn";
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
        Debug.Log("InitGlobals() returned " +  InitGlobals()); 
        // Get entry
        StartCoroutine(GetText(entry, (path) => { EntryLoaded(path); }));
    }

    void Update()
    {
    }

    private void EntryLoaded(string path)
    {
        Debug.Log("We have received entry " + entry + " at " + path);
        doPDBinput(path, entry);
        Debug.Log("We have loaded entry " + entry);
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
        Debug.Log("GL_atm(" + A + ", " + layer + ", " + group + ", " + atom + ", " + c + ", " + color + ")");
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
