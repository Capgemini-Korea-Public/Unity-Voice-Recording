using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    public static void SaveWavFile(AudioClip clip, string filePath)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioClip is null");
            return;
        }

        // AudioClip에서 PCM 데이터 추출
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        // WAV 파일 형식의 바이트 배열로 변환
        byte[] wavData = ConvertToWav(samples, clip.channels, clip.frequency);

        // 파일로 저장
        File.WriteAllBytes(filePath, wavData);
        Debug.Log("WAV file is Saved : " + filePath);
    }

    public static byte[] ConvertToWav(float[] samples, int channels, int frequency)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            int sampleCount = samples.Length;
            int byteRate = frequency * channels * 2; // 16비트 PCM: 2바이트

            // RIFF 헤더
            stream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
            stream.Write(BitConverter.GetBytes(36 + sampleCount * 2), 0, 4);
            stream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);

            // fmt 서브청크
            stream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
            stream.Write(BitConverter.GetBytes(16), 0, 4);             // 서브청크 크기
            stream.Write(BitConverter.GetBytes((short)1), 0, 2);         // 오디오 포맷 (1 = PCM)
            stream.Write(BitConverter.GetBytes((short)channels), 0, 2);    // 채널 수
            stream.Write(BitConverter.GetBytes(frequency), 0, 4);          // 샘플링 레이트
            stream.Write(BitConverter.GetBytes(byteRate), 0, 4);           // 바이트레이트
            stream.Write(BitConverter.GetBytes((short)(channels * 2)), 0, 2); // 블록 정렬 (BlockAlign)
            stream.Write(BitConverter.GetBytes((short)16), 0, 2);          // 샘플당 비트 수

            // data 서브청크
            stream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
            stream.Write(BitConverter.GetBytes(sampleCount * 2), 0, 4);

            // PCM 데이터: float를 16비트 PCM으로 변환하여 기록
            for (int i = 0; i < sampleCount; i++)
            {
                // -1 ~ 1 사이의 float 값을 short 범위로 변환
                short intData = (short)(Mathf.Clamp(samples[i], -1f, 1f) * short.MaxValue);
                stream.Write(BitConverter.GetBytes(intData), 0, 2);
            }

            return stream.ToArray();
        }
    }
}
