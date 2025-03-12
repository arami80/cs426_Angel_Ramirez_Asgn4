using UnityEngine;
using Unity.Netcode;

public class PlayerBounce : NetworkBehaviour
{
    public float bounceForce = 20f;

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return; // ✅ Only allow the owner to detect the collision

        if (collision.gameObject.CompareTag("AnswerCube")) // ✅ Only bounce off the answer cube
        {
            RequestBounceServerRpc();
        }
    }

    [ServerRpc] // ✅ Server handles the bounce
    private void RequestBounceServerRpc(ServerRpcParams rpcParams = default)
    {
        PerformBounceClientRpc();
    }

    [ClientRpc] // ✅ Syncs bounce effect to all clients
    private void PerformBounceClientRpc()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
        }
    }
}
