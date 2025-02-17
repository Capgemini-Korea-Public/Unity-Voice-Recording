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
    public AudioClip recordedClip;
    public string microphoneDevice = null; // ����Ϸ��� ����ũ (null == �⺻)
    public int recordingDuration = 10; // ���� ������ �ִ� �ð�
    public int frequency = 44100; // ���� ����Ʈ
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
        if (!isRecording)
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
                    Debug.Log("���� �ɼ�: ���� �� ��");
                    break;
            }
        }
    }

    // ����� Ŭ���� �ٷ� STT�� ������ �޼���
    public void ProcessRecordingForSTT()
    {
        if (recordedClip == null)
        {
            Debug.LogWarning("������ Ŭ���� �����ϴ�.");
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
            Debug.LogWarning("����� ������ Ŭ���� �����ϴ�.");
            return;
        }

        // ����� AudioSource�� ������ Ŭ���� �Ҵ��ϰ� ����մϴ�.
        playbackSource.clip = recordedClip;
        playbackSource.loop = false;
        playbackSource.Play();

        Debug.Log("���� ��� ����");
    }
}

