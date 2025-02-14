using System;
using System.Collections;
using UnityEngine;
using System.IO;
using UnityEngine.Audio;
using UnityEditor;

public class AudioRecorder : MonoBehaviour
{
    public AudioSource analysisSource;  // ���� �� �ǽð� �м��� AudioSource
    public AudioSource playbackSource;  // ���� ����� AudioSource
    public AudioMixerGroup mutedGroup;  // �ǽð� �м��� �׷� (������ ���� ������)
    public AudioMixerGroup normalGroup; // ����� �׷� (���� ����)

    public AudioClip recordedClip;
    public string microphoneDevice = null; // ����Ϸ��� ����ũ (null == �⺻)
    public int recordingDuration = 10; // ���� ������ �ִ� �ð�
    public int frequency = 44100; // ���� ����Ʈ

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
            Debug.Log("�̹� ���� ���Դϴ�.");
            return;
        }

        if (string.IsNullOrEmpty(selectedDevice))
        {
            if (Microphone.devices.Length > 0)
            {
                selectedDevice = Microphone.devices[0];
                Debug.Log("�⺻ ����ũ ��ġ�� �ڵ� ���õ�: " + selectedDevice);
            }
            else
            {
                Debug.LogWarning("��� ������ ����ũ�� �����ϴ�.");
                return;
            }
        }

        microphoneDevice = selectedDevice;

        recordedClip = Microphone.Start(microphoneDevice, true, recordingDuration, frequency);
        Debug.Log("���� ����");

        if (analysisSource != null)
        {
            analysisSource.clip = recordedClip;
            analysisSource.loop = true;
            analysisSource.mute = true;
            analysisSource.outputAudioMixerGroup = mutedGroup; //audioSource.mute�� ���� �ÿ� �ǽð� �������Ͱ� ������� ����
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

        yield return recordingDurationSeconds; // 10�� �� �ڵ� ����

        StopRecording();
    }

    public void StopRecording()
    {
        if(!isRecording)
        {
            Debug.Log("���� ���� ���� �ƴ�");
            return;
        }

        int lastSample = Microphone.GetPosition(microphoneDevice);
        Microphone.End(microphoneDevice);
        isRecording = false;
        Debug.Log("���� �ߴ�");

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
                Debug.LogError("������ Ŭ���� �����ϴ�.");
                return;
            }

            // ���� ���� deviceName�� ����� ���� ������ ���� ��ġ�� �����ɴϴ�.
            
            if (lastSample <= 0)
            {
                Debug.LogError("���� ������ ������ �����ϴ�.");
                return;
            }

            // ������ Ŭ���� ä�� ���� frequency�� �����ɴϴ�.
            int channels = recordedClip.channels;
            int frequency = recordedClip.frequency;

            // ���� ������ �����͸�ŭ�� ���� �迭�� ����ϴ�.
            float[] samples = new float[lastSample * channels];
            recordedClip.GetData(samples, 0);

            // ���� ������ �κи� ���� ���ο� AudioClip ����
            AudioClip trimmedClip = AudioClip.Create(recordedClip.name + "_trimmed", lastSample, channels, frequency, false);
            trimmedClip.SetData(samples, 0);

            // ���� trimmedClip�� WAV ���Ϸ� �����ϸ� ���� ������ ���̸�ŭ�� ������ �����˴ϴ�.
#if UNITY_EDITOR
            string folderPath = Path.Combine(Application.dataPath, "Sounds");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            string wavFilePath = Path.Combine(folderPath, trimmedClip.name + ".wav");
            WavUtility.SaveWavFile(trimmedClip, wavFilePath);
            AssetDatabase.Refresh();

            // ffmpeg.exe�� ���� ��� ���� (Assets/Plugin/ffmpeg/bin/ffmpeg.exe)
            string ffmpegPath = Path.Combine(Application.dataPath, "Plugins", "FFmpeg", "bin", "ffmpeg.exe");

            // FFmpeg�� ����� WAV�� OGG�� ��ȯ
            string oggFilePath = Path.Combine(folderPath, trimmedClip.name + ".ogg");
            FFmpegConverter.ConvertWavToOgg(wavFilePath, oggFilePath, ffmpegPath);

            Debug.Log("���� ������ ���̸�ŭ ���� �Ϸ�: " + wavFilePath);
#endif
        }
    }
}

