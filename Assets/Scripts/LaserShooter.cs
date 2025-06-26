using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LaserShooter : MonoBehaviour
{
    [Header("General Settings")]
    public float SFXVolume = .1f;
    
    [Header("Laser Settings")]
    public Transform firePoint;
    public LineRenderer lineRenderer;
    public float laserLength = 2f;
    public int laserDamage = 10;
    public float laserStunDuration = 2f;
    public float laserReloadTime = 1f;
    public static bool laserPowerup = false;
    public AudioClip laserSFX;

    [Header("Card Settings")]
    public GameObject cardPrefab;
    public float projectileSpeed = 100f;
    public int cardDamage = 25;
    public float cardRange = 20f;
    public float cardReloadTime = 0.5f;
    public static int maxAmmo = 20;
    public AudioClip cardSFX;

    [Header("Crosshair Settings")]
    public int animationSpeed = 5;
    public Image crosshairImage;
    public Color targetColorEnemy;

    private Vector3 originalCrosshairScale;
    private Color originalCrosshairColor;
    private Color currentCrosshairColor;
    private float lastLaserTime = 0f;
    private float lastCardTime = 0f;

    public LayerMask enemyLayerMask = -1;

    void Start()
    {
        // Store original crosshair properties for animation
        if (crosshairImage)
        {
            originalCrosshairColor = crosshairImage.color;
            originalCrosshairScale = crosshairImage.transform.localScale;
        }

        // Use this transform as firepoint if none assigned
        if (!firePoint)
            firePoint = transform;
    }

    void Update()
    {
        // Toggle laser powerup with Q key
        if (Input.GetKeyDown(KeyCode.Q) && Time.timeScale != 0)
            laserPowerup = !laserPowerup;

        // Fire weapon on left mouse click
        if (Input.GetButtonDown("Fire1") && Time.timeScale != 0)
        {
            if (laserPowerup && Time.time >= lastLaserTime + laserReloadTime)
                ShootLaser();
            else if (!laserPowerup && Time.time >= lastCardTime + cardReloadTime)
                ShootCard();
        }
    }

    void FixedUpdate()
    {
        // Update crosshair appearance based on what we're aiming at
        if (crosshairImage)
            InteractiveEffect();
    }

    void ShootLaser()
    {
        if (!firePoint) return;

        lastLaserTime = Time.time;

        Ray ray = new Ray(firePoint.position, firePoint.forward);
        RaycastHit hit;
        Vector3 endPos = ray.origin + ray.direction * laserLength;

        // Check if laser hits an enemy within range
        if (Physics.Raycast(ray, out hit, laserLength, enemyLayerMask))
        {
            endPos = hit.point;

            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyBehavior enemy = hit.collider.GetComponent<EnemyBehavior>();
                if (enemy)
                {
                    enemy.TakeDamage(laserDamage);
                    enemy.Stun(laserStunDuration);
                }
            }
        }

        // Play laser sound effect
        if (laserSFX)
        {
            AudioSource.PlayClipAtPoint(laserSFX, transform.position, SFXVolume);
        }

        // Show visual laser beam
        StartCoroutine(ShowLaser(ray.origin, endPos));
    }

    System.Collections.IEnumerator ShowLaser(Vector3 start, Vector3 end)
    {
        if (lineRenderer)
        {
            // Draw laser line from start to end point
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
            lineRenderer.enabled = true;

            // Show laser for brief moment
            yield return new WaitForSeconds(0.05f);

            lineRenderer.enabled = false;
        }
    }

    void ShootCard()
    {
        if (!cardPrefab) return;

        lastCardTime = Time.time;

        // Spawn card slightly in front of fire point
        Vector3 spawnPos = firePoint.position + firePoint.forward * 0.5f;
        GameObject card = Instantiate(cardPrefab, spawnPos, firePoint.rotation);

        // Ensure card has projectile component
        CardProjectile projectile = card.GetComponent<CardProjectile>();
        if (!projectile)
            projectile = card.AddComponent<CardProjectile>();

        projectile.damage = cardDamage;

        // Launch card forward with physics
        Rigidbody rb = card.GetComponent<Rigidbody>();
        if (rb)
            rb.AddForce(firePoint.forward * projectileSpeed, ForceMode.VelocityChange);

        // Play card throwing sound
        if (cardSFX)
        {
            AudioSource.PlayClipAtPoint(cardSFX, transform.position, SFXVolume);
        }
    }

    public void IncreaseCardDamage(int damageIncrease)
    {
        cardDamage += damageIncrease;
    }

    void InteractiveEffect()
    {
        // Check if we're aiming at an enemy
        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, cardRange, enemyLayerMask))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                // Animate crosshair when targeting enemy (shrink and change color)
                UpdateCrosshairColor();
                CrosshairAnimation(originalCrosshairScale * 0.5f, currentCrosshairColor, animationSpeed);
                return;
            }
        }

        // Return crosshair to normal when not targeting enemy
        CrosshairAnimation(originalCrosshairScale, originalCrosshairColor, animationSpeed);
    }

    void CrosshairAnimation(Vector3 targetScale, Color targetColor, float speed)
    {
        if (!crosshairImage) return;

        // Smoothly transition crosshair scale and color
        float step = speed * Time.deltaTime;
        crosshairImage.color = Color.Lerp(crosshairImage.color, targetColor, step);
        crosshairImage.transform.localScale = Vector3.Lerp(crosshairImage.transform.localScale, targetScale, step);
    }

    void UpdateCrosshairColor()
    {
        // Try to match crosshair color to card material color
        if (cardPrefab)
        {
            Renderer renderer = cardPrefab.GetComponent<Renderer>();
            if (renderer && renderer.sharedMaterial)
                currentCrosshairColor = renderer.sharedMaterial.color;
            else
                currentCrosshairColor = targetColorEnemy;
        }
    }
}

public class CardProjectile : MonoBehaviour
{
    public int damage = 25;
    public float lifetime = 5f;

    void Start()
    {
        // Auto-destroy card after lifetime expires
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Deal damage to enemy on collision
            EnemyBehavior enemy = collision.gameObject.GetComponent<EnemyBehavior>();
            if (enemy)
                enemy.TakeDamage(damage);

            Destroy(gameObject);
        }
        else if (!collision.gameObject.CompareTag("Player"))
        {
            // Destroy card when hitting anything except player
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Deal damage to enemy on trigger (for trigger colliders)
            EnemyBehavior enemy = other.GetComponent<EnemyBehavior>();
            if (enemy)
                enemy.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}