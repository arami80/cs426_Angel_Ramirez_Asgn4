using UnityEngine;
using TMPro;

public class AnswerDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text answerText;

    public void SetAnswerText(string answer)
    {
        answerText.text = answer;
    }

    public string GetAnswerText()
    {
        return answerText.text;
    }
}
