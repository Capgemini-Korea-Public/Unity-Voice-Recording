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

        recordedClip = Microphone.Start(microphoneDevice, true, recordingDuration, frequency);
        Debug.Log("녹음 시작");

        if (analysisSource != null)
        {
            analysisSource.clip = recordedClip;
            analysisSource.loop = true;
            analysisSource.mute = true;
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
            playbackSource.outputAudioMixerGroup = normalGroup;
            playbackSource.Play();

            if (recordedClip == null)
            {
                Debug.LogError("녹음된 클립이 없습니다.");
                return;
            }

            // 녹음 중인 deviceName을 사용해 실제 녹음된 샘플 위치를 가져옵니다.
            
            if (lastSample <= 0)
            {
                Debug.LogError("실제 녹음된 샘플이 없습니다.");
                return;
            }

            // 녹음된 클립의 채널 수와 frequency를 가져옵니다.
            int channels = recordedClip.channels;
            int frequency = recordedClip.frequency;

            // 실제 녹음된 데이터만큼의 샘플 배열을 만듭니다.
            float[] samples = new float[lastSample * channels];
            recordedClip.GetData(samples, 0);

            // 실제 녹음된 부분만 담은 새로운 AudioClip 생성
            AudioClip trimmedClip = AudioClip.Create(recordedClip.name + "_trimmed", lastSample, channels, frequency, false);
            trimmedClip.SetData(samples, 0);

            // 이후 trimmedClip을 WAV 파일로 저장하면 실제 녹음된 길이만큼의 파일이 생성됩니다.
#if UNITY_EDITOR
            string folderPath = Path.Combine(Application.dataPath, "Sounds");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            string wavFilePath = Path.Combine(folderPath, trimmedClip.name + ".wav");
            WavUtility.SaveWavFile(trimmedClip, wavFilePath);
            AssetDatabase.Refresh();

            // ffmpeg.exe의 절대 경로 생성 (Assets/Plugin/ffmpeg/bin/ffmpeg.exe)
            string ffmpegPath = Path.Combine(Application.dataPath, "Plugins", "FFmpeg", "bin", "ffmpeg.exe");

            // FFmpeg를 사용해 WAV를 OGG로 변환
            string oggFilePath = Path.Combine(folderPath, trimmedClip.name + ".ogg");
            FFmpegConverter.ConvertWavToOgg(wavFilePath, oggFilePath, ffmpegPath);

            Debug.Log("실제 녹음된 길이만큼 저장 완료: " + wavFilePath);
#endif
        }
    }
}

