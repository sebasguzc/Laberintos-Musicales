using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Objetivo (Jugador)")]
    public Transform player;

    [Header("Audio de Tensión (Arrastrar aquí)")]
    public EnemyTensionSynth tensionSynth; // Casilla para arrastrar el script de sonido

    [Header("Parámetros de Patrullaje")]
    public float patrolSpeed = 1.5f;
    public float wanderRadius = 3f;
    public float waitTimeAtPoint = 2f;

    [Header("Parámetros de Detección (Proximidad)")]
    public float chaseSpeed = 3f;
    public float detectionRadius = 4f;
    public float stopChaseRadius = 6f;

    [Header("Parámetros de Daño")]
    public int damageAmount = 10;
    public float damageCooldown = 1f;

    // Variables privadas de control interno
    private Vector2 startPosition;
    private Vector2 targetPatrolPoint;
    private float waitTimer;
    private float lastDamageTime;
    private Rigidbody2D rb;
    private bool isChasing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 0f;

        startPosition = transform.position;
        GetNewPatrolPoint();

        // Si no se asignó en el Inspector, busca el script en este mismo GameObject
        if (tensionSynth == null)
        {
            tensionSynth = GetComponent<EnemyTensionSynth>();
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void Update()
    {
        // Busca al personaje clonado que tenga el Tag "Player" y que esté activo en la escena
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            return;
        }

        // Distancia real entre la posición del enemigo y la posición del personaje en movimiento
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isChasing && distanceToPlayer <= detectionRadius)
        {
            isChasing = true;
            if (tensionSynth != null)
            {
                tensionSynth.SendMessage("StartTension", SendMessageOptions.DontRequireReceiver);
            }
        }
        else if (isChasing && distanceToPlayer > stopChaseRadius)
        {
            isChasing = false;
            if (tensionSynth != null)
            {
                tensionSynth.SendMessage("StopTension", SendMessageOptions.DontRequireReceiver);
            }
            GetNewPatrolPoint();
        }
    }

    void FixedUpdate()
    {
        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    // Métodos para evitar errores de llamada desde otros componentes
    public void UpdateBehavior() { }
    public void UpdateBehavior(object arg) { }

    private void Patrol()
    {
        float distanceToPoint = Vector2.Distance(transform.position, targetPatrolPoint);

        if (distanceToPoint > 0.2f)
        {
            Vector2 direction = (targetPatrolPoint - (Vector2)transform.position).normalized;
            rb.MovePosition(rb.position + direction * patrolSpeed * Time.fixedDeltaTime);
        }
        else
        {
            waitTimer += Time.fixedDeltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                GetNewPatrolPoint();
                waitTimer = 0f;
            }
        }
    }

    private void GetNewPatrolPoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        targetPatrolPoint = startPosition + randomCircle;
    }

    private void ChasePlayer()
    {
        if (player == null) return;

        // Normaliza la dirección para que el movimiento sea constante e irrotacional
        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        rb.MovePosition(rb.position + direction * chaseSpeed * Time.fixedDeltaTime);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                collision.gameObject.SendMessage("TakeDamage", damageAmount, SendMessageOptions.DontRequireReceiver);
                Debug.Log($"¡Daño infligido al jugador!: {damageAmount}");
                lastDamageTime = Time.time;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? (Vector3)startPosition : transform.position;
        Gizmos.DrawWireSphere(center, wanderRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stopChaseRadius);
    }
}