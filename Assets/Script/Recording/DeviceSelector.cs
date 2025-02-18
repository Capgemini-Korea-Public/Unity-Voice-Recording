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

    public event Action<string> OnDeviceSelected; // ���õ� ��ġ�� �ٸ� ������Ʈ�� �����ϴ� �̺�Ʈ

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
            // ���� ���� ����
            deviceDropdown.value = index;
            Debug.Log("Retaining previously selected device: " + selectedDeviceName);
        }
        else
        {
            // ���� ������ ����̽��� �� �̻� �������� �ʴ� ���
            if (!string.IsNullOrEmpty(selectedDeviceName))
            {
                Debug.LogWarning("Selected device '" + selectedDeviceName + "' is no longer available.");
            }
            // �⺻������ ù ��° �׸� ����
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

            // ���� ���� ����� ����̽��� ������
            if (currentDevices.Length == 0)
            {
                if (deviceDropdown.interactable) // ������ ����� ���¿��ٸ�
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
                // ��Ⱑ ���� ����Ǿ��ų�, ����� ����� ���
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
            // �⺻������ ���� ������ Record ��ư�� Ȱ��ȭ�ϰ�, ���� ���� �� Stop ��ư Ȱ��ȭ�ϵ��� ó���մϴ�.
            if (recordButton != null)
                recordButton.interactable = true;
            if (stopButton != null)
                stopButton.interactable = false; // ���� ���¿� ���� �ܺο��� �����ϵ��� ó��
        }
    }
}
