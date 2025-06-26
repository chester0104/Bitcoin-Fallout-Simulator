using UnityEngine;

public class PickupBehavior : MonoBehaviour
{
    public enum PickupType { Batteries, Gears, Cheese, Apple, Orbs1, Orbs5, Orbs25, Orbs100, Orbs1000 }
    public PickupType pickupType;
    public LevelManager levelManager;

    [Header("Particle Settings")]
    public GameObject player;
    public float minDistance = 1f;
    public float maxDistance = 50f;
    public float minSize = 0.1f;
    public float maxSize = 1f;
    ParticleSystem ps;
    void Start()
    {
        
        ps = GetComponent<ParticleSystem>();
        levelManager = FindFirstObjectByType<LevelManager>();

        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        float t = Mathf.InverseLerp(maxDistance, minDistance, distance);
        float size = Mathf.Lerp(minSize, maxSize, t);

        ps.startSize = size;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switch (pickupType)
            {
                case PickupType.Batteries:
                    levelManager.UpdateScore(50);
                    levelManager.AddBatteries();
                    break;
                case PickupType.Gears:
                    levelManager.UpdateScore(50);
                    levelManager.AddGears();
                    break;
                case PickupType.Cheese:
                    other.GetComponent<PlayerHealth>().TakeHealth(30);
                    break;
                case PickupType.Apple:
                    other.GetComponent<PlayerHealth>().TakeHealth(15);
                    break;
                case PickupType.Orbs1:
                    other.GetComponent<PlayerXP>().TakeXP(1);
                    break;
                case PickupType.Orbs5:
                    other.GetComponent<PlayerXP>().TakeXP(5);
                    break;
                case PickupType.Orbs25:
                    other.GetComponent<PlayerXP>().TakeXP(25);
                    break;
                case PickupType.Orbs100:
                    other.GetComponent<PlayerXP>().TakeXP(100);
                    break;
                case PickupType.Orbs1000:
                    other.GetComponent<PlayerXP>().TakeXP(1000);
                    break;
            }

            Destroy(gameObject);
        }
    }
}
