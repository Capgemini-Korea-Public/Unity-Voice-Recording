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
    private string selectedDeviceName = null;

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
            selectedDeviceName = null;
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

        int index = options.IndexOf(selectedDeviceName);
        if (index >= 0)
        {
            // 이전 선택 유지
            deviceDropdown.value = index;
            Debug.Log("이전 선택된 장치 유지: " + selectedDeviceName);
        }
        else
        {
            // 이전 선택한 디바이스가 더 이상 존재하지 않는 경우
            if (!string.IsNullOrEmpty(selectedDeviceName))
            {
                Debug.LogWarning("선택된 장치 '" + selectedDeviceName + "' 가 더 이상 사용 불가능 합니다.");
            }
            // 기본값으로 첫 번째 항목 선택
            deviceDropdown.value = 0;
            selectedDeviceName = options[0];
        }

        deviceDropdown.onValueChanged.AddListener(OnDropdownChanged);
        OnDropdownChanged(deviceDropdown.value);
    }

    private void OnDropdownChanged(int value)
    {
        selectedDeviceName = deviceDropdown.options[value].text;
        Debug.Log("선택된 장치: " + selectedDeviceName);
        OnDeviceSelected?.Invoke(selectedDeviceName);
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
