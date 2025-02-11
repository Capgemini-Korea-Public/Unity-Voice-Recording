using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public class DeviceSelector : MonoBehaviour
{
    public TMP_Dropdown deviceDropdown;

    public event Action<string> OnDeviceSelected; // 선택된 장치를 다른 컴포넌트에 전달하는 이벤트

    void Start()
    {
        PopulateDeviceDropdown();
    }

    private void PopulateDeviceDropdown()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("사용 가능한 마이크 디바이스가 없습니다.");
            return;
        }

        List<string> options = new List<string>(Microphone.devices);

        foreach (string device in Microphone.devices)
        {
            Debug.Log("마이크 장치: " + device);

        }

        deviceDropdown.ClearOptions();
        deviceDropdown.AddOptions(options);

        deviceDropdown.onValueChanged.AddListener(OnDropdownChanged);

        if(deviceDropdown.options.Count > 0)
        {
            OnDropdownChanged(deviceDropdown.value);
        }
    }

    private void OnDropdownChanged(int value)
    {
        string selectedDevice = deviceDropdown.options[value].text;
        Debug.Log("선택된 장치 : " +  selectedDevice);
        OnDeviceSelected?.Invoke(selectedDevice);
    }
}
