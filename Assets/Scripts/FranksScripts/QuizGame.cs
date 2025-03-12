using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshPro

public class QuizGame : MonoBehaviour
{
    public TMP_Text questionText;       // UI Text for the question
    public TMP_Text timerText;          // UI Text for the global timer
    public TMP_Text personalTimerText;  // UI Text for the personal timer
    public Transform playerStartPosition; // Reference to the player's starting position
    public GameObject player;          // Reference to the player object

    private float globalTimer = 25f;   // Global countdown timer (per question)
    private float personalTimer = 0f;  // Tracks personal time for each question
    private float totalPersonalTime = 0f; // Total personal time across all questions
    private int currentQuestionIndex = 0; // Tracks which question is active
    private bool isQuestionAnswered = false; // Tracks if the current question is answered
    private bool gameEnded = false;

    public FloorMaterialManager FloorMaterial;

    private string[] questions = {
        "What is 2 + 2?\nA. 3   B. 4   C. 5   D. 6",
        "What is 3 + 4?\nA. 5   B. 6   C. 7   D. 8"
    };

    private string[] correctAnswers = { "Cube B", "Cube C" }; // Correct answers for each question

    void Start()
    {
        // Initialize the first question and timers
        InitializeQuestion();
    }

    void Update()
    {
        if (gameEnded) return; // Stop updates if the game is over

        if (!isQuestionAnswered)
        {
            // Update the global timer and personal timer
            globalTimer -= Time.deltaTime;
            personalTimer += Time.deltaTime;

            // Update UI
            timerText.text = "Time Left: " + Mathf.Ceil(globalTimer) + "s";
            personalTimerText.text = "Time Taken: " + personalTimer.ToString("F2") + "s";

            // Check if the global timer runs out
            if (globalTimer <= 0)
            {
                timerText.text = "Time's Up!";
                isQuestionAnswered = true; // Mark the question as answered
                NextQuestion(); // Automatically move to the next question
            }
        }
    }

    public void CheckAnswer(string chosenAnswer)
    {
        if (gameEnded) return;

        if (chosenAnswer == correctAnswers[currentQuestionIndex])
        {
            Debug.Log("Correct Answer! Personal Timer stopped at: " + personalTimer + " seconds.");
            timerText.text = "Correct! Time: " + personalTimer.ToString("F2") + "s";
            personalTimerText.text = "Final Time: " + personalTimer.ToString("F2") + "s";
            isQuestionAnswered = true;

            if (currentQuestionIndex < questions.Length - 1)
            {
                NextQuestion(); // Transition to the next question
            }
            else
            {
                EndGame(true); // End the game if it's the last question
            }
        }
        else
        {
            Debug.Log("Wrong Answer! Try again.");
            timerText.text = "Wrong! Try again!";
        }
    }

    private void NextQuestion()
    {
        // Add the personal timer to the total time
        totalPersonalTime += personalTimer;

        // Reset the global timer for the next question
        globalTimer = 25f;

        // Move to the next question
        currentQuestionIndex++;

        if (currentQuestionIndex < questions.Length)
        {
            // Update the question and reset the personal timer
            questionText.text = questions[currentQuestionIndex];
            correctAnswers[currentQuestionIndex] = correctAnswers[currentQuestionIndex];
            personalTimer = 0f;
            isQuestionAnswered = false;

            // Reset the player's position
            ResetPlayerPosition();
        }
        else
        {
            EndGame(true); // End the game if there are no more questions
        }
    }

    private void EndGame(bool won)
    {
        gameEnded = true;

        if (won)
        {
            Debug.Log("You Win! Total Time Taken: " + totalPersonalTime + " seconds.");
            personalTimerText.text = "Total Time: " + totalPersonalTime.ToString("F2") + "s";
        }
        else
        {
            Debug.Log("Game Over! You ran out of time.");
            personalTimerText.text = "Time's Up! Total Time: " + totalPersonalTime.ToString("F2") + "s";
        }

    }

     private void InitializeQuestion()
    {
        questionText.text = questions[currentQuestionIndex];
        personalTimerText.text = "Time Taken: 0.00s";
        timerText.text = "Time Left: " + Mathf.Ceil(globalTimer) + "s";
        isQuestionAnswered = false;
        gameEnded = false;

        // Reset the player's position

        //ANGELS CODE

        FloorMaterial.UpdateFloorMaterial(currentQuestionIndex);

        ResetPlayerPosition();
    }

    private void ResetPlayerPosition()
    {
        player.transform.position = playerStartPosition.position; // Reset position
        player.transform.rotation = playerStartPosition.rotation; // Reset rotation

        // Reset Rigidbody velocity to prevent it from carrying over momentum
        Rigidbody playerRigidbody = player.GetComponent<Rigidbody>();
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }
    }
}
