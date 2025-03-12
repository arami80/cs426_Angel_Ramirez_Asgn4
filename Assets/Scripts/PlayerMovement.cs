using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    public float speed = 2f;
    public List<Color> colors = new List<Color>();

    [SerializeField] private AudioListener audioListener;
    [SerializeField] private Camera playerCamera;

    private bool canMove = false; // ✅ Players start frozen until countdown ends

    void Update()
    {
        if (!IsOwner || !canMove) return; // ✅ Prevent movement before countdown ends

        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) moveDirection.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDirection.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDirection.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDirection.x = +1f;

        transform.position += moveDirection * speed * Time.deltaTime;
    }

    public override void OnNetworkSpawn()
    {
        GetComponent<MeshRenderer>().material.color = colors[(int)OwnerClientId];

        if (!IsOwner) return;
        audioListener.enabled = true;
        playerCamera.enabled = true;
    }

    // ✅ Allow movement after countdown ends
    [ClientRpc]
    public void EnableMovementClientRpc()
    {
        Debug.Log($"✅ Movement enabled for Player {OwnerClientId}");
        canMove = true;
    }

    // ✅ Teleport player back to spawn when a new question starts
    [ClientRpc]
    public void MovePlayerToPositionClientRpc(Vector3 newPosition)
    {
        transform.position = newPosition;
    }
}
