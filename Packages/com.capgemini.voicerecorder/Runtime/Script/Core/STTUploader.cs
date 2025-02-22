using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MyAudioPackage.Core
{
    /// <summary>
    /// STTUploaderCore는 WAV 파일의 바이트 데이터를 STT 서버에 업로드하는 순수 C# 유틸리티 클래스입니다.
    /// MonoBehaviour에 의존하지 않고, async/await 방식으로 비동기 업로드를 지원합니다.
    /// </summary>
    public static class STTUploader
    {
        /// <summary>
        /// WAV 파일의 바이트 데이터를 주어진 URL로 POST 방식 업로드합니다.
        /// 성공 시 응답 텍스트를 반환하며, 실패 시 예외를 발생시킵니다.
        /// </summary>
        /// <param name="wavData">업로드할 WAV 파일의 바이트 배열</param>
        /// <param name="url">업로드할 서버의 URL</param>
        /// <returns>서버의 응답 텍스트</returns>
        public static async Task<string> UploadWavBytesAsync(byte[] wavData, string url)
        {
            using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.uploadHandler = new UploadHandlerRaw(wavData);
                request.uploadHandler.contentType = "audio/wav";
                request.downloadHandler = new DownloadHandlerBuffer();

                // UnityWebRequestAsyncOperation을 기다리기 위해 Task.Yield()를 사용합니다.
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.text;
                }
                else
                {
                    throw new Exception("STT Upload Failed: " + request.error);
                }
            }
        }
    }
}
