using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
    [SerializeField] private QuizGame quizGame; // Drag and drop the GameManager in the Inspector

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AnswerCube")) // Ensure cubes are tagged "AnswerCube"
        {
            string chosenAnswer = other.gameObject.name; // Get the cube's name (A, B, C, D)
            quizGame.CheckAnswer(chosenAnswer); // Pass the chosen answer to the QuizGame script
        }
    }
}
