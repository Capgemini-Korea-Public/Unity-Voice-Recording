using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public DeviceSelector deviceSelector;
    public AudioRecorder audioRecorder;

    void Awake()
    {
        deviceSelector = GetComponent<DeviceSelector>();
        audioRecorder = GetComponent<AudioRecorder>();

        deviceSelector.OnDeviceSelected += audioRecorder.SetDevice;
    }

    public void StartRecording() => audioRecorder.StartRecording();

    public void StopRecording() => audioRecorder.StopRecording();

    public void PlayRecording() => audioRecorder.PlayRecording();
}
