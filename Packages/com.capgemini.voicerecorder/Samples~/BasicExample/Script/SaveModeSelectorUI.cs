using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MyAudioPackage.Core;

namespace MyAudioPackage.UI
{
    public class SaveModeSelectorUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public TMP_Dropdown saveModeDropdown;

        // 코어 저장 모드 선택 인스턴스 (외부에서 사용하거나 AudioManager 등에서 참조 가능)
        public SaveModeSelector saveModeSelectorCore = new SaveModeSelector();

        private void Start()
        {
            // 드롭다운 옵션 설정
            List<string> options = new List<string> { "Save To Wav", "Save To Ogg", "None" };
            saveModeDropdown.ClearOptions();
            saveModeDropdown.AddOptions(options);

            // 기본값 설정: 예를 들어 "None"을 기본값으로 사용 (인덱스 2)
            saveModeDropdown.value = 0;
            saveModeSelectorCore.SetSaveModeFromIndex(saveModeDropdown.value);

            // 드롭다운 값 변경 이벤트에 리스너 추가
            saveModeDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }

        private void OnDropdownChanged(int index)
        {
            saveModeSelectorCore.SetSaveModeFromIndex(index);
            Debug.Log("Selected Save Mode: " + saveModeSelectorCore.SelectedSaveMode);
        }
    }
}
