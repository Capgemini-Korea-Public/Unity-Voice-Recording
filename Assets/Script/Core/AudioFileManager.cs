using System.IO;
using UnityEngine;

namespace MyAudioPackage.Core
{
    /// <summary>
    /// AudioFileManagerCore는 WAV 파일 저장 및 OGG 변환 기능을 순수 C#으로 구현한 클래스입니다.
    /// MonoBehaviour에 의존하지 않고, 생성자에서 파일 경로 등을 설정합니다.
    /// </summary>
    public class AudioFileManager
    {
        /// <summary>
        /// 저장할 WAV 파일이 위치할 폴더 경로입니다.
        /// 기본적으로 Application.persistentDataPath를 사용합니다.
        /// </summary>
        public string WavFolderPath { get; private set; }

        /// <summary>
        /// FFmpeg 실행 파일의 전체 경로입니다.
        /// </summary>
        public string FfmpegPath { get; private set; }

        /// <summary>
        /// FFmpeg 실행 파일의 상대 경로. 필요에 따라 수정할 수 있습니다.
        /// </summary>
        public string FfmpegRelativePath { get; set; } = "Plugin/FFmpeg/bin/ffmpeg.exe";

        /// <summary>
        /// 생성자에서 폴더 경로와 FFmpeg 경로를 설정합니다.
        /// </summary>
        public AudioFileManager()
        {
            // Application.persistentDataPath는 각 플랫폼에 맞는 영구 저장 경로입니다.
            WavFolderPath = Application.persistentDataPath;
            // 저장 폴더가 없으면 생성합니다.
            if (!Directory.Exists(WavFolderPath))
                Directory.CreateDirectory(WavFolderPath);

            // Application.dataPath는 Assets 폴더의 경로를 나타내며, FFmpegRelativePath와 결합하여 FFmpeg 실행 파일의 경로를 만듭니다.
            FfmpegPath = Path.Combine(Application.dataPath, FfmpegRelativePath);
        }

        /// <summary>
        /// AudioClip의 일부(마지막 샘플까지)를 잘라 WAV 파일로 저장합니다.
        /// </summary>
        /// <param name="clip">저장할 AudioClip</param>
        /// <param name="lastSample">실제 녹음된 마지막 샘플 위치</param>
        public void SaveAsWav(AudioClip clip, int lastSample)
        {
            int channels = clip.channels;
            int frequency = clip.frequency;

            // 실제 녹음된 데이터만큼의 샘플 배열을 생성합니다.
            float[] samples = new float[lastSample * channels];
            clip.GetData(samples, 0);

            // 녹음된 데이터만 포함하는 새 AudioClip을 생성합니다.
            AudioClip trimmedClip = AudioClip.Create(clip.name + "_trimmed", lastSample, channels, frequency, false);
            trimmedClip.SetData(samples, 0);

            // 저장할 WAV 파일 경로를 설정합니다.
            string wavFilePath = Path.Combine(WavFolderPath, clip.name + ".wav");
            // WavUtility는 기존에 작성한 WAV 변환 도우미 클래스입니다.
            WavUtility.SaveWavFile(trimmedClip, wavFilePath);
            Debug.Log("WAV file saved: " + wavFilePath);
        }

        /// <summary>
        /// AudioClip을 WAV 파일로 저장한 후, FFmpeg를 사용하여 OGG 파일로 변환합니다.
        /// 변환 후, 원본 WAV 파일은 삭제합니다.
        /// </summary>
        /// <param name="clip">저장할 AudioClip</param>
        /// <param name="lastSample">실제 녹음된 마지막 샘플 위치</param>
        public void SaveAsOgg(AudioClip clip, int lastSample)
        {
            // 먼저 WAV 파일로 저장합니다.
            SaveAsWav(clip, lastSample);

            // 저장한 WAV 파일 경로를 가져옵니다.
            string wavFilePath = Path.Combine(WavFolderPath, clip.name + ".wav");
            Debug.Log("WAV file saved: " + wavFilePath);

            // 변환할 OGG 파일 경로를 설정합니다.
            string oggFilePath = Path.Combine(WavFolderPath, clip.name + ".ogg");
            // FFmpegConverter는 FFmpeg를 사용하여 파일 변환을 수행하는 도우미 클래스입니다.
            FFmpegConverter.ConvertWavToOgg(wavFilePath, oggFilePath, FfmpegPath);

            // 변환 후 원본 WAV 파일 삭제
            if (File.Exists(wavFilePath))
            {
                File.Delete(wavFilePath);
                Debug.Log("Deleted WAV file: " + wavFilePath);
            }
        }
    }
}
