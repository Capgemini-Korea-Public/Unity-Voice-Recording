using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyAudioPackage.Core; // 코어 클래스 네임스페이스

namespace MyAudioPackage.UI
{
    public class AudioRecorderUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public Button startButton;
        public Button stopButton;
        public Button playButton;
        public TMP_InputField durationInputField;
        public TextMeshProUGUI logText;
        public TextMeshProUGUI maxRecordingDurationText;

        public AudioSource analysisSource;
        public AudioSource playbackSource;

        // 코어 로직 인스턴스: MonoBehaviour에 의존하지 않는 순수 C# 클래스
        private AudioRecorder recorderCore;
        private AudioFileManager fileManagerCore;

        private void Awake()
        {
            recorderCore = new AudioRecorder(analysisSource, playbackSource);
            fileManagerCore = new AudioFileManager();

            // UI 버튼 이벤트 연결
            if (startButton != null)
                startButton.onClick.AddListener(OnStartButtonClicked);
            if (stopButton != null)
                stopButton.onClick.AddListener(OnStopButtonClicked);
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayButtonClicked);

            // 입력 필드 변경 시 녹음 시간 업데이트
            if (durationInputField != null)
                durationInputField.onValueChanged.AddListener(OnDurationInputChanged);

            // 코어 이벤트 구독: 녹음 시작/중지 및 로그 메시지 전달
            recorderCore.OnRecordingStarted += HandleRecordingStarted;
            recorderCore.OnRecordingStopped += HandleRecordingStopped;
            recorderCore.OnLog += AppendLog;
        }

        private async void OnStartButtonClicked()
        {
            // 입력된 녹음 시간이 있으면 코어에 반영
            if (int.TryParse(durationInputField.text, out int duration))
            {
                recorderCore.SetRecordingDuration(duration);
            }
            // 녹음 시작 (비동기 호출)
            await recorderCore.StartRecordingAsync();
        }

        private void OnStopButtonClicked()
        {
            recorderCore.StopRecording();
        }

        private void OnPlayButtonClicked()
        {
            recorderCore.PlayRecording();
        }

        private void OnDurationInputChanged(string value)
        {
            if (int.TryParse(value, out int duration))
            {
                recorderCore.SetRecordingDuration(duration);
                maxRecordingDurationText.text = $"Max Recording Duration: {duration} seconds";
            }
        }

        private void HandleRecordingStarted()
        {
            AppendLog("Recording started.");
            // UI 업데이트: 예를 들어, 버튼 상태 변경 등 추가 가능
        }

        private void HandleRecordingStopped()
        {
            AppendLog("Recording stopped.");
            // UI 업데이트: 예를 들어, 버튼 상태 변경 등 추가 가능
        }

        private void AppendLog(string message)
        {
            if (logText != null)
                logText.text += message + "\n";
            Debug.Log(message);
        }
    }
}
