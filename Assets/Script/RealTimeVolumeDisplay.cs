using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RealTimeVolumeDisplay : MonoBehaviour
{
    [Header("UI 요소")]
    public Slider volumeSlider;
    public TextMeshProUGUI volumeText;

    private float currentRMS = 0f;
    private readonly object lockObj = new object();

    private const float minDB = -80f;
    private const float maxDB = 0f;
    private const float speakingThreshold = -50f;

    private void OnAudioFilterRead(float[] data, int channels)
    {
        float sum = 0f;
        for(int i = 0; i < data.Length; i++)
        {
            sum += data[i] * data[i];
        }
        float rms = Mathf.Sqrt(sum / data.Length);

        lock (lockObj)
        {
            currentRMS = rms;
        }
    }



    private void Update()
    {
        float rms;

        lock (lockObj)
        {
            rms = currentRMS;
        }

        float db = 20f * Mathf.Log10(rms + 1e-5f);

        float normalizedVolume = Mathf.InverseLerp(minDB, maxDB, db);

        if(volumeSlider != null)
        {
            volumeSlider.value = normalizedVolume;
        }

        if (volumeText != null)
        {
            volumeText.text = (db > speakingThreshold)
                ? $"말하는 중 : {db:F2} dB"
                : "말이 감지되지 않음";
        }
    }
}
