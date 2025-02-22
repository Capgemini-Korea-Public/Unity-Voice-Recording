using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyAudioPackage.Core
{
    public class DeviceManager
    {
        // 선택된 장치 변경시 외부에 알리는 이벤트
        public event Action<string> OnDeviceSelected;
        // 디바이스 목록이 변경되었을 때 알리는 이벤트
        public event Action OnDeviceListChanged;

        // 캐시된 디바이스 목록
        private List<string> cachedDevices = new List<string>();
        // 현재 선택된 장치
        public string SelectedDevice { get; private set; } = null;

        /// <summary>
        /// 현재 사용 가능한 마이크 디바이스 목록을 반환합니다.
        /// </summary>
        public string[] GetAvailableDevices()
        {
            return Microphone.devices;
        }

        /// <summary>
        /// 캐시된 디바이스 목록과 새롭게 읽은 목록을 비교하여 변경되었는지 판단합니다.
        /// </summary>
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

        /// <summary>
        /// 디바이스 목록을 새로 고치고, 변경되었으면 이벤트를 발생합니다.
        /// </summary>
        public void RefreshDeviceList()
        {
            string[] currentDevices = GetAvailableDevices();

            // 장치가 없으면 캐시 초기화 후 이벤트 발생
            if (currentDevices.Length == 0)
            {
                cachedDevices.Clear();
                SelectedDevice = null;
                OnDeviceListChanged?.Invoke();
                return;
            }

            // 목록에 변화가 있으면 업데이트
            if (HasDeviceListChanged(currentDevices))
            {
                cachedDevices = new List<string>(currentDevices);
                OnDeviceListChanged?.Invoke();

                // 이전에 선택된 장치가 없거나 더 이상 존재하지 않으면 첫 번째 장치를 선택
                if (string.IsNullOrEmpty(SelectedDevice) || Array.IndexOf(currentDevices, SelectedDevice) < 0)
                {
                    SelectedDevice = currentDevices[0];
                    OnDeviceSelected?.Invoke(SelectedDevice);
                }
            }
        }

        /// <summary>
        /// 외부(UI 래퍼)에서 호출하여 선택한 장치를 저장하고, 이벤트를 발생시킵니다.
        /// </summary>
        public void SetSelectedDevice(string device)
        {
            SelectedDevice = device;
            OnDeviceSelected?.Invoke(device);
        }

        /// <summary>
        /// 현재 캐시된 장치 목록을 반환합니다.
        /// </summary>
        public List<string> GetCachedDevices()
        {
            return new List<string>(cachedDevices);
        }
    }
}
