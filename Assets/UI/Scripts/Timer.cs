using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

public class Timer : MonoBehaviour
{
    [SerializeField] Label timerText;
    [SerializeField] float gameDuration = 30f;
    [SerializeField] AudioClip warningSound;
    [SerializeField] AudioClip finishSound;
    [SerializeField] AudioVolumeSettings audioSettings;

    Coroutine timerRoutine;
    AudioSource soundSource;

    void OnEnable()
    {
        timerText = GetComponent<UIDocument>().rootVisualElement.Q<Label>("timer");
        soundSource = GetComponent<AudioSource>();
        if (soundSource == null)
            soundSource = gameObject.AddComponent<AudioSource>();

        soundSource.playOnAwake = false;
        soundSource.volume = 1f;
        soundSource.spatialBlend = 0f;
        UpdateTimerText(0);
    }

    public void Begin(float preparationDuration)
    {
        StopTimer();
        timerRoutine = StartCoroutine(TimerRoutine(preparationDuration));
    }

    public void StopTimer()
    {
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }
    }

    IEnumerator TimerRoutine(float preparationDuration)
    {
        float remainingPreparation = preparationDuration;
        while (remainingPreparation > 0f)
        {
            UpdateTimerText(Mathf.CeilToInt(remainingPreparation));
            remainingPreparation -= Time.deltaTime;
            yield return null;
        }

        float elapsedTime = gameDuration;
        int lastWarningSecond = -1;
        while (elapsedTime > 0f)
        {
            int seconds = Mathf.CeilToInt(elapsedTime);
            UpdateTimerText(seconds);

            if (seconds <= 5 && seconds != lastWarningSecond)
            {
                PlaySound(warningSound, audioSettings.warningVolume);
                lastWarningSecond = seconds;
            }

            elapsedTime -= Time.deltaTime;
            yield return null;
        }

        UpdateTimerText(0);
        PlaySound(finishSound, audioSettings.finishVolume);
        timerRoutine = null;
    }

    void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null)
            soundSource.PlayOneShot(clip, volume);
    }

    void UpdateTimerText(int seconds)
    {
        timerText.text = string.Format("{0} ", seconds);
    }
}