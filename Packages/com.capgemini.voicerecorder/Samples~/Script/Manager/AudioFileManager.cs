using System.IO;
using UnityEngine;

public class AudioFileManager : MonoBehaviour
{
    [Header("File Path")]
    public string wavFolderPath;
    public string ffmpegPath = Path.Combine(Application.dataPath, "Plugin", "FFmpeg", "bin", "ffmpeg.exe");

    void Awake()
    {
        wavFolderPath = Path.Combine(Application.persistentDataPath);
        // 폴더가 존재하지 않으면 생성합니다.
        if (!Directory.Exists(wavFolderPath))
            Directory.CreateDirectory(wavFolderPath);
    }

    // WAV와 OGG 저장 기능을 담당
    public void SaveAsWav(AudioClip clip, int lastSample)
    {
        // 녹음된 클립의 채널 수와 frequency를 가져옵니다.
        int channels = clip.channels;
        int frequency = clip.frequency;

        // 실제 녹음된 데이터만큼의 샘플 배열을 만듭니다.
        float[] samples = new float[lastSample * channels];
        clip.GetData(samples, 0);

        // 실제 녹음된 부분만 담은 새로운 AudioClip 생성
        AudioClip trimmedClip = AudioClip.Create(clip.name + "_trimmed", lastSample, channels, frequency, false);
        trimmedClip.SetData(samples, 0);

        string wavFilePath = Path.Combine(wavFolderPath, clip.name + ".wav");
        WavUtility.SaveWavFile(clip, wavFilePath);
        Debug.Log("WAV file save success: " + wavFilePath);
    }

    public void SaveAsOgg(AudioClip clip, int lastSample)
    {
        SaveAsWav(clip, lastSample);

        string wavFilePath = Path.Combine(wavFolderPath, clip.name + ".wav");
        WavUtility.SaveWavFile(clip, wavFilePath);
        Debug.Log("WAV file save success: " + wavFilePath);

        // OGG 파일은 persistentDataPath를 사용하는 예시
        string oggFilePath = Path.Combine(Application.persistentDataPath, clip.name + ".ogg");
        FFmpegConverter.ConvertWavToOgg(wavFilePath, oggFilePath, ffmpegPath);

        if(File.Exists(wavFilePath))
        {
            File.Delete(wavFilePath);
            Debug.Log("Delete Wav");
        }
    }
}
