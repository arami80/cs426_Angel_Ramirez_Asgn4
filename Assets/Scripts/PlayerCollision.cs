using UnityEngine;
using Unity.Netcode;

public class PlayerCollision : NetworkBehaviour
{
    public float launchForce = 30f; // ✅ High force applied to the ball

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return; // ✅ Only allow the owner to detect collisions

        if (collision.gameObject.CompareTag("Ball")) // ✅ Detect collision with ball
        {
            Vector3 hitDirection = (collision.transform.position - transform.position).normalized;
            RequestBallLaunchServerRpc(collision.gameObject.GetComponent<NetworkObject>().NetworkObjectId, hitDirection);
        }
    }

    [ServerRpc] // ✅ Server applies physics to the ball
    private void RequestBallLaunchServerRpc(ulong ballId, Vector3 direction, ServerRpcParams rpcParams = default)
    {
        NetworkObject ballObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[ballId];
        if (ballObject != null)
        {
            BallPhysics ballScript = ballObject.GetComponent<BallPhysics>();
            if (ballScript != null)
            {
                ballScript.ApplyLaunchForceClientRpc(direction);
            }
        }
    }
}
