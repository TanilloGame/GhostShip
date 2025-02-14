using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Troop : MonoBehaviour
{
    public enum TroopType { Small, Medium, Large }
    public TroopType troopType;
    public int health;
    public int damage;

    private BoardGenerator boardGenerator;
    private List<Troop> attackers = new List<Troop>(); // Lista de tropas que ya han atacado
    private Renderer troopRenderer;
    private SpriteRenderer spriteRenderer;
    private bool isBlinking = false; // Controla si el efecto de parpadeo está activo

    void Start()
    {
        boardGenerator = FindObjectOfType<BoardGenerator>();

        troopRenderer = GetComponent<Renderer>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (troopRenderer == null && spriteRenderer == null)
        {
            Debug.LogError("No se encontró un Renderer en " + gameObject.name);
        }

        InvokeRepeating(nameof(CheckForEnemies), 1f, 1f); // Comprobar enemigos cada segundo
    }

    void CheckForEnemies()
    {
        int x = Mathf.RoundToInt(transform.position.x / (boardGenerator.tileSize + boardGenerator.tileSpacing));
        int y = Mathf.RoundToInt(transform.position.z / (boardGenerator.tileSize + boardGenerator.tileSpacing));

        CheckTileForEnemy(x - 1, y); // Izquierda
        CheckTileForEnemy(x + 1, y); // Derecha
    }

    void CheckTileForEnemy(int x, int y)
    {
        GameObject tile = boardGenerator.GetTile(x, y);
        if (tile != null)
        {
            Collider[] colliders = Physics.OverlapSphere(tile.transform.position, 0.1f);
            foreach (Collider collider in colliders)
            {
                Troop enemyTroop = collider.GetComponent<Troop>();
                if (enemyTroop != null && IsEnemy(enemyTroop) && !attackers.Contains(enemyTroop))
                {
                    Attack(enemyTroop);
                    attackers.Add(enemyTroop); // Registra que ya atacó a esta tropa
                }
            }
        }
    }

    bool IsEnemy(Troop other)
    {
        return (gameObject.CompareTag("Player1Troop") && other.CompareTag("Player2Troop")) ||
               (gameObject.CompareTag("Player2Troop") && other.CompareTag("Player1Troop"));
    }

    void Attack(Troop enemy)
    {
        enemy.TakeDamage(damage);
    }

    public void TakeDamage(int amount)
    {
        health -= amount;

        // Activa el efecto de parpadeo si no está activo ya
        if (!isBlinking)
        {
            StartCoroutine(DamageEffect());
        }

        if (health <= 0)
        {
            Die();
        }
    }

    IEnumerator DamageEffect()
    {
        isBlinking = true; // Indica que el efecto de parpadeo está activo

        for (int i = 0; i < 4; i++) // Parpadeo (4 ciclos)
        {
            // Desactiva el MeshRenderer para el parpadeo
            if (troopRenderer != null) troopRenderer.enabled = false;

            yield return new WaitForSeconds(0.1f);

            // Reactiva el MeshRenderer
            if (troopRenderer != null) troopRenderer.enabled = true;

            yield return new WaitForSeconds(0.1f);
        }

        isBlinking = false; // Indica que el efecto de parpadeo ha terminado
    }

    void Die()
    {
        Destroy(gameObject);
    }
}