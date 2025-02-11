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

    public event Action<string> OnDeviceSelected; // ���õ� ��ġ�� �ٸ� ������Ʈ�� �����ϴ� �̺�Ʈ

    void Start()
    {
        PopulateDeviceDropdown();
    }

    private void PopulateDeviceDropdown()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("��� ������ ����ũ ����̽��� �����ϴ�.");
            return;
        }

        List<string> options = new List<string>(Microphone.devices);

        foreach (string device in Microphone.devices)
        {
            Debug.Log("����ũ ��ġ: " + device);

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
        Debug.Log("���õ� ��ġ : " +  selectedDevice);
        OnDeviceSelected?.Invoke(selectedDevice);
    }
}
