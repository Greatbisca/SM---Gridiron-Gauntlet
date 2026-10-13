using UnityEngine;

public class DetetarPlayer : MonoBehaviour
{
    // Esta função é ativada automaticamente quando o Collider "Is Trigger" toca em algo
    private void OnTriggerEnter(Collider other)
    {
        // Verifica se o objeto que entrou na área tem a Tag "Player"
        if (other.CompareTag("Player"))
        {
            Debug.Log("O Inimigo tocou no Player!");

            // Procura o script de vida no Player e tira-lhe 1 vida
            Life playerLife = other.GetComponent<Life>();
            if (playerLife != null)
            {
                playerLife.TirarVida(1, transform.position);
            }
            else
            {
                Debug.LogWarning("Player não tem o script Life!");
            }
        }
    }
}