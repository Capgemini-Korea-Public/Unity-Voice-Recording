using System;
using System.Collections;
using UnityEngine;
using System.IO;
using UnityEngine.Audio;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class AudioRecorder : MonoBehaviour
{
    public AudioSource analysisSource;  // ���� �� �ǽð� �м��� AudioSource
    public AudioSource playbackSource;  // ���� ����� AudioSource
    public AudioClip recordedClip;
    public TMP_InputField durationInputField;
    public string microphoneDevice = null; // ����Ϸ��� ����ũ (null == �⺻)
    public string sttUrl;
    public TextMeshProUGUI maxRecordingDurationTxt;
    public int recordingDuration = 10; // ���� ������ �ִ� ��
    public int frequency = 44100; // ���� ����Ʈ
    public bool isRecording = false;
    public Button StopBtn;

    private string selectedDevice;

    public AudioFileManager fileManager;
    public AudioProcessingManager processingManager;
    public STTuploader sttuploader;

    void Start()
    {
        durationInputField.onValueChanged.AddListener(OnDurationChanged);
    }

    void OnDurationChanged(string value)
    {
        if (int.TryParse(value, out int result))
        {
            recordingDuration = Mathf.Clamp(result, 1, 30);
            maxRecordingDurationTxt.text = "Max Recording Duration : " + value;
            Debug.Log($"Recording duration set to {recordingDuration} seconds");
        }
    }

    public void SetDevice(string deviceName)
    {
        selectedDevice = deviceName;
    }

    public void StartRecording()
    {
        if(isRecording)
        {
            Debug.Log("Recording is already in progress.");
            return;
        }

        if (string.IsNullOrEmpty(selectedDevice))
        {
            if (Microphone.devices.Length > 0)
            {
                selectedDevice = Microphone.devices[0];
                Debug.Log("Automatically selected default microphone device: " + selectedDevice);
            }
        }

        StopBtn.interactable = true;
        microphoneDevice = selectedDevice;

        recordedClip = Microphone.Start(microphoneDevice, true, recordingDuration, frequency);
        Debug.Log("Recording started.");

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

        yield return new WaitForSeconds(recordingDuration);

        StopRecording();
    }

    public void StopRecording()
    {
        if (!isRecording)
        {
            Debug.Log("Not currently recording.");
            return;
        }

        int lastSample = Microphone.GetPosition(microphoneDevice);
        Microphone.End(microphoneDevice);
        isRecording = false;
        StopBtn.interactable = false;
        Debug.Log("Recording stopped.");

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
                    Debug.Log("Save mode : None");
                    break;
            }
        }
    }

    // ����� Ŭ���� �ٷ� STT�� ������ �޼���
    public void ProcessRecordingForSTT()
    {
        if (recordedClip == null)
        {
            Debug.LogWarning("No recorded clip available.");
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
            Debug.LogWarning("No recorded clip available for playback.");
            return;
        }

        // ����� AudioSource�� ������ Ŭ���� �Ҵ��ϰ� ����մϴ�.
        playbackSource.clip = recordedClip;
        playbackSource.loop = false;
        playbackSource.Play();

        Debug.Log("Playback started.");
    }
}

