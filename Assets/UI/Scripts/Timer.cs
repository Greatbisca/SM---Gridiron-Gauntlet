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
    [SerializeField] float startMessageDuration = 0.9f;
    [SerializeField] float startMessageExitDuration = 0.18f;

    Coroutine timerRoutine;
    Coroutine startMessageRoutine;
    Coroutine finishMessageRoutine;
    AudioSource soundSource;
    Label startMessageShadow;
    Label startMessageText;
    Label finishMessageShadow;
    Label finishMessageText;

    void OnEnable()
    {
        timerText = GetComponent<UIDocument>().rootVisualElement.Q<Label>("timer");
        startMessageShadow = GetComponent<UIDocument>().rootVisualElement.Q<Label>("start-message-shadow");
        startMessageText = GetComponent<UIDocument>().rootVisualElement.Q<Label>("start-message");
        finishMessageShadow = GetComponent<UIDocument>().rootVisualElement.Q<Label>("finish-message-shadow");
        finishMessageText = GetComponent<UIDocument>().rootVisualElement.Q<Label>("finish-message");
        soundSource = GetComponent<AudioSource>();
        if (soundSource == null)
            soundSource = gameObject.AddComponent<AudioSource>();

        soundSource.playOnAwake = false;
        soundSource.volume = 1f;
        soundSource.spatialBlend = 0f;
        UpdateTimerText(0);
        HideStartMessage();
        HideFinishMessage();
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

        if (startMessageRoutine != null)
        {
            StopCoroutine(startMessageRoutine);
            startMessageRoutine = null;
        }
        if (finishMessageRoutine != null)
        {
            StopCoroutine(finishMessageRoutine);
            finishMessageRoutine = null;
        }

        HideStartMessage();
        HideFinishMessage();
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
        ShowFinishMessage();
        timerRoutine = null;
    }

    public void ShowStartMessage()
    {
        if (startMessageText == null) return;

        startMessageText.style.display = DisplayStyle.Flex;
        if (startMessageShadow != null)
            startMessageShadow.style.display = DisplayStyle.Flex;
        startMessageText.style.scale = new Scale(new Vector2(0.72f, 0.72f));
        startMessageText.style.rotate = new Rotate(new Angle(-3f));
        if (startMessageShadow != null)
        {
            startMessageShadow.style.scale = new Scale(new Vector2(0.72f, 0.72f));
            startMessageShadow.style.rotate = new Rotate(new Angle(-3f));
        }
        startMessageRoutine = StartCoroutine(HideStartMessageRoutine());
        StartCoroutine(StartMessageBounceRoutine());
    }

    IEnumerator HideStartMessageRoutine()
    {
        yield return new WaitForSeconds(startMessageDuration);
        yield return StartCoroutine(SquashStartMessageRoutine());
        HideStartMessage();
        startMessageRoutine = null;
    }

    IEnumerator SquashStartMessageRoutine()
    {
        float elapsed = 0f;
        const float startScale = 1f;

        while (elapsed < startMessageExitDuration && startMessageText != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / startMessageExitDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            float scaleX = Mathf.Lerp(startScale, 1.15f, eased);
            float scaleY = Mathf.Lerp(startScale, 0.08f, eased);
            ApplyStartMessageTransform(scaleX, scaleY, Mathf.Lerp(2f, 0f, eased));
            yield return null;
        }
    }

    IEnumerator StartMessageBounceRoutine()
    {
        float elapsed = 0f;
        const float duration = 0.45f;

        while (elapsed < duration && startMessageText != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            float bounce = Mathf.Sin(t * Mathf.PI) * 0.08f;
            float scale = Mathf.Lerp(0.72f, 1f, eased) + bounce;
            float angle = Mathf.Lerp(-3f, 2f, eased);

            startMessageText.style.scale = new Scale(new Vector2(scale, scale));
            startMessageText.style.rotate = new Rotate(new Angle(angle));
            if (startMessageShadow != null)
            {
                startMessageShadow.style.scale = new Scale(new Vector2(scale, scale));
                startMessageShadow.style.rotate = new Rotate(new Angle(angle));
            }

            yield return null;
        }
    }

    void HideStartMessage()
    {
        if (startMessageText != null)
            startMessageText.style.display = DisplayStyle.None;
        if (startMessageShadow != null)
            startMessageShadow.style.display = DisplayStyle.None;
    }

    void ApplyStartMessageTransform(float scaleX, float scaleY, float angle)
    {
        Vector2 scale = new Vector2(scaleX, scaleY);
        startMessageText.style.scale = new Scale(scale);
        startMessageText.style.rotate = new Rotate(new Angle(angle));
        if (startMessageShadow != null)
        {
            startMessageShadow.style.scale = new Scale(scale);
            startMessageShadow.style.rotate = new Rotate(new Angle(angle));
        }
    }

        void ShowFinishMessage()
        {
            if (finishMessageText == null) return;

            finishMessageText.style.display = DisplayStyle.Flex;
            if (finishMessageShadow != null)
                finishMessageShadow.style.display = DisplayStyle.Flex;
            ApplyFinishMessageTransform(0.72f, 0.72f, -3f);
            finishMessageRoutine = StartCoroutine(FinishMessageRoutine());
        }

        IEnumerator FinishMessageRoutine()
        {
            float elapsed = 0f;
            const float entryDuration = 0.45f;
            while (elapsed < entryDuration && finishMessageText != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / entryDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                float scale = Mathf.Lerp(0.72f, 1f, eased) + Mathf.Sin(t * Mathf.PI) * 0.08f;
                ApplyFinishMessageTransform(scale, scale, Mathf.Lerp(-3f, 2f, eased));
                yield return null;
            }

            yield return new WaitForSeconds(startMessageDuration);

            elapsed = 0f;
            while (elapsed < startMessageExitDuration && finishMessageText != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / startMessageExitDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                ApplyFinishMessageTransform(
                    Mathf.Lerp(1f, 1.15f, eased),
                    Mathf.Lerp(1f, 0.08f, eased),
                    Mathf.Lerp(2f, 0f, eased));
                yield return null;
            }

            HideFinishMessage();
            finishMessageRoutine = null;
        }

        void ApplyFinishMessageTransform(float scaleX, float scaleY, float angle)
        {
            Vector2 scale = new Vector2(scaleX, scaleY);
            finishMessageText.style.scale = new Scale(scale);
            finishMessageText.style.rotate = new Rotate(new Angle(angle));
            if (finishMessageShadow != null)
            {
                finishMessageShadow.style.scale = new Scale(scale);
                finishMessageShadow.style.rotate = new Rotate(new Angle(angle));
            }
        }

        void HideFinishMessage()
        {
            if (finishMessageText != null)
                finishMessageText.style.display = DisplayStyle.None;
            if (finishMessageShadow != null)
                finishMessageShadow.style.display = DisplayStyle.None;
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