using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class STTuploader : MonoBehaviour
{
    public IEnumerator UploadWavBytes(byte[] wavData, string url)
    {
        UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);

        request.uploadHandler = new UploadHandlerRaw(wavData);
        request.uploadHandler.contentType = "audio/wav";

        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if(request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            Debug.Log("STT Upload Success :" + responseText);
        }
        else
        {
            Debug.Log("STT Upload Failed :" + request.error);
        }
    }
}