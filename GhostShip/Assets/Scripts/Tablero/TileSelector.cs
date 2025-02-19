using UnityEngine;
using DG.Tweening;
using System.Linq;

public class TileSelector : MonoBehaviour
{
    public BoardGenerator boardGenerator;
    public TurnManager turnManager;

    public GameObject[] player1Troops;
    public GameObject[] player2Troops;
    public Color highlightColor = Color.yellow;

    private int player1TroopIndex = 0;
    private int player2TroopIndex = 0;
    private GameObject lastHighlightedTile;
    private Color originalTileColor;

    void Update()
    {
        if (boardGenerator == null || turnManager == null) return;

        HandleMouseHover();
        HandleMouseClick();
    }

    void HandleMouseHover()
    {
        Camera activeCamera = GetActiveCamera();
        Ray ray = activeCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject tile = hit.collider.gameObject;

            if (!tile.CompareTag("Tile")) return; // Asegurar que sea una casilla válida

            int x = Mathf.RoundToInt(tile.transform.position.x / (boardGenerator.tileSize + boardGenerator.tileSpacingX));
            int y = Mathf.RoundToInt(tile.transform.position.z / (boardGenerator.tileSize + boardGenerator.tileSpacingZ));

            bool isValidTile = (turnManager.currentPlayer == TurnManager.Player.Player1 && y == 0) ||
                               (turnManager.currentPlayer == TurnManager.Player.Player2 && y == boardGenerator.height - 1);

            if (isValidTile && !IsTileOccupied(x, y))
            {
                if (lastHighlightedTile != tile)
                {
                    ResetTileColor(lastHighlightedTile);
                    HighlightTile(tile);
                    lastHighlightedTile = tile;
                }
            }
            else
            {
                ResetTileColor(lastHighlightedTile);
                lastHighlightedTile = null;
            }
        }
        else
        {
            ResetTileColor(lastHighlightedTile);
            lastHighlightedTile = null;
        }
    }

    void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0) && lastHighlightedTile != null)
        {
            TryPlaceTroop(lastHighlightedTile);
        }
    }

    void TryPlaceTroop(GameObject tile)
    {
        int x = Mathf.RoundToInt(tile.transform.position.x / (boardGenerator.tileSize + boardGenerator.tileSpacingX));
        int y = Mathf.RoundToInt(tile.transform.position.z / (boardGenerator.tileSize + boardGenerator.tileSpacingZ));

        if ((turnManager.currentPlayer == TurnManager.Player.Player1 && y == 0) ||
            (turnManager.currentPlayer == TurnManager.Player.Player2 && y == boardGenerator.height - 1))
        {
            if (!IsTileOccupied(x, y))
            {
                PlaceTroop(x, y);
            }
            else
            {
                Debug.Log("Casilla ocupada. No se puede colocar una tropa aquí.");
            }
        }
    }

    void PlaceTroop(int x, int y)
    {
        GameObject troopPrefab = (turnManager.currentPlayer == TurnManager.Player.Player1)
            ? player1Troops[player1TroopIndex]
            : player2Troops[player2TroopIndex];

        if (turnManager.currentPlayer == TurnManager.Player.Player1)
        {
            player1TroopIndex = (player1TroopIndex + 1) % player1Troops.Length;
        }
        else
        {
            player2TroopIndex = (player2TroopIndex + 1) % player2Troops.Length;
        }

        Vector3 position = new Vector3(
            x * (boardGenerator.tileSize + boardGenerator.tileSpacingX),
            0.5f,
            y * (boardGenerator.tileSize + boardGenerator.tileSpacingZ)
        );

        Quaternion rotation = (turnManager.currentPlayer == TurnManager.Player.Player1)
            ? Quaternion.identity
            : Quaternion.Euler(0, 180, 0);

        Instantiate(troopPrefab, position, rotation);
        turnManager.EndTurn();

        MoveTroopsForward();
    }

    bool IsTileOccupied(int x, int y)
    {
        Vector3 position = new Vector3(
            x * (boardGenerator.tileSize + boardGenerator.tileSpacingX),
            0.5f,
            y * (boardGenerator.tileSize + boardGenerator.tileSpacingZ)
        );

        return Physics.OverlapSphere(position, 0.1f).Any(collider => collider.GetComponent<Troop>() != null);
    }

    void HighlightTile(GameObject tile)
    {
        if (tile == null) return;
        Renderer renderer = tile.GetComponent<Renderer>();
        if (renderer != null)
        {
            originalTileColor = renderer.material.color;
            renderer.material.color = highlightColor;
        }
    }

    void ResetTileColor(GameObject tile)
    {
        if (tile == null) return;
        Renderer renderer = tile.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = originalTileColor;
        }
    }

    private Camera GetActiveCamera()
    {
        return (turnManager.currentPlayer == TurnManager.Player.Player1)
            ? turnManager.player1Camera
            : turnManager.player2Camera;
    }

    void MoveTroopsForward()
    {
        Troop[] allTroops = FindObjectsOfType<Troop>()
            .OrderBy(t => t.transform.position.z).ToArray();

        foreach (Troop troop in allTroops)
        {
            if ((turnManager.currentPlayer == TurnManager.Player.Player1 && troop.CompareTag("Player1Troop")) ||
                (turnManager.currentPlayer == TurnManager.Player.Player2 && troop.CompareTag("Player2Troop")))
            {
                int x = Mathf.RoundToInt(troop.transform.position.x / (boardGenerator.tileSize + boardGenerator.tileSpacingX));
                int y = Mathf.RoundToInt(troop.transform.position.z / (boardGenerator.tileSize + boardGenerator.tileSpacingZ));

                int direction = (turnManager.currentPlayer == TurnManager.Player.Player1) ? 1 : -1;

                for (int i = 0; i < troop.speed; i++)
                {
                    int newY = y + direction;

                    if (newY >= 0 && newY < boardGenerator.height)
                    {
                        Troop enemyTroop = GetTroopAtPosition(x, newY);
                        if (enemyTroop != null)
                        {
                            if (IsEnemy(troop, enemyTroop))
                            {
                                if (troop.troopType > enemyTroop.troopType)
                                {
                                    Destroy(enemyTroop.gameObject);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        Vector3 newPosition = new Vector3(
                            x * (boardGenerator.tileSize + boardGenerator.tileSpacingX),
                            0.5f,
                            newY * (boardGenerator.tileSize + boardGenerator.tileSpacingZ)
                        );

                        troop.transform.DOMove(newPosition, 0.5f).SetEase(Ease.OutQuad);
                        y = newY;
                    }
                }
            }
        }
    }

    Troop GetTroopAtPosition(int x, int y)
    {
        Vector3 position = new Vector3(
            x * (boardGenerator.tileSize + boardGenerator.tileSpacingX),
            0.5f,
            y * (boardGenerator.tileSize + boardGenerator.tileSpacingZ)
        );

        return Physics.OverlapSphere(position, 0.1f)
            .Select(collider => collider.GetComponent<Troop>())
            .FirstOrDefault(t => t != null);
    }

    bool IsEnemy(Troop troop, Troop other)
    {
        return troop.CompareTag("Player1Troop") != other.CompareTag("Player1Troop");
    }
}
