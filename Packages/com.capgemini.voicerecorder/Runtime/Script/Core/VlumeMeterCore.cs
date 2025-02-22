using System;
using UnityEngine;

namespace MyAudioPackage.Core
{
    /// <summary>
    /// VolumeMeterCore는 AudioClip의 최근 샘플들을 분석하여 현재 볼륨(dB)을 계산하는 순수 C# 클래스입니다.
    /// 이 클래스는 UI에 종속되지 않으며, 볼륨 값을 계산해 반환합니다.
    /// </summary>
    public class VolumeMeter
    {
        /// <summary>
        /// 한 번에 처리할 최대 샘플 수 (기본값: 1024)
        /// </summary>
        public int SampleWindow { get; set; } = 1024;

        // 마지막으로 처리한 샘플 위치
        private int lastSamplePos = 0;

        // 분석할 AudioClip (녹음된 클립)
        public AudioClip Clip { get; set; }

        /// <summary>
        /// 생성자: AudioClip과 (선택적으로) SampleWindow를 설정합니다.
        /// </summary>
        /// <param name="clip">분석할 AudioClip</param>
        /// <param name="sampleWindow">한 번에 처리할 샘플 수</param>
        public VolumeMeter(AudioClip clip, int sampleWindow = 1024)
        {
            Clip = clip;
            SampleWindow = sampleWindow;
            lastSamplePos = 0;
        }

        /// <summary>
        /// AudioClip의 최근 샘플들을 분석하여 dB 값을 계산합니다.
        /// 이 메서드는 Microphone.GetPosition을 호출하여 현재 샘플 위치를 기준으로 새 샘플을 처리합니다.
        /// </summary>
        /// <returns>계산된 볼륨 (dB 단위)</returns>
        public float UpdateVolume()
        {
            if (Clip == null)
            {
                Debug.LogWarning("VolumeMeterCore: AudioClip is null.");
                return -80f;
            }

            int currentPos = Microphone.GetPosition(null);
            if (currentPos < 0)
            {
                return -80f;
            }

            int newSamples = 0;
            if (currentPos >= lastSamplePos)
                newSamples = currentPos - lastSamplePos;
            else
                newSamples = (Clip.samples - lastSamplePos) + currentPos;

            newSamples = Mathf.Max(newSamples, 0);
            newSamples = Mathf.Min(newSamples, SampleWindow);

            float dB = -80f;
            if (newSamples > 0)
            {
                float[] samples = new float[newSamples * Clip.channels];

                if (lastSamplePos + newSamples <= Clip.samples)
                {
                    Clip.GetData(samples, lastSamplePos);
                }
                else
                {
                    int firstPart = Clip.samples - lastSamplePos;
                    float[] samplesPart1 = new float[firstPart * Clip.channels];
                    float[] samplesPart2 = new float[(newSamples - firstPart) * Clip.channels];
                    Clip.GetData(samplesPart1, lastSamplePos);
                    Clip.GetData(samplesPart2, 0);
                    samplesPart1.CopyTo(samples, 0);
                    samplesPart2.CopyTo(samples, firstPart * Clip.channels);
                }

                // RMS 계산
                float sum = 0f;
                foreach (float sample in samples)
                {
                    sum += sample * sample;
                }
                float rms = Mathf.Sqrt(sum / samples.Length);
                dB = 20f * Mathf.Log10(rms + 1e-6f);
            }

            lastSamplePos = currentPos;
            return dB;
        }
    }
}
