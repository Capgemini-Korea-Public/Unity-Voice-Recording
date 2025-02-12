using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
    private WaitForSeconds wait = new WaitForSeconds(0.05f); // 50ms 간격
    private int debugCallCount = 0;

    private void OnAudioFilterRead(float[] data, int channels)
    {
        debugCallCount++;
        // 500번마다 로그를 남겨서 호출 여부를 확인
        if (debugCallCount % 500 == 0)
        {
            Debug.Log("OnAudioFilterRead 호출됨: data.Length = " + data.Length);
        }

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

    void OnEnable()
    {
        StartCoroutine(UpdateVolumeDisplay());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator UpdateVolumeDisplay()
    {
        while (true)
        {
            float rms;
            lock (lockObj)
            {
                rms = currentRMS;
            }
            float db = 20f * Mathf.Log10(rms + 1e-5f);
            float normalizedVolume = Mathf.InverseLerp(minDB, maxDB, db);

            if (volumeSlider != null)
                volumeSlider.value = normalizedVolume;

            if (volumeText != null)
            {
                volumeText.text = (db > speakingThreshold)
                    ? $"Talking : {db:F2} dB"
                    : "Words not detected";
            }
            yield return wait;
        }
    }
}
