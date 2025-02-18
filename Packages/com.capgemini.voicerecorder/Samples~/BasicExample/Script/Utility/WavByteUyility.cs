using System.IO;
using UnityEngine;

public static class WavByteUtility
{
    public static byte[] ConvertAudioClipToWavBytes(AudioClip clip)
    {
        // 1. AudioClip���� float ���� ������ ����
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        // 2. float �迭�� 16��Ʈ PCM �����ͷ� ��ȯ (little-endian)
        int sampleCount = samples.Length;
        byte[] pcmData = new byte[sampleCount * 2]; // 2����Ʈ per sample
        for (int i = 0; i < sampleCount; i++)
        {
            short intData = (short)(Mathf.Clamp(samples[i], -1f, 1f) * short.MaxValue);
            byte[] byteArr = System.BitConverter.GetBytes(intData);
            // BitConverter returns little-endian on Windows.
            byteArr.CopyTo(pcmData, i * 2);
        }

        // 3. WAV ��� ����
        int sampleRate = clip.frequency;
        int channels = clip.channels;
        int byteRate = sampleRate * channels * 2; // 16��Ʈ ����� -> 2����Ʈ
        int dataSize = pcmData.Length;
        int fileSize = 36 + dataSize;
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            // RIFF ���
            writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"));
            writer.Write(fileSize);
            writer.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"));

            // fmt ����ûũ
            writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt "));
            writer.Write(16); // ����ûũ ������
            writer.Write((short)1); // ����� ���� (1 = PCM)
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write((short)(channels * 2)); // Block Align
            writer.Write((short)16); // Bits per sample

            // data ����ûũ
            writer.Write(System.Text.Encoding.UTF8.GetBytes("data"));
            writer.Write(dataSize);

            // PCM ������ ���
            writer.Write(pcmData);

            return stream.ToArray();
        }
    }
}
