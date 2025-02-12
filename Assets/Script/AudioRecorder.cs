using System;
using System.Collections;
using UnityEngine;
using System.IO;
using UnityEngine.Audio;
using UnityEditor;

public class AudioRecorder : MonoBehaviour
{
    public AudioSource analysisSource;  // 녹음 및 실시간 분석용 AudioSource
    public AudioSource playbackSource;  // 녹음 재생용 AudioSource
    public AudioMixerGroup mutedGroup;  // 실시간 분석용 그룹 (볼륨이 낮게 설정됨)
    public AudioMixerGroup normalGroup; // 재생용 그룹 (정상 볼륨)

    public AudioClip recordedClip;
    public string microphoneDevice = null; // 사용하려는 마이크 (null == 기본)
    public int recordingDuration = 10; // 녹음 가능한 최대 시간
    public int frequency = 44100; // 샘플 레이트

    private string selectedDevice;
    private bool isRecording = false;
    private AudioSource audioSource;
    private WaitForSeconds recordingDurationSeconds = new WaitForSeconds(10);

    public void SetDevice(string deviceName)
    {
        selectedDevice = deviceName;
    }

    public void StartRecording()
    {
        if(isRecording)
        {
            Debug.Log("이미 녹음 중입니다.");
            return;
        }

        if (string.IsNullOrEmpty(selectedDevice))
        {
            if (Microphone.devices.Length > 0)
            {
                selectedDevice = Microphone.devices[0];
                Debug.Log("기본 마이크 장치로 자동 선택됨: " + selectedDevice);
            }
            else
            {
                Debug.LogWarning("사용 가능한 마이크가 없습니다.");
                return;
            }
        }

        microphoneDevice = selectedDevice;

        recordedClip = Microphone.Start(selectedDevice, true, recordingDuration, frequency);
        Debug.Log("녹음 시작");

        if (analysisSource != null)
        {
            analysisSource.clip = recordedClip;
            analysisSource.loop = true;
            analysisSource.outputAudioMixerGroup = mutedGroup; //audioSource.mute를 해줄 시에 실시간 볼륨미터가 적용되지 않음
            analysisSource.Play();
        }

        StartCoroutine(WaitForRecordingStart());
    }

    IEnumerator WaitForRecordingStart()
    {
        while(!(Microphone.GetPosition(microphoneDevice) > 0))
        {
            yield return null;
        }

        isRecording = true;

        yield return recordingDurationSeconds; // 10초 뒤 자동 종료

        StopRecording();
    }

    public void StopRecording()
    {
        if(!isRecording)
        {
            Debug.Log("현재 녹음 중이 아님");
            return;
        }
            
        Microphone.End(microphoneDevice);
        isRecording = false;
        Debug.Log("녹음 중단");

        if (analysisSource != null && analysisSource.isPlaying)
        {
            analysisSource.Stop();
        }

        if (recordedClip != null)
        {
            playbackSource.clip = recordedClip;
            playbackSource.loop = false;
            playbackSource.outputAudioMixerGroup = normalGroup;
            playbackSource.Play();

            if (recordedClip == null)
            {
                Debug.LogError("녹음된 클립이 없습니다.");
                return;
            }

            // 에디터 환경에서는 "Assets/Sounds" 폴더에 저장, 빌드 환경에서는 persistentDataPath 사용
#if UNITY_EDITOR
            string folderPath = "Assets/Sounds";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, recordedClip.name + ".wav");
            WavUtility.SaveWavFile(recordedClip, filePath);
            AssetDatabase.Refresh(); // 에셋 데이터베이스 갱신해서 Project 창에 반영
#else
        string filePath = Path.Combine(Application.persistentDataPath, recordedClip.name + ".wav");
        WavUtility.SaveWavFile(recordedClip, filePath);
#endif

            Debug.Log("파일 저장 완료: " + filePath);
        }
    }
}

