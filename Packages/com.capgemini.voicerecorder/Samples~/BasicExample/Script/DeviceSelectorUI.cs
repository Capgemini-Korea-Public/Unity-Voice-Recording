using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyAudioPackage.Core; // 코어 네임스페이스

namespace MyAudioPackage.UI
{
    public class DeviceSelectorUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public TMP_Dropdown deviceDropdown;
        public TextMeshProUGUI deviceError;

        // 코어 로직 인스턴스
        public DeviceManager deviceManagerCore = new DeviceManager();

        private WaitForSeconds deviceCheckDelay = new WaitForSeconds(1f);

        private void Start()
        {
            // 코어 이벤트 구독
            deviceManagerCore.OnDeviceSelected += HandleDeviceSelected;
            deviceManagerCore.OnDeviceListChanged += HandleDeviceListChanged;

            PopulateDropdown();
            StartCoroutine(MonitorDeviceChanges());
        }

        /// <summary>
        /// 드롭다운에 현재 마이크 디바이스 목록을 채웁니다.
        /// </summary>
        private void PopulateDropdown()
        {
            string[] devices = deviceManagerCore.GetAvailableDevices();
            deviceDropdown.ClearOptions();

            if (devices.Length == 0)
            {
                deviceError.text = "No microphone device found.";
                deviceDropdown.interactable = false;
                return;
            }

            List<string> options = new List<string>(devices);
            deviceDropdown.AddOptions(options);
            deviceDropdown.interactable = true;
            deviceError.text = "";

            // 이전에 선택한 장치가 있다면 유지, 없으면 기본값으로 첫 번째 선택
            int index = Array.IndexOf(devices, deviceManagerCore.SelectedDevice);
            if (index < 0)
            {
                deviceManagerCore.SetSelectedDevice(options[0]);
                index = 0;
            }
            deviceDropdown.value = index;

            deviceDropdown.onValueChanged.RemoveAllListeners();
            deviceDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }

        private void OnDropdownChanged(int index)
        {
            string selectedDevice = deviceDropdown.options[index].text;
            deviceManagerCore.SetSelectedDevice(selectedDevice);
        }

        private void HandleDeviceSelected(string device)
        {
            Debug.Log("Selected device: " + device);
            // UI 추가 업데이트가 필요하면 이곳에서 처리 (예: 오류 메시지 초기화 등)
        }

        private void HandleDeviceListChanged()
        {
            PopulateDropdown();
        }

        private IEnumerator MonitorDeviceChanges()
        {
            while (true)
            {
                yield return deviceCheckDelay;
                deviceManagerCore.RefreshDeviceList();
            }
        }
    }
}
