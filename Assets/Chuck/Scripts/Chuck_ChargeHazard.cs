using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ChargeHazard : MonoBehaviour
{
    public int m_LineId;
    public float m_Speed = 3;
    public float m_Distance = 15;
    public float m_StopTime = 1;

    [Header("Mid-run pause")]
    public float m_CellSize = 1;
    public int m_PauseAfterCells = 3;
    public float m_PauseTime = 1;

    [Header("Crossing check")]
    [Tooltip("If two Chucks would get closer than this, one waits")]
    public float m_SafeDistance = 1.5f;

    [Header("Preview Audio")]
    public AudioClip m_PreviewSound;
    public AudioClip m_ResumeSound;
    public AudioClip m_FinalChargeLoopSound;
    public AudioVolumeSettings m_AudioSettings;

    public bool IsCharging { get; private set; }

    static readonly List<ChargeHazard> s_All = new List<ChargeHazard>();

    Rigidbody m_Rb;
    AudioSource m_AudioSource;
    AudioSource m_FinalChargeAudioSource;
    Vector3 m_StartPosition;
    Vector3 m_Dir;
    Vector3 m_RunStart;
    bool m_Waiting;
    bool m_Committed;

    void Awake()
    {
        m_Rb = GetComponent<Rigidbody>();
        m_Rb.freezeRotation = true;
        m_StartPosition = m_Rb.position;

        m_AudioSource = GetComponent<AudioSource>();
        if (m_AudioSource == null)
            m_AudioSource = gameObject.AddComponent<AudioSource>();

        m_AudioSource.playOnAwake = false;
        m_AudioSource.spatialBlend = 1f;
        m_AudioSource.rolloffMode = AudioRolloffMode.Linear;
        m_AudioSource.minDistance = 5f;
        m_AudioSource.maxDistance = 35f;
        m_AudioSource.dopplerLevel = 0f;

        m_FinalChargeAudioSource = gameObject.AddComponent<AudioSource>();
        m_FinalChargeAudioSource.playOnAwake = false;
        m_FinalChargeAudioSource.loop = true;
        m_FinalChargeAudioSource.spatialBlend = 1f;
        m_FinalChargeAudioSource.rolloffMode = AudioRolloffMode.Linear;
        m_FinalChargeAudioSource.minDistance = 5f;
        m_FinalChargeAudioSource.maxDistance = 35f;
        m_FinalChargeAudioSource.dopplerLevel = 0f;
    }

    void OnEnable()  { s_All.Add(this); }
    void OnDisable()
    {
        s_All.Remove(this);
        StopFinalChargeSound();
    }

    public void Charge()
    {
        if (IsCharging) return;
        StartCoroutine(ChargeRoutine());
    }

    IEnumerator ChargeRoutine()
    {
        IsCharging = true;
        m_Waiting = false;
        m_Committed = false;

        m_Dir = transform.forward;
        m_Dir.y = 0f;
        m_Dir.Normalize();
        m_RunStart = m_Rb.position;

        float pauseDistance = m_PauseAfterCells * m_CellSize;
        bool hasPaused = pauseDistance >= m_Distance;
        m_Committed = hasPaused; // no pause on this run, so it is committed from the start
        if (hasPaused)
            PlayFinalChargeSound();

        while (FlatDistance(m_RunStart, m_Rb.position) < m_Distance)
        {
            if (!hasPaused && FlatDistance(m_RunStart, m_Rb.position) >= pauseDistance)
            {
                hasPaused = true;
                Stop();
                m_Waiting = true;
                PlayPreviewSound();

                yield return new WaitForSeconds(m_PauseTime);

                while (!PathIsClear())
                {
                    Stop();
                    yield return new WaitForFixedUpdate();
                }

                PlayResumeSound();
                PlayFinalChargeSound();
                m_Waiting = false;
                m_Committed = true;
            }

            Vector3 v = m_Rb.linearVelocity;
            m_Rb.linearVelocity = new Vector3(m_Dir.x * m_Speed, v.y, m_Dir.z * m_Speed);
            yield return new WaitForFixedUpdate();
        }

        StopFinalChargeSound();
        Stop();
        m_Committed = false;
        yield return new WaitForSeconds(m_StopTime);

        m_Rb.linearVelocity = Vector3.zero;
        m_Rb.position = m_StartPosition;
        transform.position = m_StartPosition;
        IsCharging = false;
    }

    void PlayPreviewSound()
    {
        if (m_PreviewSound != null && m_AudioSettings != null)
            m_AudioSource.PlayOneShot(m_PreviewSound, m_AudioSettings.previewVolume);
    }

    void PlayResumeSound()
    {
        if (m_ResumeSound != null && m_AudioSettings != null)
            m_AudioSource.PlayOneShot(m_ResumeSound, m_AudioSettings.resumeVolume);
    }

    void PlayFinalChargeSound()
    {
        if (m_FinalChargeLoopSound == null || m_AudioSettings == null) return;

        m_FinalChargeAudioSource.clip = m_FinalChargeLoopSound;
        m_FinalChargeAudioSource.volume = m_AudioSettings.finalChargeVolume;
        m_FinalChargeAudioSource.Play();
    }

    void StopFinalChargeSound()
    {
        if (m_FinalChargeAudioSource != null)
            m_FinalChargeAudioSource.Stop();
    }

    bool PathIsClear()
    {
        foreach (var other in s_All)
        {
            if (other == this || !other.IsCharging) continue;

            bool otherGoesFirst = other.m_Committed ||
                                  (other.m_Waiting && other.GetInstanceID() < GetInstanceID());
            if (!otherGoesFirst) continue;

            if (WillCollide(other)) return false;
        }
        return true;
    }

    // Assumes both Chucks run at full speed from right now until the end of their run.
    bool WillCollide(ChargeHazard other)
    {
        Vector3 pA = Flat(m_Rb.position);
        Vector3 pB = Flat(other.m_Rb.position);
        Vector3 vA = m_Dir * m_Speed;
        Vector3 vB = other.m_Dir * other.m_Speed;

        float window = Mathf.Min(RemainingDistance() / m_Speed,
                                 other.RemainingDistance() / other.m_Speed);

        Vector3 dp = pA - pB;
        Vector3 dv = vA - vB;
        float dvSq = dv.sqrMagnitude;

        // Time of closest approach, clamped to the time both are still running
        float t = dvSq > 0.0001f ? Mathf.Clamp(-Vector3.Dot(dp, dv) / dvSq, 0f, window) : 0f;

        return (dp + dv * t).magnitude < m_SafeDistance;
    }

    float RemainingDistance()
    {
        return Mathf.Max(0f, m_Distance - FlatDistance(m_RunStart, m_Rb.position));
    }

    void Stop()
    {
        Vector3 v = m_Rb.linearVelocity;
        m_Rb.linearVelocity = new Vector3(0f, v.y, 0f);
    }

    static Vector3 Flat(Vector3 v)
    {
        v.y = 0f;
        return v;
    }

    static float FlatDistance(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(Flat(a), Flat(b));
    }
}