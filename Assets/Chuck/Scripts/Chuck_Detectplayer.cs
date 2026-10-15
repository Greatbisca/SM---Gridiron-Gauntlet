using UnityEngine;

public class DetetarPlayer : MonoBehaviour
{
    [SerializeField] AudioClip m_HitSound;
    [SerializeField] AudioVolumeSettings m_AudioSettings;

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
        if (playerLife.TirarVida(1, transform.position) && m_HitSound != null)
            AudioSource.PlayClipAtPoint(m_HitSound, playerLife.transform.position, m_AudioSettings.hitVolume);
    }
}