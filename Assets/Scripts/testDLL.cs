using UnityEngine;
using System.Runtime.InteropServices;
using System;

///
/// Need to test this : https://www.codeproject.com/Tips/318140/How-to-make-a-callback-to-Csharp-from-C-Cplusplus
///

public class testDLL : MonoBehaviour
{
    [DllImport("PMPDBVDLL")]
    public static extern void DebugCBSetup(CallbackDebugDelegate callback);
    [DllImport("PMPDBVDLL")]
    public static extern short InitGlobals();

    public delegate void CallbackDebugDelegate(IntPtr msg);
    private static CallbackDebugDelegate delegateInstance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        delegateInstance = DebugDelegate;
        DebugCBSetup(delegateInstance);
        Debug.Log("InitGlobals() returned " +  InitGlobals()); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public static void DebugDelegate(IntPtr c)
    {
        string s = Marshal.PtrToStringAnsi(c);
        Debug.Log("From DLL: " + s);
    }
}
