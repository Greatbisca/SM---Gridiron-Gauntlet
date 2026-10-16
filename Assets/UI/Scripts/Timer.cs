
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
    [SerializeField] CameraIntro cameraIntro;

    // Added: player references and starting positions
    [SerializeField] Transform player1;
    [SerializeField] Transform player2;
    [SerializeField] Transform player1Start;
    [SerializeField] Transform player2Start;

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
		LockPlayers();
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

	void LockPlayers()
	{
    	if (player1 != null)
    	{
        	movement m = player1.GetComponent<movement>();
        	if (m != null) m.disable_move = true;
    	}

    	if (player2 != null)
    	{
        	movement m = player2.GetComponent<movement>();
        	if (m != null) m.disable_move = true;
    	}
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

        // Added: move players after the finish text disappears
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(ReturnPlayers());

        // Move camera after both players return
        if (cameraIntro != null)
            cameraIntro.MoveToEnd();
    }

    // Added: smoothly return both players over 2 seconds
    IEnumerator ReturnPlayers()
{
    const float duration = 2f;

    bool move1 = player1 != null && player1Start != null;
    bool move2 = player2 != null && player2Start != null;

    if (!move1 && !move2)
        yield break;

    Rigidbody rb1 = move1 ? player1.GetComponent<Rigidbody>() : null;
    Rigidbody rb2 = move2 ? player2.GetComponent<Rigidbody>() : null;

    Vector3 p1StartPos = move1 ? player1.position : Vector3.zero;
    Vector3 p2StartPos = move2 ? player2.position : Vector3.zero;

    Quaternion p1StartRot = move1 ? player1.rotation : Quaternion.identity;
    Quaternion p2StartRot = move2 ? player2.rotation : Quaternion.identity;

    Quaternion p1EndRot = move1
        ? player1Start.rotation * Quaternion.Euler(0f, 180f, 0f)
        : Quaternion.identity;

    Quaternion p2EndRot = move2
        ? player2Start.rotation * Quaternion.Euler(0f, 180f, 0f)
        : Quaternion.identity;

    if (rb1 != null) rb1.isKinematic = true;
    if (rb2 != null) rb2.isKinematic = true;

    float elapsed = 0f;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        if (move1 && player1 != null)
        {
            player1.SetPositionAndRotation(
                Vector3.Lerp(p1StartPos, player1Start.position, t),
                Quaternion.Slerp(p1StartRot, p1EndRot, t));
        }

        if (move2 && player2 != null)
        {
            player2.SetPositionAndRotation(
                Vector3.Lerp(p2StartPos, player2Start.position, t),
                Quaternion.Slerp(p2StartRot, p2EndRot, t));
        }

        yield return null;
    }

    if (rb1 != null)
    {
        rb1.isKinematic = false;
        rb1.linearVelocity = Vector3.zero;
        rb1.angularVelocity = Vector3.zero;
    }

    if (rb2 != null)
    {
        rb2.isKinematic = false;
        rb2.linearVelocity = Vector3.zero;
        rb2.angularVelocity = Vector3.zero;
    }
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
