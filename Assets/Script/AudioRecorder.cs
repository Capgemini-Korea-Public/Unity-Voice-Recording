using System;
using System.Collections;
using UnityEngine;
using System.IO;
using UnityEngine.Audio;

public class AudioRecorder : MonoBehaviour
{
    public AudioClip recordedClip;
    public string microphoneDevice = null; // 사용하려는 마이크 (null == 기본)
    public int recordingDuration = 10; // 녹음 가능한 최대 시간
    public int frequency = 44100; // 샘플 레이트

    private string selectedDevice;
    private bool isRecording = false;
    private AudioSource audioSource;
    private WaitForSeconds recordingDurationSeconds = new WaitForSeconds(10);

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if(audioSource == null )
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        //StartCoroutine(StartRecording());
    }

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

        recordedClip = Microphone.Start(selectedDevice, false, recordingDuration, frequency);
        Debug.Log("녹음 시작");
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

        if (recordedClip != null)
        {
            audioSource.clip = recordedClip;
            audioSource.Play(); // 녹음된 오디오 재생(테스트용)

            string filePath = Path.Combine(Application.persistentDataPath, recordedClip.name);
            WavUtility.SaveWavFile(recordedClip, filePath);
        }
    }
}

