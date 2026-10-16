using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CameraIntro : MonoBehaviour
{
    [Tooltip("Where the intro starts. Place it with GameObject > Align With View.")]
    public Transform m_Start;
    [Tooltip("Optional: pulls the path toward this point, so the camera sweeps in a curve instead of a straight line.")]
    public Transform m_Mid;
    
    public float m_StartDelay = 1f;
    public float m_Duration = 4f;
    public AnimationCurve m_Ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("Press Space to skip the intro")]
    public bool m_AllowSkip = true;
    
    public bool m_FreezePlayers = true;
    public ChuckManager m_ChuckManager;
    [Tooltip("Objects hidden during the intro and shown when it ends (e.g. the HUD canvas)")]
    public GameObject[] m_ShowAfterIntro;
    public UnityEvent m_OnFinished;
    public Transform m_End;

    Vector3 m_EndPos;
    Quaternion m_EndRot;
    movement[] m_Players;
    bool m_Skip;

    void Awake()
    {
        if (m_Start == null)
        {
            Debug.LogWarning("CameraIntro: m_Start is not set, intro disabled.");
            enabled = false;
            return;
        }

        // The pose the camera has in the scene is the gameplay pose
        m_EndPos = transform.position;
        m_EndRot = transform.rotation;
        transform.SetPositionAndRotation(m_Start.position, m_Start.rotation);

        // Awake runs before every Start, so the manager will not auto-start
        if (m_ChuckManager != null) m_ChuckManager.m_AutoStart = false;

        foreach (var go in m_ShowAfterIntro)
            if (go != null) go.SetActive(false);

        if (m_FreezePlayers)
        {
            m_Players = FindObjectsByType<movement>(FindObjectsSortMode.None);
            SetPlayersFrozen(true);
        }
    }

    void Start()
    {
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        float wait = m_StartDelay;
        while (wait > 0f && !Skipped())
        {
            wait -= Time.deltaTime;
            yield return null;
        }

        float t = 0f;
        while (t < 1f && !Skipped())
        {
            t += Time.deltaTime / Mathf.Max(0.01f, m_Duration);
            float k = m_Ease.Evaluate(Mathf.Clamp01(t));
            transform.position = PositionAt(k);
            transform.rotation = Quaternion.Slerp(m_Start.rotation, m_EndRot, k);
            yield return null;
        }

        transform.SetPositionAndRotation(m_EndPos, m_EndRot);
        Finish();
    }

    Vector3 PositionAt(float k)
    {
        Vector3 a = m_Start.position;
        if (m_Mid == null) return Vector3.Lerp(a, m_EndPos, k);

        // Quadratic curve with m_Mid as the control point
        Vector3 b = m_Mid.position;
        return Vector3.Lerp(Vector3.Lerp(a, b, k), Vector3.Lerp(b, m_EndPos, k), k);
    }

    bool Skipped()
    {
        if (m_Skip) return true;
        if (!m_AllowSkip) return false;
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Space)) m_Skip = true;
#endif
        return m_Skip;
    }
    
    public void MoveToEnd()
    {
        if (m_End == null)
        {
            Debug.LogError("CameraIntro: m_End is not assigned!");
            return;
        }

        StartCoroutine(MoveToEndRoutine());
    }

    
    IEnumerator MoveToEndRoutine()
    {
        yield return new WaitForSeconds(2f);

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / 2f;
            float k = Mathf.Clamp01(t);

            transform.position = Vector3.Lerp(startPos, m_End.position, k);
            transform.rotation = Quaternion.Slerp(startRot, m_End.rotation, k);

            yield return null;
        }

        transform.SetPositionAndRotation(m_End.position, m_End.rotation);
    }

    void Finish()
    {
        foreach (var go in m_ShowAfterIntro)
            if (go != null) go.SetActive(true);

        if (m_FreezePlayers) SetPlayersFrozen(false);
        if (m_ChuckManager != null) m_ChuckManager.StartGame();
        m_OnFinished.Invoke();
    }

    void SetPlayersFrozen(bool frozen)
    {
        if (m_Players == null) return;
        foreach (var p in m_Players)
            if (p != null) p.disable_move = frozen;
    }
}