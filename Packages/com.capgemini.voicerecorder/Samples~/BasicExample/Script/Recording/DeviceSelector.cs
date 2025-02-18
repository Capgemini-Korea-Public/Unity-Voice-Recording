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
    public TextMeshProUGUI deviceError;
    public Button recordButton;
    public Button stopButton;

    public event Action<string> OnDeviceSelected; // 선택된 장치를 다른 컴포넌트에 전달하는 이벤트

    private List<string> cachedDevices = new List<string>();
    private WaitForSeconds deviceCheckDelay = new WaitForSeconds(1f);
    private string selectedDeviceName = null;

    void Start()
    {
        PopulateDeviceDropdown();
        StartCoroutine(MonitorDeviceChanges());
        UpdateButtonStates();
    }

    private void PopulateDeviceDropdown()
    {
        string[] devices = Microphone.devices;
        cachedDevices = new List<string>(devices);

        if (devices.Length == 0)
        {
            Debug.LogWarning("No available microphone devices found.");
            deviceDropdown.ClearOptions();
            deviceError.text = "No microphone device found.";
            selectedDeviceName = null;
            deviceDropdown.interactable = false;
            return;
        }

        List<string> options = new List<string>(devices);

        foreach (string device in options)
        {
            Debug.Log("Microphone device: " + device);
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
            Debug.Log("Retaining previously selected device: " + selectedDeviceName);
        }
        else
        {
            // 이전 선택한 디바이스가 더 이상 존재하지 않는 경우
            if (!string.IsNullOrEmpty(selectedDeviceName))
            {
                Debug.LogWarning("Selected device '" + selectedDeviceName + "' is no longer available.");
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
        Debug.Log("Selected device: " + selectedDeviceName);
        OnDeviceSelected?.Invoke(selectedDeviceName);
    }

    IEnumerator MonitorDeviceChanges()
    {
        while (true)
        {
            yield return deviceCheckDelay;

            string[] currentDevices = Microphone.devices;

            // 만약 현재 연결된 디바이스가 없으면
            if (currentDevices.Length == 0)
            {
                if (deviceDropdown.interactable) // 이전에 연결된 상태였다면
                {
                    Debug.Log("No microphone devices found.");
                    deviceDropdown.ClearOptions();
                    deviceError.text = "No microphone device found.";
                    deviceDropdown.interactable = false;
                    UpdateButtonStates();
                }
            }
            else
            {
                // 기기가 새로 연결되었거나, 목록이 변경된 경우
                if (!deviceDropdown.interactable || HasDeviceListChanged(currentDevices))
                {
                    Debug.Log("Microphone devices are now available or device list changed.");
                    deviceError.text = "";
                    deviceDropdown.interactable = true;
                    PopulateDeviceDropdown();
                    UpdateButtonStates();
                }
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

    private void UpdateButtonStates()
    {
        if (Microphone.devices.Length == 0)
        {
            if (recordButton != null)
                recordButton.interactable = false;
            if (stopButton != null)
                stopButton.interactable = false;
        }
        else
        {
            // 기본적으로 녹음 전에는 Record 버튼만 활성화하고, 녹음 중일 때 Stop 버튼 활성화하도록 처리합니다.
            if (recordButton != null)
                recordButton.interactable = true;
            if (stopButton != null)
                stopButton.interactable = false; // 녹음 상태에 따라 외부에서 변경하도록 처리
        }
    }
}
