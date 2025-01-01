using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

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

public class Example : MonoBehaviour
{
    void Start()
    {
        // A correct website page.
        StartCoroutine(GetRequest("https://www.example.com"));

        // A non-existing page.
        StartCoroutine(GetRequest("https://error.html"));
        // Get entry
        StartCoroutine(GetText("1crn"));
    }

    IEnumerator GetText(string entry_name)
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
                    System.IO.File.WriteAllText(savePath, www.downloadHandler.text);
                    Debug.Log(entry_name + ":\nReceived: " + www.downloadHandler.text);
                    break;
            }
        }
    }
    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }
        }
    }
}
