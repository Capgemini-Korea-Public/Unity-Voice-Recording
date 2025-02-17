using UnityEngine.UI;
using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using TMPro;

public enum SaveMode
{
    SaveWav,
    SaveOgg,
    None  // 저장하지 않음
}
public class AudioProcessingManager : MonoBehaviour
{
    public AudioRecorder audioRecorder;
    public AudioFileManager audioFileManager;
    public TMP_Dropdown saveModeDropdown;

    public SaveMode SelectedSaveMode { get; private set; } = SaveMode.None;

    private void Start()
    {
        List<string> options = new List<string> { "Save To Wav", "Save To Ogg", "None" };
        saveModeDropdown.ClearOptions();
        saveModeDropdown.AddOptions(options);
        saveModeDropdown.onValueChanged.AddListener(OnSaveModeChanged);

        // 기본값 설정 (예: 저장 안 함)
        OnSaveModeChanged(saveModeDropdown.value);
    }


    public void OnSaveModeChanged(int index)
    {
        switch (index)
        {
            case 0:
                SelectedSaveMode = SaveMode.SaveWav;
                break;
            case 1:
                SelectedSaveMode = SaveMode.SaveOgg;
                break;
            case 2:
                SelectedSaveMode = SaveMode.None;
                break;
        }
        Debug.Log("선택된 저장 옵션: " + SelectedSaveMode);
    }
}
