using UnityEngine;

public class Life : MonoBehaviour
{
    [SerializeField] private int m_MaxVidas = 3;
    [SerializeField] private int m_VidasAtuais;

    private float m_TempoInvulnerabilidade = 3f;
    private float m_TempoPiscarDurante = 2.5f; // Pisca durante 2.5s, depois para 0.5s antes de terminar
    private float m_TempoDesdeSucesso = 0f;
    private bool m_Invulneravel = false;

    private float m_CooldownDano = 0.5f;
    private float m_UltimoDano = -1000f;

    private Renderer m_Renderer;
    private float m_TempoEntrepiscos = 0.1f;
    private float m_TempoPiscar = 0f;
    private bool m_VisibilidadeAtual = true;

    private Rigidbody m_Rigidbody;
    [SerializeField] private float m_KnockbackForce = 100f;

    void Start()
    {
        m_VidasAtuais = m_MaxVidas;
        m_Renderer = GetComponent<Renderer>();
        m_Rigidbody = GetComponent<Rigidbody>();

        if (m_Renderer == null)
        {
            Debug.LogWarning("Player não tem um Renderer! Piscar não funcionará.");
        }

        if (m_Rigidbody == null)
        {
            Debug.LogWarning("Player não tem um Rigidbody! Knockback não funcionará.");
        }

        Debug.Log($"Player iniciado com {m_VidasAtuais} vidas");
    }

    void Update()
    {
        // Atualiza o tempo de invulnerabilidade
        if (m_Invulneravel)
        {
            m_TempoDesdeSucesso += Time.deltaTime;

            // Lógica de piscar ENQUANTO o tempo de piscar não terminou
            if (m_TempoDesdeSucesso < m_TempoPiscarDurante && m_Renderer != null)
            {
                m_TempoPiscar += Time.deltaTime;

                // A cada 0.1 segundos, faz toggle de visibilidade
                if (m_TempoPiscar >= m_TempoEntrepiscos)
                {
                    m_VisibilidadeAtual = !m_VisibilidadeAtual;
                    m_Renderer.enabled = m_VisibilidadeAtual;
                    m_TempoPiscar = 0f;
                }
            }
            else if (m_TempoDesdeSucesso >= m_TempoPiscarDurante && m_Renderer != null)
            {
                // Após passar o tempo de piscar, garante que está visível
                m_Renderer.enabled = true;
                m_VisibilidadeAtual = true;
            }

            if (m_TempoDesdeSucesso >= m_TempoInvulnerabilidade)
            {
                m_Invulneravel = false;
                m_TempoDesdeSucesso = 0f;
                m_TempoPiscar = 0f;

                // Garante que o Player fica visível novamente
                if (m_Renderer != null)
                {
                    m_Renderer.enabled = true;
                    m_VisibilidadeAtual = true;
                }

                Debug.Log("Player voltou ao normal");
            }
        }
    }

    public void TirarVida(int dano, Vector3 posicaoChuck)
    {
        // Se estiver em invulnerabilidade, não tira vida
        if (m_Invulneravel)
        {
            return;
        }

        // Verifica cooldown individual deste Player
        if (Time.time - m_UltimoDano < m_CooldownDano)
        {
            return; // Espera mais um pouco
        }

        m_VidasAtuais -= dano;
        m_Invulneravel = true;
        m_TempoDesdeSucesso = 0f;
        m_UltimoDano = Time.time;

        Debug.Log($"Player atingido! Vidas restantes: {m_VidasAtuais}. Imortal por {m_TempoInvulnerabilidade} segundos");

        // Aplicar knockback
        if (m_Rigidbody != null)
        {
            Vector3 direcao = (transform.position - posicaoChuck).normalized;
            m_Rigidbody.linearVelocity = Vector3.zero; // Reseta velocidade anterior
            m_Rigidbody.AddForce(direcao * m_KnockbackForce, ForceMode.Impulse);
        }

        // Se as vidas chegaram a 0 ou menos
        if (m_VidasAtuais <= 0)
        {
            MorrerPlayer();
        }
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
