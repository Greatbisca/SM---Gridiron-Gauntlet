using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class movement : MonoBehaviour
{
    public Transform groundcheck;
    public float speed;
    public int pontos;
    public LayerMask groundmask;

    public bool disable_move = false;

    private Rigidbody rb;
    private Vector3 inputDir;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        //Não é necessário -> rb.freezeRotation = true; // evita que o player tombe
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        speed = 4f;
        pontos = 0;
    }

    private void Update()
    {
        inputDir = Vector3.zero;
        if (disable_move) return;

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
    }

    private void FixedUpdate()
    {
        Vector3 dir = inputDir;

        if (dir != Vector3.zero)
        {
            float dist = speed * Time.fixedDeltaTime + 0.01f;

            // há o outro player à frente? torna mais dificil de empurrar 
            if (rb.SweepTest(dir, out RaycastHit hit, dist) && hit.collider.GetComponentInParent<movement>() != null)
            {
                speed = 2f; 
            }
			else
				speed = 4f; 
        }

        Vector3 v = dir * speed;
        v.y = rb.linearVelocity.y; // mantém a gravidade (Unity 2022 ou anterior: rb.velocity)
        rb.linearVelocity = v;
    }
}