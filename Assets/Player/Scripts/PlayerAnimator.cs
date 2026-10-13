using UnityEngine;

/// Put on the player ROOT. Feeds the Animator on the Luigi child and turns him toward his movement.
/// Reads the Rigidbody velocity (falls back to position change if there is no Rigidbody).
public class PlayerAnimator : MonoBehaviour
{
    public Animator m_Animator;

    [Header("Running animation")]
    [Tooltip("Movement slower than this counts as standing still")]
    public float m_MinSpeed = 0.3f;
    [Tooltip("Higher = the animation reacts faster to starting/stopping")]
    public float m_Smoothing = 12f;

    [Header("Facing")]
    public bool m_FaceMoveDirection = true;
    public float m_TurnSpeed = 15f;
    [Tooltip("If Luigi runs sideways or backwards, try 90, 180 or -90")]
    public float m_YawOffset = 0f;
    [Tooltip("Seconds he keeps his facing after being hit (so he falls the right way)")]
    public float m_HitLockTime = 1.5f;

    [Header("Debug (read only)")]
    public float m_DebugSpeed;

    static readonly int SpeedId = Animator.StringToHash("Speed");
    static readonly int HitId   = Animator.StringToHash("Hit");
    static readonly int LoseId  = Animator.StringToHash("Lose");
    static readonly int WinId   = Animator.StringToHash("Win");

    Rigidbody m_Rb;
    Vector3 m_LastPosition;
    Vector3 m_MoveDir = Vector3.forward;
    float m_RawSpeed;
    float m_Speed;
    float m_FacingLockedUntil;

    void Awake()
    {
        if (m_Animator == null) m_Animator = GetComponentInChildren<Animator>();
        if (m_Animator == null)
        {
            Debug.LogWarning($"{name}: no Animator found, PlayerAnimator disabled.", this);
            enabled = false;
            return;
        }
        m_Rb = GetComponent<Rigidbody>();
        m_LastPosition = transform.position;
    }

    void FixedUpdate()
    {
        Vector3 vel;
        if (m_Rb != null)
        {
            vel = m_Rb.linearVelocity;
        }
        else
        {
            vel = (transform.position - m_LastPosition) / Time.fixedDeltaTime;
            m_LastPosition = transform.position;
        }

        vel.y = 0f;
        float speed = vel.magnitude;
        m_RawSpeed = speed < m_MinSpeed ? 0f : speed;
        if (m_RawSpeed > 0f) m_MoveDir = vel.normalized;
    }

    void Update()
    {
        m_Speed = Mathf.Lerp(m_Speed, m_RawSpeed, m_Smoothing * Time.deltaTime);
        m_Animator.SetFloat(SpeedId, m_Speed);
        m_DebugSpeed = m_Speed;

        if (m_FaceMoveDirection && m_RawSpeed > 0f && Time.time >= m_FacingLockedUntil)
        {
            Transform model = m_Animator.transform;
            Quaternion target = Quaternion.LookRotation(m_MoveDir) * Quaternion.Euler(0f, m_YawOffset, 0f);
            model.rotation = Quaternion.Slerp(model.rotation, target, m_TurnSpeed * Time.deltaTime);
        }
    }

    // Call these from the life script / GameManager
    public void PlayHit()
    {
        m_FacingLockedUntil = Time.time + m_HitLockTime;
        m_Animator.SetTrigger(HitId);
    }

    public void PlayLose()
    {
        m_FacingLockedUntil = float.PositiveInfinity;
        m_Animator.SetTrigger(LoseId);
    }

    public void PlayWin()
    {
        m_FacingLockedUntil = float.PositiveInfinity;
        m_Animator.SetTrigger(WinId);
    }

    // Right-click the component in Play mode to test without a life script
    [ContextMenu("Test Hit")]  void TestHit()  { PlayHit(); }
    [ContextMenu("Test Lose")] void TestLose() { PlayLose(); }
    [ContextMenu("Test Win")]  void TestWin()  { PlayWin(); }
}