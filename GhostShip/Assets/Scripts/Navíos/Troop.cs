using System.Collections;
using UnityEngine;

public class Troop : MonoBehaviour
{
    public enum TroopType { Small, Medium, Large }
    public TroopType troopType;

    // Variables modificables desde el Inspector
    public int health;
    public float speed = 1f;
    public int damage; // Daño del ataque
    public int attackRange = 1; // Rango de ataque (solo a izquierda y derecha)
    public ParticleSystem attackEffectPrefab; // Efecto de partículas para el ataque
    public float highlightDuration = 0.5f; // Duración del resalte de la casilla en segundos

    private BoardGenerator boardGenerator;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private float moveDuration = 0.5f; // Duración del movimiento
    private float moveTimer = 0f;

    void Start()
    {
        Debug.Log($"{gameObject.name} inicializada.");
        boardGenerator = FindObjectOfType<BoardGenerator>();
        if (boardGenerator != null)
        {
            Debug.Log($"{gameObject.name} registrada en el BoardGenerator.");
            boardGenerator.RegisterTroop(this);
        }
        else
        {
            Debug.LogError("BoardGenerator no encontrado.");
        }
    }

    void OnDestroy()
    {
        Debug.Log($"{gameObject.name} destruida.");
        if (boardGenerator != null)
        {
            boardGenerator.UnregisterTroop(this);
        }
    }

    // Mueve la tropa hacia adelante y al finalizar el movimiento, chequea el ataque
    public void MoveAndAttack()
    {
        Debug.Log($"{gameObject.name} iniciando movimiento y ataque.");
        int x = Mathf.RoundToInt(transform.position.x / (boardGenerator.tileSize + boardGenerator.tileSpacingX));
        int y = Mathf.RoundToInt(transform.position.z / (boardGenerator.tileSize + boardGenerator.tileSpacingZ));

        // Dirección de movimiento dependiendo del jugador
        int moveDirection = gameObject.CompareTag("Player1Troop") ? 1 : -1;
        int targetX = x + moveDirection;

        Debug.Log($"{gameObject.name} moviéndose a ({targetX}, {y}).");
        MoveTo(targetX, y);
    }

