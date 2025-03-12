using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private static Vector3 hostSpawnPos;
    private static Vector3 clientSpawnPos;

    public override void OnNetworkSpawn()
    {
        if (IsServer) // The Host (Server) assigns positions
        {
            AssignSpawnPositionServerRpc(OwnerClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AssignSpawnPositionServerRpc(ulong playerId)
    {
        // Find the spawn points in the scene
        GameObject hostSpawn = GameObject.Find("SpawnPoint_Host");
        GameObject clientSpawn = GameObject.Find("SpawnPoint_Client");

        if (hostSpawn != null) hostSpawnPos = hostSpawn.transform.position;
        if (clientSpawn != null) clientSpawnPos = clientSpawn.transform.position;

        // Assign unique positions
        if (playerId == 0) // Host
        {
            transform.position = hostSpawnPos;
        }
        else // Client
        {
            transform.position = clientSpawnPos;
        }

        Debug.Log($"Player {playerId} spawned at {transform.position}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return; // Ensures only the player's local object triggers it

        if (other.CompareTag("AnswerCube"))
        {
            AnswerDisplay answerDisplay = other.GetComponent<AnswerDisplay>();

            if (answerDisplay != null)
            {
                string selectedAnswer = answerDisplay.GetAnswerText();
                ulong clientId = OwnerClientId;

                Debug.Log($"✅ Player {clientId} collided with {selectedAnswer}");

                // ✅ Call a ServerRpc to validate the answer
                SubmitAnswerServerRpc(clientId, selectedAnswer);
            }
        }
    }

    // ✅ New method to send the answer to the server
    [ServerRpc(RequireOwnership = false)]
    private void SubmitAnswerServerRpc(ulong playerId, string answer)
    {
        GameTimerManager gameTimerManager = FindObjectOfType<GameTimerManager>();
        if (gameTimerManager != null)
        {
            gameTimerManager.CheckAnswer(playerId, answer);
        }
    }

}
