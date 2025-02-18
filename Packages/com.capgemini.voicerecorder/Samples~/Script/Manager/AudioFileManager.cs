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
        // ������ �������� ������ �����մϴ�.
        if (!Directory.Exists(wavFolderPath))
            Directory.CreateDirectory(wavFolderPath);
    }

    // WAV�� OGG ���� ����� ���
    public void SaveAsWav(AudioClip clip, int lastSample)
    {
        // ������ Ŭ���� ä�� ���� frequency�� �����ɴϴ�.
        int channels = clip.channels;
        int frequency = clip.frequency;

        // ���� ������ �����͸�ŭ�� ���� �迭�� ����ϴ�.
        float[] samples = new float[lastSample * channels];
        clip.GetData(samples, 0);

        // ���� ������ �κи� ���� ���ο� AudioClip ����
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

        // OGG ������ persistentDataPath�� ����ϴ� ����
        string oggFilePath = Path.Combine(Application.persistentDataPath, clip.name + ".ogg");
        FFmpegConverter.ConvertWavToOgg(wavFilePath, oggFilePath, ffmpegPath);

        if(File.Exists(wavFilePath))
        {
            File.Delete(wavFilePath);
            Debug.Log("Delete Wav");
        }
    }
}
