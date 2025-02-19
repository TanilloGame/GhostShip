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

    private BoardGenerator boardGenerator;
    private Renderer troopRenderer;
    private SpriteRenderer spriteRenderer;
    private bool isBlinking = false;

    void Start()
    {
        boardGenerator = FindObjectOfType<BoardGenerator>();
        troopRenderer = GetComponent<Renderer>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void MoveAndAttack()
    {
        int x = Mathf.RoundToInt(transform.position.x / (boardGenerator.tileSize + boardGenerator.tileSpacingX));
        int y = Mathf.RoundToInt(transform.position.z / (boardGenerator.tileSize + boardGenerator.tileSpacingZ));

        int targetX = gameObject.CompareTag("Player1Troop") ? x + 1 : x - 1;

        if (CanMoveToTile(targetX, y, out Troop enemyTroop))
        {
            if (enemyTroop != null)
            {
                // Si hay un enemigo, decidir si atacarlo o no
                if (IsStrongerThan(enemyTroop))
                {
                    enemyTroop.Die();
                    MoveTo(targetX, y);
                }
            }
            else
            {
                // Si no hay enemigo, simplemente avanzar
                MoveTo(targetX, y);
            }
        }
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
                if (IsEnemy(foundTroop))
                {
                    enemyTroop = foundTroop;
                    return true; // Puede atacar si es más fuerte
                }
                else
                {
                    return false; // No se atraviesa con sus propias tropas
                }
            }
        }
        return true; // La casilla está vacía
    }

    bool IsEnemy(Troop other)
    {
        return (gameObject.CompareTag("Player1Troop") && other.CompareTag("Player2Troop")) ||
               (gameObject.CompareTag("Player2Troop") && other.CompareTag("Player1Troop"));
    }

    bool IsStrongerThan(Troop enemy)
    {
        return (troopType == TroopType.Large && enemy.troopType != TroopType.Large) ||
               (troopType == TroopType.Medium && enemy.troopType == TroopType.Small) ||
               (troopType == TroopType.Small && enemy.troopType == TroopType.Small);
    }

    void MoveTo(int x, int y)
    {
        Vector3 targetPosition = new Vector3(
            x * (boardGenerator.tileSize + boardGenerator.tileSpacingX),
            transform.position.y,
            y * (boardGenerator.tileSize + boardGenerator.tileSpacingZ)
        );
        transform.position = targetPosition;
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
