using UnityEngine;

public class UpgradeBehavior : MonoBehaviour
{
    public int playerDamageIncrease = 15;
    public int playerHealthIncrease = 25;
    public int playerSpeedIncrease = 3;
    public GameObject player;
    PlayerHealth playerHealth;
    LaserShooter laserShooter;
    FPSPlayerController fpsPlayerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void IncreasePlayerDamage()
    {
        GetPlayer();
        laserShooter.IncreaseCardDamage(playerDamageIncrease);
    }

    // increases the player health and heals to full
    public void IncreasePlayerTotalHealth()
    {
        GetPlayer();
        playerHealth.TakeBonusHealth(playerHealthIncrease);
    }

    public void IncreasePlayerSpeed()
    {
        GetPlayer();
        fpsPlayerController.IncreaseSpeed(playerSpeedIncrease);
    }

    public void GetPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = player.GetComponent<PlayerHealth>();
        laserShooter = player.GetComponent<LaserShooter>();
        fpsPlayerController = player.GetComponent<FPSPlayerController>();
    }
}
