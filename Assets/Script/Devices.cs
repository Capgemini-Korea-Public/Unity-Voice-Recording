using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Devices : MonoBehaviour
{
    public TMP_Dropdown deviceDropdown;

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
    }

}
