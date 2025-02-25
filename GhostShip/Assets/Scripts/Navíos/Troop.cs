using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Troop : MonoBehaviour
{
    public enum TroopType { Small, Medium, Large }
    public TroopType troopType;
    public int health;
    public int damage;
    public int speed; // Velocidad de la tropa
    public int attackValue; // Valor de ataque asignado

    private BoardGenerator boardGenerator;
    private Renderer troopRenderer;
    private SpriteRenderer spriteRenderer;
    private bool isBlinking = false;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private float moveDuration = 0.5f; // Duración del movimiento
    private float moveTimer = 0f;
    public ParticleSystem cannonSmokePrefab;

    void Start()
    {
        boardGenerator = FindObjectOfType<BoardGenerator>();
        troopRenderer = GetComponent<Renderer>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Registrar la tropa en el BoardGenerator
        if (boardGenerator != null)
        {
            boardGenerator.RegisterTroop(this);
        }
    }

    void OnDestroy()
    {
        // Desregistrar la tropa del BoardGenerator cuando es destruida
        if (boardGenerator != null)
        {
            boardGenerator.UnregisterTroop(this);
        }
    }

    public void MoveAndAttack()
    {
        int x = Mathf.RoundToInt(transform.position.x / (boardGenerator.tileSize + boardGenerator.tileSpacingX));
        int y = Mathf.RoundToInt(transform.position.z / (boardGenerator.tileSize + boardGenerator.tileSpacingZ));

        // Determinar la dirección de movimiento
        int moveDirection = gameObject.CompareTag("Player1Troop") ? 1 : -1;
        int targetX = x + moveDirection;

        // Mover la tropa hacia adelante
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
                // Comprobar ataque después de moverse
                CheckForAttack();
            }
        }

        ApplyWaveMotion();
    }

    private void CheckForAttack()
    {
        int x = Mathf.RoundToInt(transform.position.x / (boardGenerator.tileSize + boardGenerator.tileSpacingX));
        int y = Mathf.RoundToInt(transform.position.z / (boardGenerator.tileSize + boardGenerator.tileSpacingZ));

        Debug.Log($"Checking for attack at position ({x}, {y})");

        // Comprobar casillas a la izquierda y derecha
        for (int dx = -1; dx <= 1; dx += 2) // -1 para izquierda, 1 para derecha
        {
            int targetX = x + dx;
            Debug.Log($"Checking tile at ({targetX}, {y})");

            // Si el valor de ataque es mayor que 0 y hay un enemigo en la casilla, realizar ataque
            if (CanMoveToTile(targetX, y, out Troop enemyTroop) && enemyTroop != null && attackValue > 0)
            {
                Debug.Log($"Attacking enemy at ({targetX}, {y})");
                // Si hay un enemigo, atacar
                AttackEnemy(enemyTroop, targetX, y);
                return;
            }
        }
    }

    void AttackEnemy(Troop enemyTroop, int targetX, int targetY)
    {
        // Instanciar el sistema de partículas (humo o efecto de disparo)
        Vector3 smokePosition = transform.position + new Vector3(0, 0.5f, 0); // Ajusta la posición según sea necesario
        ParticleSystem smoke = Instantiate(cannonSmokePrefab, smokePosition, Quaternion.identity);
        smoke.Play();

        // Iluminar brevemente la casilla atacada en rojo
        GameObject tile = boardGenerator.GetTile(targetX, targetY);
        if (tile != null)
        {
            StartCoroutine(HighlightTile(tile));
        }

        // Realizar el ataque al enemigo
        enemyTroop.Die();
    }
    bool CanMoveToTile(int x, int y, out Troop enemyTroop)
    {
        enemyTroop = null;
        GameObject tile = boardGenerator.GetTile(x, y);
        if (tile == null) return false;

        Collider[] colliders = Physics.OverlapSphere(tile.transform.position, 0.1f);
        foreach (Collider collider in colliders)
        {
            Troop foundTroop = collider.GetComponent<Troop>();
            if (foundTroop != null)
            {
                if (IsEnemy(foundTroop))  // Comprobar si es una tropa enemiga
                {
                    enemyTroop = foundTroop;
                    return true; // Hay un enemigo al que atacar
                }
                else
                {
                    return false; // No es una tropa enemiga, no atacar
                }
            }
        }
        return true; // La casilla está vacía o no hay tropas en ella
    }
    bool IsEnemy(Troop other)
    {
        // Compara si las tropas son enemigas según sus etiquetas
        return (gameObject.CompareTag("Player1Troop") && other.CompareTag("Player2Troop")) ||
               (gameObject.CompareTag("Player2Troop") && other.CompareTag("Player1Troop"));
    }

    void Die()
    {
        Destroy(gameObject);
    }

    private void ApplyWaveMotion()
    {
        // Verifica si boardGenerator está disponible y sus propiedades están inicializadas
        if (boardGenerator != null)
        {
            // Asegúrate de que las propiedades están asignadas (si es necesario, asigna valores por defecto)
            float waveFrequency = boardGenerator.waveFrequency != 0 ? boardGenerator.waveFrequency : 1.0f;
            float waveSpeed = boardGenerator.waveSpeed != 0 ? boardGenerator.waveSpeed : 1.0f;
            float waveHeight = boardGenerator.waveHeight != 0 ? boardGenerator.waveHeight : 1.0f;

            int x = Mathf.RoundToInt(transform.position.x / (boardGenerator.tileSize + boardGenerator.tileSpacingX));
            int y = Mathf.RoundToInt(transform.position.z / (boardGenerator.tileSize + boardGenerator.tileSpacingZ));

            // Calcula el desplazamiento de la onda
            float waveOffset = Mathf.Sin((x + y) * waveFrequency + Time.time * waveSpeed);

            // Aplica el desplazamiento en el eje Y
            Vector3 newPosition = transform.position;
            newPosition.y = waveOffset * waveHeight + 0.5f; // Ajusta la altura para que la tropa esté ligeramente por encima de la casilla
            transform.position = newPosition;
        }
        else
        {
            // Si boardGenerator no está disponible, simplemente no aplica el movimiento de onda
            Debug.LogWarning("BoardGenerator no encontrado. El movimiento de onda no se aplicará.");
        }
    }

    private IEnumerator HighlightTile(GameObject tile)
    {
        Renderer tileRenderer = tile.GetComponent<Renderer>();
        Color originalColor = tileRenderer.material.color;
        tileRenderer.material.color = Color.red;  // Ilumina la casilla en rojo
        yield return new WaitForSeconds(0.5f);
        tileRenderer.material.color = originalColor;  // Restaura el color original
    }
}
