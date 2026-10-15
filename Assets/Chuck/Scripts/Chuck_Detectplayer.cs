using UnityEngine;

public class DetetarPlayer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        TentarAcertar(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TentarAcertar(collision.collider);
    }

    private void TentarAcertar(Collider other)
    {
        Life playerLife = other.GetComponentInParent<Life>();
        if (playerLife == null) return;

        Debug.Log("O Inimigo tocou no Player!");
        playerLife.TirarVida(1, transform.position);
    }
}