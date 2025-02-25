using System.Collections;
using UnityEngine;

public class Troop : MonoBehaviour
{
    public enum TroopType { Small, Medium, Large }
    public TroopType troopType;

    // Variables modificables desde el Inspector
    public int health;
    public float speed = 1f;
    public int damage; // Da�o del ataque
    public int attackRange = 1; // Rango de ataque (solo a izquierda y derecha)
    public ParticleSystem attackEffectPrefab; // Efecto de part�culas para el ataque
    public float highlightDuration = 0.5f; // Duraci�n del resalte de la casilla en segundos

    private BoardGenerator boardGenerator;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private float moveDuration = 0.5f; // Duraci�n del movimiento
    private float moveTimer = 0f;

    void Start()
    {
        boardGenerator = FindObjectOfType<BoardGenerator>();
        if (boardGenerator != null)
        {
            boardGenerator.RegisterTroop(this);
        }
    }

    void OnDestroy()
    {
        if (boardGenerator != null)
        {
            boardGenerator.UnregisterTroop(this);
        }
    }

    // Mueve la tropa hacia adelante y al finalizar el movimiento, chequea el ataque
    public void MoveAndAttack()
    {
        int x = Mathf.RoundToInt(transform.position.x / (boardGenerator.tileSize + boardGenerator.tileSpacingX));
        int y = Mathf.RoundToInt(transform.position.z / (boardGenerator.tileSize + boardGenerator.tileSpacingZ));

        // Direcci�n de movimiento dependiendo del jugador
        int moveDirection = gameObject.CompareTag("Player1Troop") ? 1 : -1;
        int targetX = x + moveDirection;

        MoveTo(targetX, y);
    }

    void MoveTo(int x, int y)
    {
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
                isMoving = false;
                CheckForAttack(); // Comprobar ataque despu�s de moverse
            }
        }
    }

    // Comprueba si hay tropas enemigas a la izquierda o derecha
    private void CheckForAttack()
    {
        int x = Mathf.RoundToInt(transform.position.x / (boardGenerator.tileSize + boardGenerator.tileSpacingX));
        int y = Mathf.RoundToInt(transform.position.z / (boardGenerator.tileSize + boardGenerator.tileSpacingZ));

        // Recorre las casillas a la izquierda y derecha
        for (int dx = -attackRange; dx <= attackRange; dx += (attackRange * 2)) // Solo izquierda y derecha
        {
            int targetX = x + dx;

            if (CanAttackEnemy(targetX, y))
            {
                AttackEnemy(targetX, y);
                HighlightTile(targetX, y); // Resaltar la casilla atacada
                return;
            }
        }
    }

    // Verifica si hay un enemigo en la casilla especificada
    private bool CanAttackEnemy(int x, int y)
    {
        GameObject tile = boardGenerator.GetTile(x, y);
        if (tile == null) return false;

        Collider[] colliders = Physics.OverlapSphere(tile.transform.position, 0.5f); // Aumentar el radio
        foreach (Collider collider in colliders)
        {
            Troop foundTroop = collider.GetComponent<Troop>();
            if (foundTroop != null && IsEnemy(foundTroop) && foundTroop.transform.position.z == transform.position.z) // Verificar si est� en la misma fila
            {
                return true; // Hay un enemigo al que atacar
            }
        }
        return false;
    }

    // Compara si las tropas son enemigas seg�n sus etiquetas
    private bool IsEnemy(Troop other)
    {
        return (gameObject.CompareTag("Player1Troop") && other.CompareTag("Player2Troop")) ||
               (gameObject.CompareTag("Player2Troop") && other.CompareTag("Player1Troop"));
    }

    // Realiza el ataque al enemigo y muestra efectos visuales
    private void AttackEnemy(int targetX, int targetY)
    {
        // Instanciar el sistema de part�culas de ataque
        Vector3 effectPosition = new Vector3(targetX * (boardGenerator.tileSize + boardGenerator.tileSpacingX),
                                             transform.position.y + 0.5f, // Ajusta la altura
                                             targetY * (boardGenerator.tileSize + boardGenerator.tileSpacingZ));
        ParticleSystem attackEffect = Instantiate(attackEffectPrefab, effectPosition, Quaternion.identity);
        attackEffect.Play();

        // Hacer da�o al enemigo
        GameObject tile = boardGenerator.GetTile(targetX, targetY);
        if (tile != null)
        {
            Collider[] colliders = Physics.OverlapSphere(tile.transform.position, 0.1f);
            foreach (Collider collider in colliders)
            {
                Troop foundTroop = collider.GetComponent<Troop>();
                if (foundTroop != null && IsEnemy(foundTroop))
                {
                    foundTroop.TakeDamage(damage);
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log($"{gameObject.name} ha recibido {damage} de da�o. Salud restante: {health}");
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

    // Restaura el color original despu�s de un tiempo
    private IEnumerator RestoreTileColor(Renderer tileRenderer, Color originalColor)
    {
        yield return new WaitForSeconds(highlightDuration);
        tileRenderer.material.color = originalColor; // Restauramos el color original
    }

    // Dibujar el �rea de detecci�n en la escena
    void OnDrawGizmosSelected()
    {
        // Dibujar un c�rculo que representa el �rea de detecci�n
        Gizmos.color = Color.green; // Color del �rea de detecci�n
        Gizmos.DrawWireSphere(transform.position, attackRange); // C�rculo alrededor de la tropa

        // Si quieres mostrar solo a la izquierda y derecha:
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x - attackRange, transform.position.y, transform.position.z)); // Izquierda
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + attackRange, transform.position.y, transform.position.z)); // Derecha
    }
}
