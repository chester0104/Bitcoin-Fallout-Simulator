// preedit-code/Breakable.cs
using UnityEngine;

public class Breakable : MonoBehaviour
{
    public GameObject cratePieces;
    public GameObject resource1;
    public GameObject resource2;
    public float YOffset = 1.0f;

    void OnCollisionEnter(Collision collision)
    {
        // Check if the colliding object has a "Projectile" tag
        if (collision.gameObject.CompareTag("Projectile"))
        {
            BreakCrate();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Also check for trigger collisions with projectiles
        if (other.CompareTag("Projectile"))
        {
            BreakCrate();
        }
    }

    void BreakCrate()
    {
        Vector3 resourceSpawnPoint = new Vector3(transform.position.x, transform.position.y + YOffset, transform.position.z);
        Instantiate(cratePieces, transform.position, transform.rotation);

        GameObject resourceToSpawn = UnityEngine.Random.value < 0.5f ? resource1 : resource2;
        if(resource1 || resource2)
            Instantiate(resourceToSpawn, resourceSpawnPoint, transform.rotation);

        Destroy(gameObject);
    }
}