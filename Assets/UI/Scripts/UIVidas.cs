using UnityEngine;
using UnityEngine.UIElements;

public class UIVidas : MonoBehaviour
{
    [SerializeField] private Life m_Player1;
    [SerializeField] private Life m_Player2;

    [SerializeField] private Label m_TextoVidas1;
    [SerializeField] private Label m_TextoVidas2;

    void OnEnable()
    {
        UIDocument documento = GetComponent<UIDocument>();

        m_TextoVidas1 = documento.rootVisualElement.Q<Label>("TextoVidas1");
        m_TextoVidas2 = documento.rootVisualElement.Q<Label>("TextoVidas2");

        if (m_Player1 == null || m_Player2 == null)
        {
            Life[] players = FindObjectsByType<Life>(FindObjectsSortMode.None);

            if (players.Length >= 2)
            {
                m_Player1 = players[0];
                m_Player2 = players[1];
            }
            else
            {
                Debug.LogError("UIVidas: Não consegui encontrar 2 Players!");
            }
        }
    }

    void Update()
    {
        if (m_Player1 != null && m_TextoVidas1 != null)
        {
            int vidasAtuais = m_Player1.ObterVidasAtuais();
            m_TextoVidas1.text = $"P1: {vidasAtuais}";
        }

        if (m_Player2 != null && m_TextoVidas2 != null)
        {
            int vidasAtuais = m_Player2.ObterVidasAtuais();
            m_TextoVidas2.text = $"P2: {vidasAtuais}";
        }
    }

}
