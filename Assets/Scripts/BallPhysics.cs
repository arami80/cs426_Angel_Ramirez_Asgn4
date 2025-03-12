using UnityEngine;
using Unity.Netcode;

public class BallPhysics : NetworkBehaviour
{
    private Rigidbody rb;
    public float springForce = 30f; // ✅ Force applied when hit by the player

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Prevent passing through objects
            rb.interpolation = RigidbodyInterpolation.Interpolate; // Smooth physics updates
        }
    }

    [ClientRpc] // ✅ Syncs force to all clients
    public void ApplyLaunchForceClientRpc(Vector3 direction)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; // ✅ Reset any existing movement
            rb.AddForce(direction * springForce, ForceMode.Impulse);
        }
    }
}
