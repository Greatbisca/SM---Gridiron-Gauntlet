using UnityEngine;
using UnityEngine.UIElements;

public class UIVidas : MonoBehaviour
{
    [SerializeField] private Life m_Player1;
    [SerializeField] private Life m_Player2;

    //[SerializeField] private Label m_TextoVidas1;
    //[SerializeField] private Label m_TextoVidas2;
	[SerializeField] private Image m_VidaPlayer1;
    [SerializeField] private Image m_VidaPlayer2;
	
	[SerializeField] private Texture2D imagemVida3;
	[SerializeField] private Texture2D imagemVida2;
	[SerializeField] private Texture2D imagemVida1;
	StyleBackground barraVida;
	
    void OnEnable()
    {
        UIDocument documento = GetComponent<UIDocument>();
        //m_TextoVidas1 = documento.rootVisualElement.Q<Label>("TextoVidas1");
        //m_TextoVidas2 = documento.rootVisualElement.Q<Label>("TextoVidas2");
		m_VidaPlayer1 = documento.rootVisualElement.Q<Image>("life_player1");
		m_VidaPlayer2 = documento.rootVisualElement.Q<Image>("life_player2");
		barraVida = new StyleBackground(imagemVida3);
        
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
	
        if (m_Player1 != null && m_VidaPlayer1 != null)
        {
            int vidasAtuais = m_Player1.ObterVidasAtuais();
            //m_TextoVidas1.text = $"P1: {vidasAtuais}";
			defineImagem(vidasAtuais);
			m_VidaPlayer1.style.backgroundImage = barraVida;
        }

        if (m_Player2 != null && m_VidaPlayer2 != null)
        {
            int vidasAtuais = m_Player2.ObterVidasAtuais();
            //m_TextoVidas2.text = $"P2: {vidasAtuais}";
			defineImagem(vidasAtuais);
			m_VidaPlayer2.style.backgroundImage = barraVida;
			
			// Espelhar horizontalmente a imagem do Player 2
			m_VidaPlayer2.style.scale = new Scale(new Vector2(-1, 1));
        }
    }
	
	void defineImagem(int vida){		
		if(vida == 3){
			barraVida = new StyleBackground(imagemVida3);
		}else if(vida == 2){
			barraVida = new StyleBackground(imagemVida2);
		}else if(vida == 1){
			barraVida = new StyleBackground(imagemVida1);
		}else
			barraVida = null;
	}

}
