using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    public float speed = 2f;
    public float mouseSensitivity = 2f; // Sensitivity for mouse movement
    private float pitch = 0f; // Up and down rotation
    private float yaw = 0f; // Left and right rotation

    public List<Color> colors = new List<Color>();

    [SerializeField] private AudioListener audioListener;
    [SerializeField] private Camera playerCamera;

    private bool canMove = false; // ✅ Players start frozen until countdown ends

    void Update()
    {
        if (!IsOwner || !canMove) return; // ✅ Prevent movement before countdown ends

        HandleMovement();
        HandleMouseLook(); // ✅ Add mouse look functionality
    }

    void HandleMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) moveDirection.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDirection.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDirection.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDirection.x = +1f;

        transform.position += transform.TransformDirection(moveDirection) * speed * Time.deltaTime;
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f); // Limit up/down rotation

        transform.rotation = Quaternion.Euler(0, yaw, 0); // Rotate player horizontally
        playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0); // Rotate camera vertically
    }

    public override void OnNetworkSpawn()
    {
        GetComponent<MeshRenderer>().material.color = colors[(int)OwnerClientId];

        if (!IsOwner) return;
        audioListener.enabled = true;
        playerCamera.enabled = true;
        Cursor.lockState = CursorLockMode.Locked; // Lock cursor for better FPS control
        Cursor.visible = false;
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
