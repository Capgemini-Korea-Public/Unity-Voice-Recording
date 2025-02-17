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
    public AudioClip recordedClip;
    public string microphoneDevice = null; // 사용하려는 마이크 (null == 기본)
    public int recordingDuration = 10; // 녹음 가능한 최대 시간
    public int frequency = 44100; // 샘플 레이트
    public string sttUrl;

    private string selectedDevice;
    private bool isRecording = false;
    private WaitForSeconds recordingDurationSeconds = new WaitForSeconds(10);

    public AudioFileManager fileManager;
    public AudioProcessingManager processingManager;
    public STTuploader sttuploader;

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

        recordedClip = Microphone.Start(microphoneDevice, true, recordingDuration, frequency);
        Debug.Log("녹음 시작");

        if (analysisSource != null)
        {
            analysisSource.clip = recordedClip;
            analysisSource.loop = true;
            analysisSource.mute = true;
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
        if (!isRecording)
        {
            Debug.Log("현재 녹음 중이 아님");
            return;
        }

        int lastSample = Microphone.GetPosition(microphoneDevice);
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
            playbackSource.Play();
        }

        if (processingManager != null)
        {
            SaveMode mode = processingManager.SelectedSaveMode;
            switch (mode)
            {
                case SaveMode.SaveWav:
                    fileManager.SaveAsWav(recordedClip, lastSample);
                    break;
                case SaveMode.SaveOgg:
                    fileManager.SaveAsOgg(recordedClip, lastSample);
                    break;
                case SaveMode.None:
                    Debug.Log("저장 옵션: 저장 안 함");
                    break;
            }
        }
    }

    // 오디오 클립을 바로 STT에 보내는 메서드
    public void ProcessRecordingForSTT()
    {
        if (recordedClip == null)
        {
            Debug.LogWarning("녹음된 클립이 없습니다.");
            return;
        }

        int sampleCount = recordedClip.samples * recordedClip.channels;
        float[] samples = new float[sampleCount];
        recordedClip.GetData(samples, 0);

        //Tensor<float> audioTensor = new Tensor<float>(new TensorShape(1, sampleCount), samples);
        //STTModule.ProcessAudio(audioTensor);
    }

    IEnumerator UploadAudioClipToSTT()
    {
        byte[] wavBytes = WavByteUtility.ConvertAudioClipToWavBytes(recordedClip);

        yield return StartCoroutine(sttuploader.UploadWavBytes(wavBytes, sttUrl));
    }

    public void PlayRecording()
    {
        if (recordedClip == null)
        {
            Debug.LogWarning("재생할 녹음된 클립이 없습니다.");
            return;
        }

        // 재생용 AudioSource에 녹음된 클립을 할당하고 재생합니다.
        playbackSource.clip = recordedClip;
        playbackSource.loop = false;
        playbackSource.Play();

        Debug.Log("녹음 재생 시작");
    }
}

