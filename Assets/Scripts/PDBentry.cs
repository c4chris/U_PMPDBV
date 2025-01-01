using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Runtime.InteropServices;

// UnityWebRequest.Get example

// Access a website and use UnityWebRequest.Get to download a page.
// Also try to download a non-existing page. Display the error.

// Probably will want a callback at some point, e.g.:
//public IEnumerator CoroutineFunction(Action<int> callback)
//    {
//    yield return new waitForSeconds(20);
//    callback(1)
//    }
//
//StartCoroutine(CoroutineFunction(result =>
//{
//myVariable = result;
//})
//);

//WWW www = new WWW("http://google.com");
//
//StartCoroutine(WaitForRequest(www,(status)=>{
//    print(status.ToString());
//}));

//private IEnumerator WaitForRequest(WWW www,Action<int> callback) {
//    int tempInt = 0;
//    yield return www;
//    if (string.IsNullOrEmpty(www.error)) {
//        if(!string.IsNullOrEmpty(www.text)) {
//            tempInt = 3;
//        }
//        else {
//            tempInt=2;
//        }
//    } else {
//        print(www.error);
//        tempInt=1;
//    }
//    callback(tempInt);
//}

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PDBentry : MonoBehaviour
{
    [DllImport("PMPDBVDLL")]
    public static extern void DebugCBSetup(CallbackDebugDelegate callback);
    [DllImport("PMPDBVDLL")]
    public static extern short InitGlobals();
    [DllImport("PMPDBVDLL")]
    public static extern void doPDBinput(string path, string name);

    public delegate void CallbackDebugDelegate(IntPtr msg);
    private static CallbackDebugDelegate delegateInstance;

    public string entry = "1crn";
    void Start()
    {
        delegateInstance = DebugDelegate;
        DebugCBSetup(delegateInstance);
        Debug.Log("InitGlobals() returned " +  InitGlobals()); 
        // Get entry
        StartCoroutine(GetText(entry, (path) => { EntryLoaded(path); }));
    }

    void Update()
    {
        
    }

    private void EntryLoaded(string path)
    {
        Debug.Log("We have loaded entry " + entry + " at " + path);
        doPDBinput(path, entry); 
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
}
