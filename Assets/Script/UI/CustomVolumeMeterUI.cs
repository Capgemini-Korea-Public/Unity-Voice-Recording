using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyAudioPackage.Core; // 코어 네임스페이스

namespace MyAudioPackage.UI
{
    /// <summary>
    /// CustomVolumeMeterUI는 녹음이 시작되어 AudioSource가 전달되면
    /// 해당 AudioSource의 AudioClip을 분석해 볼륨(dB)을 UI(Slider, Text)로 업데이트합니다.
    /// Inspector에서는 AudioSource를 수동 할당하지 않습니다.
    /// </summary>
    public class CustomVolumeMeterUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public Slider volumeSlider;
        public TextMeshProUGUI volumeText;

        [Header("Settings")]
        public int sampleWindow = 1024;

        // 동적으로 할당될 녹음용 AudioSource (Inspector에 노출하지 않음)
        private AudioSource recordingSource;

        // 코어 볼륨 미터 인스턴스
        private VolumeMeter volumeMeterCore;

        private WaitForSeconds checkDelay = new WaitForSeconds(0.05f);

        private void Start()
        {
            // 볼륨 업데이트 코루틴 시작
            StartCoroutine(UpdateVolumeRoutine());
        }

        /// <summary>
        /// AudioRecorderUI에서 녹음 시작 시 호출되어 AudioSource를 전달받습니다.
        /// </summary>
        /// <param name="source">녹음에 사용되는 AudioSource</param>

        private IEnumerator UpdateVolumeRoutine()
        {
            while (true)
            {
                // 만약 아직 로컬 recordingSource가 null이면, AudioRecorderCore의 정적 필드를 확인
                if (recordingSource == null)
                {
                    recordingSource = AudioRecorder.CurrentRecordingSource;
                    // 녹음이 시작되어 clip이 할당되었으면 코어 인스턴스 초기화
                    if (recordingSource != null && recordingSource.clip != null && volumeMeterCore == null)
                    {
                        volumeMeterCore = new VolumeMeter(recordingSource.clip, sampleWindow);
                    }
                }

                if (recordingSource != null && recordingSource.clip != null && volumeMeterCore != null)
                {
                    float dB = volumeMeterCore.UpdateVolume();
                    float normalizedVolume = Mathf.InverseLerp(-80f, 0f, dB);
                    if (volumeSlider != null)
                        volumeSlider.value = normalizedVolume;
                    if (volumeText != null)
                        volumeText.text = $"Volume: {dB:F2} dB";
                }
                yield return checkDelay;
            }
        }
    }
}

