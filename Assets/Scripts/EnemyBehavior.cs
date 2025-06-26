using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyBehavior : MonoBehaviour
{
    public enum EnemyState { Patrol, Attack, Die, Chase, Stunned }
    public EnemyState currentState = EnemyState.Chase;

    [Header("Patrol Settings")]
    private Vector3[] patrolPoints;
    private int currentPatrolIndex = 0;
    private Vector3 patrolCenter;

    [Header("Attack Settings")]
    public float moveSpeed = 3f;
    public float minDistance = 2.5f;
    public int damageValue = 10;
    public float attackCooldown = 2f;
    private bool isAttacking = false;
    private float lastAttackTime = -Mathf.Infinity;

    [Header("Health & Death Settings")]
    public int health = 100;
    public int maxHealth = 100;
    public GameObject cheese;
    public GameObject apple;
    [Header("General Settings")]
    public int XPValue = 10;
    private Transform playerTransform;
    private GameObject playerObject;
    private Animator animator;
    private NavMeshAgent agent;
    private int animState;

    private bool isDying = false;
    private bool deathProcessed = false;
    private bool isStunned = false;
    private EnemyState stateBeforeStun;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject)
            playerTransform = playerObject.transform;

        patrolCenter = transform.position;
        health = maxHealth;
    }

    void Update()
    {
        if (isDying || deathProcessed) return;

        // Make enemy always face the player
        if (playerTransform)
            transform.LookAt(playerTransform);

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Attack:
                Attack();
                break;
            case EnemyState.Stunned:
                // Do nothing while stunned
                break;
            case EnemyState.Die:
                if (!isDying && !deathProcessed)
                {
                    isDying = true;
                    deathProcessed = true;
                    Die();
                }
                break;
        }
    }

    void SetupPatrolPoints()
    {
        // Create a square patrol pattern around the starting position
        patrolPoints = new Vector3[4];
        patrolPoints[0] = patrolCenter + new Vector3(-5, 0, -5);
        patrolPoints[1] = patrolCenter + new Vector3(5, 0, -5);
        patrolPoints[2] = patrolCenter + new Vector3(5, 0, 5);
        patrolPoints[3] = patrolCenter + new Vector3(-5, 0, 5);
    }

    void Patrol()
    {
        if (patrolPoints == null)
            SetupPatrolPoints();

        Vector3 targetPoint = patrolPoints[currentPatrolIndex];
        SetAnimationState(0); // 0 = Idle animation

        if (Vector3.Distance(transform.position, targetPoint) > 0.5f)
        {
            // Move towards the current patrol point at half speed
            float step = moveSpeed * 0.5f * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPoint, step);
            transform.LookAt(targetPoint);
        }
        else
        {
            // Move to next patrol point in sequence
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    void Chase()
    {
        if (!playerTransform) return;

        SetAnimationState(1); // 1 = Walk animation

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance > minDistance)
        {
            // Move to a position that's minDistance away from player
            Vector3 direction = (transform.position - playerTransform.position).normalized;
            Vector3 stopPosition = playerTransform.position + direction * minDistance;
            agent.SetDestination(stopPosition);
        }
        else
        {
            agent.ResetPath();
            // Check if player is alive before attacking
            bool playerAlive = PlayerHealth.IsAlive;
            currentState = playerAlive ? EnemyState.Attack : EnemyState.Die;
        }
    }

    void Attack()
    {
        if (isAttacking || !playerTransform) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // If player moved too far away, switch back to chase
        if (distance > minDistance)
        {
            currentState = EnemyState.Chase;
            return;
        }

        SetAnimationState(2); // 2 = Attack animation

        // Check if enough time has passed since last attack
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            isAttacking = true;

            // Deal damage to player
            PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();
            if (playerHealth)
                playerHealth.TakeDamage(damageValue);

            StartCoroutine(AttackCooldownCoroutine());
        }
    }

    void Die()
    {
        try
        {
            // Disable AI movement
            if (agent)
                agent.enabled = false;

            // Disable all colliders so enemy can't be hit anymore
            Collider[] colliders = GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }

            // Update game score
            LevelManager levelManager = FindFirstObjectByType<LevelManager>();
            if (levelManager)
                levelManager.UpdateScore(XPValue);

            // Give XP to player
            if (playerObject)
            {
                PlayerXP playerXP = playerObject.GetComponent<PlayerXP>();
                if (playerXP)
                    playerXP.TakeXP(XPValue);
            }
            int cheeseOrApple = Random.Range(1, 4);
            if (cheeseOrApple == 1)
            {
                Instantiate(cheese, transform.position, transform.rotation);
            }
            else if (cheeseOrApple == 2)
            {
                Instantiate(apple, transform.position, transform.rotation);
            }

            // Destroy enemy after brief delay
                Invoke(nameof(DestroyEnemy), 0.1f);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in Die() function: {e.Message}");
            Invoke(nameof(DestroyEnemy), 0.1f);
        }
    }

    void DestroyEnemy()
    {
        if (gameObject != null)
            Destroy(gameObject);
    }

    public void TakeDamage(int damage)
    {
        if (isDying || deathProcessed) return;

        health -= damage;

        if (health <= 0)
        {
            health = 0;
            currentState = EnemyState.Die;
        }
    }

    public void Stun(float duration)
    {
        if (isDying || deathProcessed) return;

        // Stop any existing stun coroutine
        if (isStunned)
            StopCoroutine(nameof(StunCoroutine));

        StartCoroutine(StunCoroutine(duration));
    }

    IEnumerator StunCoroutine(float duration)
    {
        if (!isStunned)
            stateBeforeStun = currentState;

        isStunned = true;
        currentState = EnemyState.Stunned;

        // Completely disable NavMeshAgent
        if (agent && agent.enabled)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }

        // Stop any attack in progress
        isAttacking = false;

        // Set stunned animation (using idle)
        SetAnimationState(0);

        yield return new WaitForSeconds(duration);

        // Re-enable NavMeshAgent and return to previous state
        if (!isDying && !deathProcessed)
        {
            if (agent && !agent.enabled)
                agent.enabled = true;

            isStunned = false;
            currentState = stateBeforeStun;
        }
    }

    private void SetAnimationState(int state)
    {
        // Only update animation if state changed and enemy isn't dying
        if (animState != state && animator && !isDying)
        {
            animState = state;
            animator.SetInteger("animState", animState);
        }
    }

    IEnumerator AttackCooldownCoroutine()
    {
        // Wait for attack animation to complete
        yield return new WaitForSeconds(1.1f);

        if (isDying || deathProcessed) yield break;

        isAttacking = false;
        SetAnimationState(0); // 0 = Idle animation

        if (!playerTransform) yield break;

        // Decide next state based on distance to player
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        currentState = distance > minDistance ? EnemyState.Chase : EnemyState.Attack;
    }
}