using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using System.Collections;

public class DeviceSelector : MonoBehaviour
{
    public TMP_Dropdown deviceDropdown;

    public event Action<string> OnDeviceSelected; // ���õ� ��ġ�� �ٸ� ������Ʈ�� �����ϴ� �̺�Ʈ

    private List<string> cachedDevices = new List<string>();
    private WaitForSeconds deviceCheckDelay = new WaitForSeconds(1f);

    void Start()
    {
        PopulateDeviceDropdown();
        StartCoroutine(MonitorDeviceChanges());
    }

    private void PopulateDeviceDropdown()
    {
        string[] devices = Microphone.devices;
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("��� ������ ����ũ ����̽��� �����ϴ�.");
            deviceDropdown.ClearOptions();
            cachedDevices = new List<string>();
            return;
        }

        List<string> options = new List<string>(devices);

        foreach (string device in options)
        {
            Debug.Log("����ũ ��ġ: " + device);

        }

        deviceDropdown.onValueChanged.RemoveAllListeners();
        deviceDropdown.ClearOptions();
        deviceDropdown.AddOptions(options);
        deviceDropdown.onValueChanged.AddListener(OnDropdownChanged);

        if (deviceDropdown.options.Count > 0)
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

    IEnumerator MonitorDeviceChanges()
    {
        while(true)
        {
            yield return deviceCheckDelay;

            string[] currentDevices = Microphone.devices;

            if(HasDeviceListChanged(currentDevices))
            {
                Debug.Log("����̽� ��� ����");
                PopulateDeviceDropdown();
            }
        }
    }

    private bool HasDeviceListChanged(string[] newDevices)
    {
        if (newDevices.Length != cachedDevices.Count)
            return true;

        for (int i = 0; i < newDevices.Length; i++)
        {
            if (newDevices[i] != cachedDevices[i])
                return true;
        }
        return false;
    }
}