    void MoveTo(int x, int y)
    {
        Debug.Log($"{gameObject.name} preparándose para moverse a ({x}, {y}).");
        targetPosition = new Vector3(
            x * (boardGenerator.tileSize + boardGenerator.tileSpacingX),
            transform.position.y,
            y * (boardGenerator.tileSize + boardGenerator.tileSpacingZ)
        );
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
                CheckForSideAttacks(); // Comprobar ataque después de moverse
            }
        }
    }

    // Comprueba si hay tropas enemigas a la izquierda o derecha
    private void CheckForSideAttacks()
    {
        Debug.Log($"{gameObject.name} verificando ataques laterales.");
        int x = Mathf.RoundToInt(transform.position.x / (boardGenerator.tileSize + boardGenerator.tileSpacingX));
        int y = Mathf.RoundToInt(transform.position.z / (boardGenerator.tileSize + boardGenerator.tileSpacingZ));

        // Verificar a la izquierda
        Debug.Log($"{gameObject.name} verificando ataque a la izquierda en ({x - 1}, {y}).");
        if (CanAttackEnemy(x - 1, y))
        {
            Debug.Log($"{gameObject.name} atacando a la izquierda.");
            AttackEnemy(x - 1, y);
            HighlightTile(x - 1, y);
        }

        // Verificar a la derecha
        Debug.Log($"{gameObject.name} verificando ataque a la derecha en ({x + 1}, {y}).");
        if (CanAttackEnemy(x + 1, y))
        {
            Debug.Log($"{gameObject.name} atacando a la derecha.");
            AttackEnemy(x + 1, y);
            HighlightTile(x + 1, y);
        }
    }

    // Verifica si hay un enemigo en la casilla especificada
    private bool CanAttackEnemy(int x, int y)
    {
        Debug.Log($"{gameObject.name} verificando si puede atacar en ({x}, {y}).");
        GameObject tile = boardGenerator.GetTile(x, y);
        if (tile == null)
        {
            Debug.Log($"{gameObject.name} no puede atacar en ({x}, {y}) porque no hay casilla.");
            return false;
        }

        Collider[] colliders = Physics.OverlapSphere(tile.transform.position, 0.5f); // Aumentar el radio
        foreach (Collider collider in colliders)
        {
            Troop foundTroop = collider.GetComponent<Troop>();
            if (foundTroop != null && IsEnemy(foundTroop) && foundTroop.transform.position.z == transform.position.z) // Verificar si está en la misma fila
            {
                Debug.Log($"{gameObject.name} encontró un enemigo en ({x}, {y}).");
                return true; // Hay un enemigo al que atacar
            }
        }
        Debug.Log($"{gameObject.name} no encontró enemigos en ({x}, {y}).");
        return false;
    }

    // Compara si las tropas son enemigas según sus etiquetas
    private bool IsEnemy(Troop other)
    {
        bool isEnemy = (gameObject.CompareTag("Player1Troop") && other.CompareTag("Player2Troop")) ||
                       (gameObject.CompareTag("Player2Troop") && other.CompareTag("Player1Troop"));
        Debug.Log($"{gameObject.name} verificando si {other.gameObject.name} es enemigo: {isEnemy}.");
        return isEnemy;
    }

    // Realiza el ataque al enemigo y muestra efectos visuales
    private void AttackEnemy(int targetX, int targetY)
    {
        Debug.Log($"{gameObject.name} atacando en ({targetX}, {targetY}).");
        // Instanciar el sistema de partículas de ataque
        Vector3 effectPosition = new Vector3(targetX * (boardGenerator.tileSize + boardGenerator.tileSpacingX),
                                             transform.position.y + 0.5f, // Ajusta la altura
                                             targetY * (boardGenerator.tileSize + boardGenerator.tileSpacingZ));
        ParticleSystem attackEffect = Instantiate(attackEffectPrefab, effectPosition, Quaternion.identity);
        attackEffect.Play();

        // Hacer daño al enemigo
        GameObject tile = boardGenerator.GetTile(targetX, targetY);
        if (tile != null)
        {
            Collider[] colliders = Physics.OverlapSphere(tile.transform.position, 0.1f);
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

    // Resalta la casilla con un color rojo durante un breve tiempo
    private void HighlightTile(int x, int y)
    {
        Debug.Log($"{gameObject.name} resaltando casilla ({x}, {y}).");
        GameObject tile = boardGenerator.GetTile(x, y);
        if (tile != null)
        {
            Renderer tileRenderer = tile.GetComponent<Renderer>();
            if (tileRenderer != null)
            {
                Color originalColor = tileRenderer.material.color; // Guardamos el color original
                tileRenderer.material.color = Color.red; // Cambiamos el color a rojo
                StartCoroutine(RestoreTileColor(tileRenderer, originalColor));
            }
        }
    }

    // Restaura el color original después de un tiempo
    private IEnumerator RestoreTileColor(Renderer tileRenderer, Color originalColor)
    {
        yield return new WaitForSeconds(highlightDuration);
        Debug.Log($"{gameObject.name} restaurando color de la casilla.");
        tileRenderer.material.color = originalColor; // Restauramos el color original
    }

    // Dibujar el área de detección en la escena
    void OnDrawGizmosSelected()
    {
        // Dibujar un círculo que representa el área de detección
        Gizmos.color = Color.green; // Color del área de detección
        Gizmos.DrawWireSphere(transform.position, attackRange); // Círculo alrededor de la tropa

        // Si quieres mostrar solo a la izquierda y derecha:
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x - attackRange, transform.position.y, transform.position.z)); // Izquierda
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + attackRange, transform.position.y, transform.position.z)); // Derecha
    }
}