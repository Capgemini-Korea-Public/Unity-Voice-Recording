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
    }

}
