using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class movement : MonoBehaviour
{
    public Transform groundcheck;
    public float speed;
    public int pontos;
    public LayerMask groundmask;

    public bool disable_move = false;
    [SerializeField] AudioClip runSound;
    [SerializeField] AudioVolumeSettings audioSettings;

    private Rigidbody rb;
    private Vector3 inputDir;
    private float knockbackTimer = 0f;
    private AudioSource runAudioSource;

    public bool EmKnockback => knockbackTimer > 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // evita que o player tombe
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        speed = 4f;
        pontos = 0;

        runAudioSource = GetComponent<AudioSource>();
        if (runAudioSource == null)
            runAudioSource = gameObject.AddComponent<AudioSource>();

        runAudioSource.clip = runSound;
        runAudioSource.loop = true;
        runAudioSource.playOnAwake = false;
        runAudioSource.volume = audioSettings.runVolume;
        runAudioSource.spatialBlend = 0f;
    }

    // Chamado pelo Life quando o player leva um hit
    public void AplicarKnockback(Vector3 velocidade, float duracao)
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        knockbackTimer = duracao;
        rb.linearVelocity = velocidade;
    }

    private void Update()
    {
        inputDir = Vector3.zero;
        if (disable_move)
        {
            StopRunSound();
            return;
        }

        if (EmKnockback)
        {
            StopRunSound();
            return;
        }

        KeyCode right, left, up, down;
        if (gameObject.name == "Player1")
        {
            right = KeyCode.D; left = KeyCode.A; up = KeyCode.W; down = KeyCode.S;
        }
        else
        {
            right = KeyCode.RightArrow; left = KeyCode.LeftArrow; up = KeyCode.UpArrow; down = KeyCode.DownArrow;
        }

        if (Input.GetKey(right)) inputDir += Vector3.right;
        if (Input.GetKey(left)) inputDir += Vector3.left;
        if (Input.GetKey(up)) inputDir += Vector3.forward;
        if (Input.GetKey(down)) inputDir += Vector3.back;

        inputDir = inputDir.normalized;

        if (inputDir != Vector3.zero)
            PlayRunSound();
        else
            StopRunSound();
    }

    private void PlayRunSound()
    {
        if (runAudioSource != null && runAudioSource.clip != null && !runAudioSource.isPlaying)
            runAudioSource.Play();
    }

    private void StopRunSound()
    {
        if (runAudioSource != null && runAudioSource.isPlaying)
            runAudioSource.Stop();
    }

    private void FixedUpdate()
    {
        // Durante o knockback nao mexe na velocidade (senao cancelava o empurrao)
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector3 dir = inputDir;

        if (dir != Vector3.zero)
        {
            float dist = speed * Time.fixedDeltaTime + 0.01f;

            // há o outro player à frente?
            if (rb.SweepTest(dir, out RaycastHit hit, dist) && hit.collider.GetComponentInParent<movement>() != null)
            {
                Vector3 n = hit.normal;
                n.y = 0f;
                n.Normalize();
                dir = Vector3.ProjectOnPlane(dir, n); // anda só ao longo do "muro"
            }
        }

        Vector3 v = dir * speed;
        v.y = rb.linearVelocity.y; // mantém a gravidade
        rb.linearVelocity = v;
    }
}