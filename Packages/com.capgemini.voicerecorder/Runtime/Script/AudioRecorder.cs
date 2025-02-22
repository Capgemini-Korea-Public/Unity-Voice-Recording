using System;
using System.Threading.Tasks;
using UnityEngine;

namespace MyAudioPackage.Core
{
    public class AudioRecorder
    {
        // 이벤트: 외부(UI 래퍼)에서 구독하여 상태 변화에 따른 UI 업데이트 가능
        public event Action OnRecordingStarted;
        public event Action OnRecordingStopped;
        public event Action<string> OnLog;

        public static AudioSource CurrentRecordingSource { get; private set; }

        // 녹음 설정
        public int RecordingDuration { get; private set; } = 10; // 기본 10초, 1~30초 범위
        public int Frequency { get; private set; } = 44100;
        public bool IsRecording { get; private set; } = false;
        public AudioClip RecordedClip { get; private set; } = null;
        public int LastSample { get; private set; } = 0;

        // 외부에서 주입받을 AudioSource (분석용, 재생용)
        public AudioSource AnalysisSource { get; private set; }
        public AudioSource PlaybackSource { get; private set; }
        public SaveMode RecordingSaveMode { get; set; } = SaveMode.SaveWav;

        // 내부적으로 사용할 마이크 장치
        private string selectedDevice = null;
        private string microphoneDevice = null;

        public AudioRecorder(AudioSource analysisSource, AudioSource playbackSource)
        {
            AnalysisSource = analysisSource;
            PlaybackSource = playbackSource;
        }
        /// <summary>
        /// 마이크 장치를 설정합니다.
        /// </summary>
        public void SetDevice(string deviceName)
        {
            selectedDevice = deviceName;
            Log($"Device set to: {selectedDevice}");
        }


        /// <summary>
        /// 녹음 시간을 설정합니다. (1 ~ 30초)
        /// </summary>
        public void SetRecordingDuration(int duration)
        {
            RecordingDuration = Mathf.Clamp(duration, 1, 30);
            Log($"Recording duration set to {RecordingDuration} seconds.");
        }

        /// <summary>
        /// 녹음을 시작합니다. (비동기 방식)
        /// </summary>
        public async Task StartRecordingAsync()
        {
            if (IsRecording)
            {
                Log("Recording is already in progress.");
                return;
            }

            // 마이크 장치가 선택되지 않은 경우 기본 장치를 사용
            if (string.IsNullOrEmpty(selectedDevice))
            {
                string[] devices = Microphone.devices;
                if (devices.Length > 0)
                {
                    selectedDevice = devices[0];
                    Log($"Automatically selected default microphone device: {selectedDevice}");
                }
                else
                {
                    Log("No microphone devices found.");
                    return;
                }
            }

            microphoneDevice = selectedDevice;

            // Microphone.Start: (장치, loop, 최대 녹음 시간, 샘플레이트)
            RecordedClip = Microphone.Start(microphoneDevice, true, RecordingDuration, Frequency);
            Log("Recording started.");

            // 분석용 AudioSource 설정: (있다면)
            if (AnalysisSource != null)
            {
                AnalysisSource.clip = RecordedClip;
                AnalysisSource.loop = true;
                AnalysisSource.mute = true;
                AnalysisSource.Play();
            }

            CurrentRecordingSource = AnalysisSource;

            // 실제 녹음 시작까지 대기 (Microphone.GetPosition > 0)
            while (Microphone.GetPosition(microphoneDevice) <= 0)
            {
                await Task.Yield();
            }

            IsRecording = true;
            OnRecordingStarted?.Invoke();

            // 녹음 지속 시간만큼 대기 후 자동 중지
            await Task.Delay(RecordingDuration * 1000);
            StopRecording();
        }

        /// <summary>
        /// 녹음을 중지합니다.
        /// </summary>
        public void StopRecording()
        {
            if (!IsRecording)
            {
                Log("Not currently recording.");
                return;
            }

            LastSample = Microphone.GetPosition(microphoneDevice);
            Microphone.End(microphoneDevice);
            IsRecording = false;
            Log("Recording stopped.");

            // 분석용 AudioSource 정지
            if (AnalysisSource != null && AnalysisSource.isPlaying)
                AnalysisSource.Stop();

            // 재생용 AudioSource에 녹음 클립 할당 후 재생 (있다면)
            if (PlaybackSource != null && RecordedClip != null)
            {
                PlaybackSource.clip = RecordedClip;
                PlaybackSource.loop = false;
                PlaybackSource.Play();
            }

            OnRecordingStopped?.Invoke();

            if (RecordingSaveMode != SaveMode.None && RecordedClip != null)
            {
                SaveRecording(RecordingSaveMode);
            }
        }

        /// <summary>
        /// 녹음된 클립을 재생합니다.
        /// </summary>
        public void PlayRecording()
        {
            if (RecordedClip == null)
            {
                Log("No recorded clip available for playback.");
                return;
            }

            if (PlaybackSource != null)
            {
                PlaybackSource.clip = RecordedClip;
                PlaybackSource.loop = false;
                PlaybackSource.Play();
                Log("Playback started.");
            }
            else
            {
                Log("Playback source is not set.");
            }
        }

        /// <summary>
        /// SaveMode에 따라 녹음된 AudioClip을 파일로 저장합니다.
        /// </summary>
        public void SaveRecording(SaveMode mode)
        {
            // AudioFileManagerCore는 파일 저장 및 변환 기능을 담당하는 순수 C# 클래스입니다.
            AudioFileManager fileManager = new AudioFileManager();

            switch (mode)
            {
                case SaveMode.SaveWav:
                    fileManager.SaveAsWav(RecordedClip, LastSample);
                    break;
                case SaveMode.SaveOgg:
                    fileManager.SaveAsOgg(RecordedClip, LastSample);
                    break;
                case SaveMode.None:
                default:
                    Log("No save operation performed (SaveMode.None).");
                    break;
            }
        }

        /// <summary>
        /// 로그 메시지를 이벤트로 전달하고 콘솔에도 출력합니다.
        /// </summary>
        private void Log(string message)
        {
            OnLog?.Invoke(message);
            Debug.Log(message);
        }
    }
}
