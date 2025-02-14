using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Unity.Hierarchy;

public class CustomVolumeMeter : MonoBehaviour
{
    [Header("UI")]
    public Slider volumeSlider;
    public TextMeshProUGUI volumeText;

    public AudioSource recordingSource;

    public int SampleWindow = 1024;

    private int lastSamplePos = 0;

    private WaitForSeconds volumeMeterCheckTime = new WaitForSeconds(0.05f);

    private void Start()
    {
        if(recordingSource == null)
        {
            Debug.LogError("녹음용 AudioSource가 할당되지 않았습니다.");
            return;
        }

        StartCoroutine(UpdateVolumeMeter());
    }

    IEnumerator UpdateVolumeMeter()
    {
        while(true)
        {
            int currentPos = Microphone.GetPosition(null);
            if (currentPos < 0)
            {
                yield return volumeMeterCheckTime;
                continue;
            }

            int newSamples;

            if (currentPos >= lastSamplePos)
                newSamples = currentPos - lastSamplePos;
            else
                newSamples = (recordingSource.clip.samples - lastSamplePos) + currentPos;
            
            newSamples = Mathf.Min(newSamples, SampleWindow);

            if (newSamples > 0)
            {
                float[] samples = new float[newSamples * recordingSource.clip.channels];

                if (lastSamplePos + newSamples <= recordingSource.clip.samples)
                {
                    recordingSource.clip.GetData(samples, lastSamplePos);
                }
                else
                {
                    int firstPart = recordingSource.clip.samples - lastSamplePos;
                    float[] samplesPart1 = new float[firstPart * recordingSource.clip.channels];
                    float[] samplesPart2 = new float[(newSamples - firstPart) * recordingSource.clip.channels];
                    recordingSource.clip.GetData(samplesPart1, lastSamplePos);
                    recordingSource.clip.GetData(samplesPart2, 0);
                    samplesPart1.CopyTo(samples, 0);
                    samplesPart2.CopyTo(samples, firstPart * recordingSource.clip.channels);
                }

                //RMS

                float sum = 0f;
                foreach (float sample in samples)
                {
                    sum += sample * sample;
                }
                float rms = Mathf.Sqrt(sum / samples.Length);
                float dB = 20f * Mathf.Log(rms + 1e-6f);

                if(volumeSlider  != null)
                {
                    float normalizedVolume = Mathf.InverseLerp(-80f, 0f, dB);
                    volumeSlider.value = normalizedVolume;
                }
                if(volumeText != null)
                {
                    volumeText.text = $"Volume : {dB:F2} dB";
                }             
            }

            lastSamplePos = currentPos;

            yield return volumeMeterCheckTime; // 50ms
        }
        
    }
}