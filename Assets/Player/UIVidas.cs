using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIVidas : MonoBehaviour
{
    [Header("Referências dos Players")]
    [SerializeField] private Life m_Player1;
    [SerializeField] private Life m_Player2;

    [Header("UI Player 1 (Esquerda)")]
    [SerializeField] private TextMeshProUGUI m_TextoVidas1;
    [SerializeField] private Image[] m_Coracoes1;

    [Header("UI Player 2 (Direita)")]
    [SerializeField] private TextMeshProUGUI m_TextoVidas2;
    [SerializeField] private Image[] m_Coracoes2;

    [Header("Cores")]
    [SerializeField] private Color m_CorVivo = Color.red;
    [SerializeField] private Color m_CorMorto = Color.gray;

    void Start()
    {
        // Se não tiver referências, tenta encontrar os Players
        if (m_Player1 == null || m_Player2 == null)
        {
            Life[] players = FindObjectsOfType<Life>();
            if (players.Length >= 2)
            {
                m_Player1 = players[0];
                m_Player2 = players[1];
                Debug.Log("UIVidas: Players encontrados automaticamente");
            }
            else
            {
                Debug.LogError("UIVidas: Não consegui encontrar 2 Players!");
                return;
            }
        }

        AtualizarUI();
    }

    void Update()
    {
        AtualizarUI();
    }

    private void AtualizarUI()
    {
        // Atualizar Player 1
        if (m_Player1 != null)
        {
            AtualizarUIPlayer(m_Player1, m_TextoVidas1, m_Coracoes1);
        }

        // Atualizar Player 2
        if (m_Player2 != null)
        {
            AtualizarUIPlayer(m_Player2, m_TextoVidas2, m_Coracoes2);
        }
    }

    private void AtualizarUIPlayer(Life player, TextMeshProUGUI texto, Image[] coracoes)
    {
        int vidasAtuais = player.ObterVidasAtuais();

        // Atualizar Texto
        if (texto != null)
        {
            texto.text = $"P{(player == m_Player1 ? 1 : 2)}: {vidasAtuais}";
        }

        // Atualizar Imagens (Corações)
        if (coracoes != null && coracoes.Length > 0)
        {
            for (int i = 0; i < coracoes.Length; i++)
            {
                if (i < vidasAtuais)
                {
                    coracoes[i].color = m_CorVivo;
                }
                else
                {
                    coracoes[i].color = m_CorMorto;
                }
            }
        }
    }
}
