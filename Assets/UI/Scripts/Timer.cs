using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

public class Timer : MonoBehaviour
{
    [SerializeField] Label timerText;
    float elapsedTime=30;

    void OnEnable()
    {
        timerText = GetComponent<UIDocument>().rootVisualElement.Q<Label>("timer");
    }

    void Update()
    {
        if(elapsedTime > 1)
            elapsedTime -= Time.deltaTime;


        int seconds = Mathf.FloorToInt(elapsedTime % 60);

        timerText.text= string.Format("{00} ", seconds);
    }
}