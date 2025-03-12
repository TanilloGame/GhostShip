using System.Collections;
using UnityEngine;

public class Troop : MonoBehaviour
{
    public enum TroopType { Small, Medium, Large }
    public TroopType troopType;

    public int health;
    public float speed = 1f;
    public int damage;
    public int attackRange = 1;
    public float highlightDuration = 0.5f;
    public float particleDuration = 1f; // Duración del sistema de partículas

    private Vector3 targetPosition;
    private bool isMoving = false;
    private float moveDuration = 0.5f;
    private float moveTimer = 0f;
    private ParticleSystem leftAttackEffect;
    private ParticleSystem rightAttackEffect;

    void Start()
    {
        Debug.Log($"{gameObject.name} inicializada.");

        // Obtener los sistemas de partículas izquierdo y derecho del prefab
        ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
        if (particleSystems.Length >= 2)
        {
            leftAttackEffect = particleSystems[0];
            rightAttackEffect = particleSystems[1];
        }
        else
        {
            Debug.LogError($"{gameObject.name} no tiene los sistemas de partículas izquierdo y derecho como hijos.");
        }
    }

    void OnDestroy()
    {
        Debug.Log($"{gameObject.name} destruida.");
    }

    public void MoveAndAttack()
    {
        Debug.Log($"{gameObject.name} iniciando movimiento y ataque.");
        int x = Mathf.RoundToInt(transform.position.x);
        int y = Mathf.RoundToInt(transform.position.z);

        int moveDirection = gameObject.CompareTag("Player1Troop") ? 1 : -1;
        int targetY = y + moveDirection;

        Debug.Log($"{gameObject.name} moviéndose a ({x}, {targetY}).");
        MoveTo(x, targetY);
    }

    void MoveTo(int x, int y)
    {
        Debug.Log($"{gameObject.name} preparándose para moverse a ({x}, {y}).");
        targetPosition = new Vector3(x, transform.position.y, y);
        isMoving = true;
        moveTimer = 0f;
    }

    void Update()
    {
        if (isMoving)
        {
            moveTimer += Time.deltaTime;
            float t = moveTimer / moveDuration;
            transform.position = Vector3.Lerp(transform.position, targetPosition, t);

            if (t >= 1f)
            {
                Debug.Log($"{gameObject.name} ha terminado de moverse.");
                isMoving = false;
                CheckForSideAttacks();
            }
        }
    }

    public void CheckForSideAttacks()
    {
        Debug.Log($"{gameObject.name} verificando ataques laterales.");
        int x = Mathf.RoundToInt(transform.position.x);
        int y = Mathf.RoundToInt(transform.position.z);

        bool leftAttack = CanAttackEnemy(x - 1, y);
        bool rightAttack = CanAttackEnemy(x + 1, y);

        if (leftAttack)
        {
            Debug.Log($"{gameObject.name} atacando a la izquierda.");
            AttackEnemy(x - 1, y, true, false);
            HighlightTile(x - 1, y);
        }

        if (rightAttack)
        {
            Debug.Log($"{gameObject.name} atacando a la derecha.");
            AttackEnemy(x + 1, y, false, true);
            HighlightTile(x + 1, y);
        }
    }

    private bool CanAttackEnemy(int x, int y)
    {
        Debug.Log($"{gameObject.name} verificando si puede atacar en ({x}, {y}).");
        Collider[] colliders = Physics.OverlapSphere(new Vector3(x, transform.position.y, y), 0.5f);
        foreach (Collider collider in colliders)
        {
            Troop foundTroop = collider.GetComponent<Troop>();
            if (foundTroop != null && IsEnemy(foundTroop))
            {
                Debug.Log($"{gameObject.name} encontró un enemigo en ({x}, {y}).");
                return true;
            }
        }
        Debug.Log($"{gameObject.name} no encontró enemigos en ({x}, {y}).");
        return false;
    }

    private bool IsEnemy(Troop other)
    {
        bool isEnemy = (gameObject.CompareTag("Player1Troop") && other.CompareTag("Player2Troop")) ||
                       (gameObject.CompareTag("Player2Troop") && other.CompareTag("Player1Troop"));
        Debug.Log($"{gameObject.name} verificando si {other.gameObject.name} es enemigo: {isEnemy}.");
        return isEnemy;
    }

    private void AttackEnemy(int targetX, int targetY, bool activateLeft, bool activateRight)
    {
        Debug.Log($"{gameObject.name} atacando en ({targetX}, {targetY}).");

        // Activar los sistemas de partículas correspondientes
        if (activateLeft && leftAttackEffect != null)
        {
            leftAttackEffect.Play();
            StartCoroutine(DisableParticleSystem(leftAttackEffect));
        }

        if (activateRight && rightAttackEffect != null)
        {
            rightAttackEffect.Play();
            StartCoroutine(DisableParticleSystem(rightAttackEffect));
        }

        Collider[] colliders = Physics.OverlapSphere(new Vector3(targetX, transform.position.y, targetY), 0.5f);
        foreach (Collider collider in colliders)
        {
            Troop foundTroop = collider.GetComponent<Troop>();
            if (foundTroop != null && IsEnemy(foundTroop))
            {
                Debug.Log($"{gameObject.name} aplicando {damage} de daño a {foundTroop.gameObject.name}.");
                foundTroop.TakeDamage(damage);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"{gameObject.name} recibiendo {damage} de daño. Salud actual: {health}.");
        health -= damage;
        Debug.Log($"{gameObject.name} salud restante: {health}.");
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} ha sido destruido.");
        Destroy(gameObject);
    }

    private void HighlightTile(int x, int y)
    {
        Debug.Log($"{gameObject.name} resaltando casilla ({x}, {y}).");
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(x, 10, y), Vector3.down, out hit))
        {
            Renderer tileRenderer = hit.collider.GetComponent<Renderer>();
            if (tileRenderer != null)
            {
                Color originalColor = tileRenderer.material.color;
                tileRenderer.material.color = Color.red;
                StartCoroutine(RestoreTileColor(tileRenderer, originalColor));
            }
        }
    }

    private IEnumerator RestoreTileColor(Renderer tileRenderer, Color originalColor)
    {
        yield return new WaitForSeconds(highlightDuration);
        Debug.Log($"{gameObject.name} restaurando color de la casilla.");
        tileRenderer.material.color = originalColor;
    }

    private IEnumerator DisableParticleSystem(ParticleSystem particleSystem)
    {
        yield return new WaitForSeconds(particleDuration);
        if (particleSystem != null)
        {
            particleSystem.Stop();
        }
    }
}
