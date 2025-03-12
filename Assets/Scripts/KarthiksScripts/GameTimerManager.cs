using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameTimerManager : NetworkBehaviour
{
    [SerializeField] private TMP_Text player1TimerText;
    [SerializeField] private TMP_Text player2TimerText;
    [SerializeField] private TMP_Text globalTimerText;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private Renderer floorRenderer;
    [SerializeField] private List<Material> floorMaterials = new List<Material>();

    //[SerializeField] private List<string> questions = new List<string>();
    //[SerializeField] private List<string> correctAnswers = new List<string>();
    private List<string> questions = new List<string>();
    private List<string> correctAnswers = new List<string>();


    private float player1Timer = 0f;
    private float player2Timer = 0f;
    private float globalTimer = 30f;
    private bool timersStarted = false;
    private bool isGamePaused = true;

    private int currentQuestionIndex = -1;
    private bool player1Answered = false;
    private bool player2Answered = false;

    private int questionCount = 0;
    private Vector3 player1SpawnPos;
    private Vector3 player2SpawnPos;
    private void Start()
    {
        // ✅ Initialize Questions and Answers
        questions = new List<string>
        {
            "  \nWhat is the central component of a computer that processes instructions? \nA) Monitor \nB) CPU  \nC) Scanner \nD) Mouse ",
            "  \nWhat are the two main components inside the CPU?\nA) Control Unit and ALU\nB) Monitor and Keyboard \nC) RAM and Hard Drive \nD) Scanner and Printer",
            "  \nWhat is the function of the Control Unit inside the CPU?\nA) It stores data permanently\nB) It controls input and output devices \nC) It directs the flow of data and instructions \nD) It converts digital signals to sound ?",
            "  \nWhat is the function of the ALU inside the CPU?\nA) It performs arithmetic and logical operations \nB) It stores images and videos\nC) It manages external storage devices \nD) It controls network connections?"
        };

        correctAnswers = new List<string> { "B", "A", "C", "A" };

        currentQuestionIndex = -1; // Start before the first question
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GameObject hostSpawn = GameObject.Find("SpawnPoint_Host");
            GameObject clientSpawn = GameObject.Find("SpawnPoint_Client");

            if (hostSpawn != null) player1SpawnPos = hostSpawn.transform.position;
            if (clientSpawn != null) player2SpawnPos = clientSpawn.transform.position;

            Debug.Log("✅ GameTimerManager running on SERVER.");
        }
        else
        {
            Debug.Log("❌ GameTimerManager running on CLIENT.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartCountdownServerRpc()
    {
        if (!IsServer) return;
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        isGamePaused = true;
        countdownText.gameObject.SetActive(true);

        for (int i = 10; i > 0; i--)
        {
            UpdateCountdownClientRpc(i.ToString());
            yield return new WaitForSecondsRealtime(1f);
        }

        UpdateCountdownClientRpc("Start!");
        yield return new WaitForSecondsRealtime(1f);
        ClearCountdownClientRpc();

        isGamePaused = false;
        EnablePlayerMovement(); // ✅ Players can move after countdown
        StartTimersServerRpc();
        SetNextQuestionServerRpc();
    }

    [ClientRpc]
    private void UpdateCountdownClientRpc(string text)
    {
        countdownText.text = text;
    }

    [ClientRpc]
    private void ClearCountdownClientRpc()
    {
        countdownText.text = "";
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartTimersServerRpc()
    {
        timersStarted = true;
    }

    private void Update()
    {
        if (!IsServer || !timersStarted || isGamePaused) return;

        if (globalTimer > 0)
        {
            globalTimer -= Time.deltaTime;
            if (!player1Answered) player1Timer += Time.deltaTime;
            if (!player2Answered) player2Timer += Time.deltaTime;
        }

        if (globalTimer <= 0 || (player1Answered && player2Answered))
        {
            globalTimer = 0;
            ResetTimersAndNextQuestion();
        }

        UpdateTimerClientRpc(player1Timer, player2Timer, Mathf.Max(globalTimer, 0));
    }

    [ClientRpc]
    private void UpdateTimerClientRpc(float player1Time, float player2Time, float globalTime)
    {
        player1TimerText.text = "Player 0 Time: " + Mathf.FloorToInt(player1Time).ToString("00") + "s";
        player2TimerText.text = "Player 1 Time: " + Mathf.FloorToInt(player2Time).ToString("00") + "s";
        globalTimerText.text = "Total Time: " + Mathf.FloorToInt(globalTime).ToString("00") + "s";
    }

    private void ResetTimersAndNextQuestion()
    {
        if (currentQuestionIndex >= questions.Count - 1)
        {
            EndGame();
            return;
        }

        globalTimer = 30f;
        player1Answered = false;
        player2Answered = false;

        questionCount++;

        if (questionCount % 2 == 0) ChangeFloorMaterialServerRpc();

        ResetPlayerPositionsServerRpc();
        SetNextQuestionServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetNextQuestionServerRpc()
    {
        if (questions.Count == 0) return;

        // Stop when all questions are exhausted
        if (currentQuestionIndex >= questions.Count - 1)
        {
            Debug.Log("✅ All questions have been answered. Game Over.");
            EndGame();  // ✅ Now correctly notifies all clients
            return;
        }

        currentQuestionIndex++;
        UpdateQuestionClientRpc(questions[currentQuestionIndex]);
    }



    [ClientRpc]
    private void UpdateQuestionClientRpc(string question)
    {
        questionText.text = question;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeFloorMaterialServerRpc()
    {
        if (floorMaterials.Count == 0) return;

        int materialIndex = questionCount / 2 % floorMaterials.Count;
        UpdateFloorMaterialClientRpc(materialIndex);
    }

    private void EndGame()
    {
        isGamePaused = true;  // Stop the game on the server
        DisplayGameOverClientRpc(); // ✅ Tell all clients to show "Game Over"
    }

    // ✅ This method ensures all clients see "Game Over"
    [ClientRpc]
    private void DisplayGameOverClientRpc()
    {
        questionText.text = "🎉 Game Over! Thanks for playing! 🎉";
        globalTimerText.text = "";  // Hide timer
        player1TimerText.text = "";
        player2TimerText.text = "";
    }


    [ClientRpc]
    private void UpdateFloorMaterialClientRpc(int materialIndex)
    {
        floorRenderer.material = floorMaterials[materialIndex];
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetPlayerPositionsServerRpc()
    {
        foreach (var player in FindObjectsOfType<PlayerMovement>())
        {
            Vector3 newPosition = (player.OwnerClientId == 0) ? player1SpawnPos : player2SpawnPos;
            player.MovePlayerToPositionClientRpc(newPosition);
        }
    }

    public void CheckAnswer(ulong playerId, string answer)
    {
        if (!IsServer) return; // ✅ Ensure only the server processes answers

        if (questions.Count == 0 || correctAnswers.Count == 0 || currentQuestionIndex < 0) return;

        bool isCorrect = answer == correctAnswers[currentQuestionIndex];

        if (isCorrect)
        {
            if (playerId == 0)
            {
                player1Answered = true;
                Debug.Log("✅ Player 0 (Host) answered correctly.");
            }
            else if (playerId == 1)
            {
                player2Answered = true;
                Debug.Log("✅ Player 1 (Client) answered correctly.");
            }
        }

        if (player1Answered && player2Answered)
        {
            globalTimer = 0;
            ResetTimersAndNextQuestion();
        }
    }


    private void EnablePlayerMovement()
    {
        foreach (var player in FindObjectsOfType<PlayerMovement>())
        {
            player.EnableMovementClientRpc();
        }
    }
}
