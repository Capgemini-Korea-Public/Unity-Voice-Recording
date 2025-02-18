using System.IO;
using UnityEngine;

public static class WavByteUtility
{
    public static byte[] ConvertAudioClipToWavBytes(AudioClip clip)
    {
        // 1. AudioClip에서 float 샘플 데이터 추출
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        // 2. float 배열을 16비트 PCM 데이터로 변환 (little-endian)
        int sampleCount = samples.Length;
        byte[] pcmData = new byte[sampleCount * 2]; // 2바이트 per sample
        for (int i = 0; i < sampleCount; i++)
        {
            short intData = (short)(Mathf.Clamp(samples[i], -1f, 1f) * short.MaxValue);
            byte[] byteArr = System.BitConverter.GetBytes(intData);
            // BitConverter returns little-endian on Windows.
            byteArr.CopyTo(pcmData, i * 2);
        }

        // 3. WAV 헤더 생성
        int sampleRate = clip.frequency;
        int channels = clip.channels;
        int byteRate = sampleRate * channels * 2; // 16비트 오디오 -> 2바이트
        int dataSize = pcmData.Length;
        int fileSize = 36 + dataSize;
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            // RIFF 헤더
            writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"));
            writer.Write(fileSize);
            writer.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"));

            // fmt 서브청크
            writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt "));
            writer.Write(16); // 서브청크 사이즈
            writer.Write((short)1); // 오디오 포맷 (1 = PCM)
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write((short)(channels * 2)); // Block Align
            writer.Write((short)16); // Bits per sample

            // data 서브청크
            writer.Write(System.Text.Encoding.UTF8.GetBytes("data"));
            writer.Write(dataSize);

            // PCM 데이터 기록
            writer.Write(pcmData);

            return stream.ToArray();
        }
    }
}
