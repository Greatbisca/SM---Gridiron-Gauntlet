using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Life : MonoBehaviour
{
    [Header("Vidas")]
    [SerializeField] private int m_MaxVidas = 3;
    [SerializeField] private int m_VidasAtuais;

    [Header("Invulnerabilidade")]
    [SerializeField] private float m_TempoInvulnerabilidade = 3f;
    [Tooltip("Pisca durante este tempo; depois fica visivel ate acabar a invulnerabilidade")]
    [SerializeField] private float m_TempoPiscarDurante = 2.5f;
    [SerializeField] private float m_TempoEntrePiscos = 0.1f;

    [Header("Knockback")]
    [SerializeField] private float m_KnockbackVelocidade = 8f;
    [SerializeField] private float m_KnockbackVertical = 3f;
    [SerializeField] private float m_KnockbackDuracao = 0.25f;

    private bool m_Invulneravel = false;
    private float m_CooldownDano = 0.5f;
    private float m_UltimoDano = -1000f;

    private readonly List<Renderer> m_Renderers = new List<Renderer>();
    private Rigidbody m_Rigidbody;
    private movement m_Movement;

    void Start()
    {
        m_VidasAtuais = m_MaxVidas;
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Movement = GetComponent<movement>();

        // Renderers do modelo (filhos). Ignora o do proprio objeto raiz (o capsule escondido)
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            if (r.gameObject == gameObject) continue;
            if (r is MeshRenderer || r is SkinnedMeshRenderer) m_Renderers.Add(r);
        }

        // Se nao ha modelo filho, usa o renderer da raiz (so se estiver visivel)
        if (m_Renderers.Count == 0)
        {
            var raiz = GetComponent<Renderer>();
            if (raiz != null && raiz.enabled) m_Renderers.Add(raiz);
        }

        if (m_Renderers.Count == 0)
            Debug.LogWarning("Player sem Renderers! O piscar nao vai funcionar.");
        if (m_Rigidbody == null)
            Debug.LogWarning("Player sem Rigidbody! O knockback nao vai funcionar.");

        Debug.Log($"Player iniciado com {m_VidasAtuais} vidas");
    }

    public bool TirarVida(int dano, Vector3 posicaoChuck)
    {
        if (m_VidasAtuais <= 0) return false;
        if (m_Invulneravel) return false;
        if (Time.time - m_UltimoDano < m_CooldownDano) return false;

        m_VidasAtuais -= dano;
        m_UltimoDano = Time.time;

        Debug.Log($"Player atingido! Vidas restantes: {m_VidasAtuais}. Imortal por {m_TempoInvulnerabilidade} segundos");

        AplicarKnockback(posicaoChuck);

        if (m_VidasAtuais <= 0)
        {
            MorrerPlayer();
            return true;
        }

        StartCoroutine(InvulnerabilidadeRoutine());
        return true;
    }

    private void AplicarKnockback(Vector3 posicaoChuck)
    {
        Vector3 dir = transform.position - posicaoChuck;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) dir = Vector3.back;
        dir.Normalize();

        Vector3 velocidade = dir * m_KnockbackVelocidade + Vector3.up * m_KnockbackVertical;

        if (m_Movement != null)
            m_Movement.AplicarKnockback(velocidade, m_KnockbackDuracao);
        else if (m_Rigidbody != null)
            m_Rigidbody.linearVelocity = velocidade;
    }

    private IEnumerator InvulnerabilidadeRoutine()
    {
        m_Invulneravel = true;

        bool visivel = true;
        float tempo = 0f;
        while (tempo < m_TempoInvulnerabilidade)
        {
            if (tempo < m_TempoPiscarDurante)
            {
                visivel = !visivel;
                SetVisivel(visivel);
            }
            else
            {
                SetVisivel(true);
            }

            yield return new WaitForSeconds(m_TempoEntrePiscos);
            tempo += m_TempoEntrePiscos;
        }

        SetVisivel(true);
        m_Invulneravel = false;
        Debug.Log("Player voltou ao normal");
    }

    private void SetVisivel(bool visivel)
    {
        foreach (var r in m_Renderers)
            if (r != null) r.enabled = visivel;
    }

    public bool EstaVivo()
    {
        return m_VidasAtuais > 0;
    }

    public int ObterVidasAtuais()
    {
        return m_VidasAtuais;
    }

    public bool EstaInvulneravel()
    {
        return m_Invulneravel;
    }

    private void MorrerPlayer()
    {
        Debug.Log("GAME OVER! O Player morreu!");
        // Aguarda 0.2 segundos para a UI atualizar antes de destruir
        Destroy(gameObject, 0.2f);
    }
}