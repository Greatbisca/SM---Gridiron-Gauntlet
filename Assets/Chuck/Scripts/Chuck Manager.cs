using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChuckManager : MonoBehaviour
{
    public List<ChargeHazard> m_Chucks = new List<ChargeHazard>();
    public float m_MinDelay = 1f;
    public float m_MaxDelay = 3f;
    public bool m_AutoStart = true;
    
    public int m_Rounds = 5;
    
    public int m_ChucksPerWave = 1;
    
    public float m_PreparationDuration = 5f;

    public AudioClip m_GameMusic;
    public AudioClip m_StartSound;
    public AudioClip m_StartEndSound;
    public AudioVolumeSettings m_AudioSettings;

    Coroutine m_Routine;
    Timer m_Timer;
    AudioSource m_MusicSource;
    AudioSource m_SfxSource;
    readonly Dictionary<movement, bool> m_PreviousMovementStates = new Dictionary<movement, bool>();

    void Start()
    {
        m_Timer = FindObjectOfType<Timer>();
        m_MusicSource = GetComponent<AudioSource>();
        if (m_MusicSource == null)
            m_MusicSource = gameObject.AddComponent<AudioSource>();

        m_MusicSource.clip = m_GameMusic;
        m_MusicSource.loop = true;
        m_MusicSource.playOnAwake = false;
        m_MusicSource.volume = m_AudioSettings.musicVolume;
        m_MusicSource.spatialBlend = 0f;

        m_SfxSource = gameObject.AddComponent<AudioSource>();
        m_SfxSource.playOnAwake = false;
        m_SfxSource.volume = 1f;
        m_SfxSource.spatialBlend = 0f;

        if (m_AutoStart) StartGame();
    }

    public void StartGame()
    {
        StopGame();
        m_Routine = StartCoroutine(GameRoutine());
    }

    public void StopGame()
    {
        if (m_Routine != null)
        {
            StopCoroutine(m_Routine);
            m_Routine = null;
        }

        if (m_Timer != null)
            m_Timer.StopTimer();

        if (m_MusicSource != null)
            m_MusicSource.Stop();

        RestorePlayerMovement();
    }

    IEnumerator GameRoutine()
    {
        LockPlayerMovement();

        if (m_Timer != null)
            m_Timer.Begin(m_PreparationDuration);

        yield return new WaitForSeconds(m_PreparationDuration);
        PlayStartSound();
        if (m_Timer != null)
            m_Timer.ShowStartMessage();
        PlayGameMusic();
        RestorePlayerMovement();
        StartCoroutine(PlayStartEndSound());

        for (int round = 1; round <= m_Rounds; round++)
        {
            yield return new WaitForSeconds(Random.Range(m_MinDelay, m_MaxDelay));

            foreach (var chuck in PickChucks(m_ChucksPerWave))
                chuck.Charge();
        }

        m_Routine = null;
        Debug.Log("ChuckManager: all rounds done");
    }

    void PlayGameMusic()
    {
        if (m_MusicSource == null || m_GameMusic == null)
        {
            Debug.LogWarning("ChuckManager: game music is not configured.");
            return;
        }

        m_MusicSource.Play();
    }

    void PlayStartSound()
    {
        if (m_StartSound == null)
        {
            Debug.LogWarning("ChuckManager: start sound is not configured.");
            return;
        }

        m_SfxSource.PlayOneShot(m_StartSound, m_AudioSettings.startVolume);
    }

    IEnumerator PlayStartEndSound()
    {
        if (m_StartSound != null)
            yield return new WaitForSeconds(m_StartSound.length);

        if (m_StartEndSound != null)
            m_SfxSource.PlayOneShot(m_StartEndSound, m_AudioSettings.startEndVolume);
    }

    void LockPlayerMovement()
    {
        m_PreviousMovementStates.Clear();

        foreach (var player in FindObjectsOfType<movement>())
        {
            m_PreviousMovementStates[player] = player.disable_move;
            player.disable_move = true;
        }
    }

    void RestorePlayerMovement()
    {
        foreach (var previousState in m_PreviousMovementStates)
        {
            if (previousState.Key != null)
                previousState.Key.disable_move = previousState.Value;
        }

        m_PreviousMovementStates.Clear();
    }

    List<ChargeHazard> PickChucks(int count)
    {
        // Lines that already have a Chuck running
        var busyLines = new HashSet<int>();
        foreach (var c in m_Chucks)
            if (c.IsCharging) busyLines.Add(c.m_LineId);

        // Idle Chucks on free lines, in random order
        var candidates = new List<ChargeHazard>();
        foreach (var c in m_Chucks)
            if (!c.IsCharging && !busyLines.Contains(c.m_LineId))
                candidates.Add(c);
        Shuffle(candidates);

        // Take Chucks until we have enough, never two from the same line
        var picked = new List<ChargeHazard>();
        var usedLines = new HashSet<int>();
        foreach (var c in candidates)
        {
            if (picked.Count >= count) break;
            if (!usedLines.Add(c.m_LineId)) continue;
            picked.Add(c);
        }
        return picked;
    }

    static void Shuffle(List<ChargeHazard> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}