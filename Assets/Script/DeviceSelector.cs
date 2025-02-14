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

    public event Action<string> OnDeviceSelected; // 선택된 장치를 다른 컴포넌트에 전달하는 이벤트

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
            Debug.LogWarning("사용 가능한 마이크 디바이스가 없습니다.");
            deviceDropdown.ClearOptions();
            cachedDevices = new List<string>();
            return;
        }

        List<string> options = new List<string>(devices);

        foreach (string device in options)
        {
            Debug.Log("마이크 장치: " + device);

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
        Debug.Log("선택된 장치 : " +  selectedDevice);
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
                Debug.Log("디바이스 목록 변경");
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
