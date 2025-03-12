using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button host_btn;
    [SerializeField] private Button client_btn;
    [SerializeField] private Button startGameButton; // ✅ Start Button
    [SerializeField] private TMP_Text joinCodeText;
    [SerializeField] private TMP_InputField joinCodeInputField;

    [SerializeField] private GameTimerManager gameTimerManager; // ✅ Assigned in Inspector

    private string joinCode;
    private int maxPlayers = 2;

    private void Awake()
    {
        host_btn.onClick.AddListener(() => StartHostRelay());
        client_btn.onClick.AddListener(() => StartClientRelay(joinCodeInputField.text));
        startGameButton.onClick.AddListener(() => StartGameButtonClicked()); // ✅ Start Button
        startGameButton.gameObject.SetActive(false); // ✅ Hide button at start
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void StartHostRelay()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
        joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        var serverData = new RelayServerData(allocation, "dtls");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);
        NetworkManager.Singleton.StartHost();
        joinCodeText.text = joinCode;

        Debug.Log("✅ Host Started! Waiting for client to join.");

        // ✅ Show "Start Game" button only for the host
        startGameButton.gameObject.SetActive(true);
    }

    public async void StartClientRelay(string joinCode)
    {
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        var serverData = new RelayServerData(joinAllocation, "dtls");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);
        NetworkManager.Singleton.StartClient();

        Debug.Log("✅ Client Joined Party.");

        // ✅ Make sure the client NEVER sees the "Start Game" button
        startGameButton.gameObject.SetActive(false);
    }

    // ✅ Host Clicks "Start Game"
    public void StartGameButtonClicked()
    {
        if (gameTimerManager == null)
        {
            Debug.LogError("❌ GameTimerManager is NULL! Make sure it's assigned in the Inspector.");
            return;
        }

        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.Log("⚠️ Only the Host can start the game!");
            return;
        }

        Debug.Log("✅ Host clicked Start Game. Starting Countdown...");

        startGameButton.gameObject.SetActive(false); // ✅ Hide Start Button after clicking
        gameTimerManager.StartCountdownServerRpc();
    }
}
