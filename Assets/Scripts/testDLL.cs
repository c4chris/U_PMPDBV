using UnityEngine;
using System.Runtime.InteropServices;

///
/// Need to test this : https://www.codeproject.com/Tips/318140/How-to-make-a-callback-to-Csharp-from-C-Cplusplus
///

public class testDLL : MonoBehaviour
{
    [DllImport("PMPDBVDLL")]
    public static extern short InitGlobals();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       Debug.Log("InitGlobals() returned " +  InitGlobals()); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
