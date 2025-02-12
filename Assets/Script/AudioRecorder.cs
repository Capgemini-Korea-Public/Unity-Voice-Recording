using System;
using System.Collections;
using UnityEngine;
using System.IO;
using UnityEngine.Audio;
using UnityEditor;

public class AudioRecorder : MonoBehaviour
{
    public AudioClip recordedClip;
    public string microphoneDevice = null; // ����Ϸ��� ����ũ (null == �⺻)
    public int recordingDuration = 10; // ���� ������ �ִ� �ð�
    public int frequency = 44100; // ���� ����Ʈ

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

        recordedClip = Microphone.Start(selectedDevice, false, recordingDuration, frequency);
        Debug.Log("���� ����");

        if (audioSource != null)
        {
            audioSource.clip = recordedClip;
            audioSource.loop = true;
            audioSource.mute = true;
            audioSource.Play();
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
            
        Microphone.End(microphoneDevice);
        isRecording = false;
        Debug.Log("���� �ߴ�");

        if (recordedClip != null)
        {
            audioSource.mute = false;
            audioSource.clip = recordedClip;
            audioSource.loop = false;
            audioSource.Play(); // ������ ����� ���(�׽�Ʈ��)

            if (recordedClip == null)
            {
                Debug.LogError("������ Ŭ���� �����ϴ�.");
                return;
            }

            // ������ ȯ�濡���� "Assets/Sounds" ������ ����, ���� ȯ�濡���� persistentDataPath ���
#if UNITY_EDITOR
            string folderPath = "Assets/Sounds";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, recordedClip.name + ".wav");
            WavUtility.SaveWavFile(recordedClip, filePath);
            AssetDatabase.Refresh(); // ���� �����ͺ��̽� �����ؼ� Project â�� �ݿ�
#else
        string filePath = Path.Combine(Application.persistentDataPath, recordedClip.name + ".wav");
        WavUtility.SaveWavFile(recordedClip, filePath);
#endif

            Debug.Log("���� ���� �Ϸ�: " + filePath);
        }
    }
}

