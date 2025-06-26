using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    public float forceMagnitude = 100;
    public float explosionRadius = 10;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Reducto();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Reducto()
    {
        Rigidbody[] pieces = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in pieces)
        {
            rb.AddExplosionForce(forceMagnitude, transform.position, explosionRadius);
        }    
    
        //Debug.Log("Rigidbodies: " + pieces.Length);

    }
}
