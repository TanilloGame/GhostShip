using UnityEngine;
using DG.Tweening;

public class TileSelector : MonoBehaviour
{
    public BoardGenerator boardGenerator;
    public TurnManager turnManager;

    public GameObject[] player1Troops; // Tropas del jugador 1
    public GameObject[] player2Troops; // Tropas del jugador 2
    public Color highlightColor = Color.yellow; // Color para resaltar la casilla

    private int player1TroopIndex = 0;
    private int player2TroopIndex = 0;
    private GameObject lastHighlightedTile; // Última casilla resaltada
    private Color originalTileColor; // Color original de la casilla

    void Update()
    {
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

            int x = Mathf.RoundToInt(tile.transform.position.x / (boardGenerator.tileSize + boardGenerator.tileSpacingX));
            int y = Mathf.RoundToInt(tile.transform.position.z / (boardGenerator.tileSize + boardGenerator.tileSpacingZ));

            // Verifica si la casilla es válida para instanciar una tropa
            bool isValidTile = (turnManager.currentPlayer == TurnManager.Player.Player1 && y == 0) ||
                               (turnManager.currentPlayer == TurnManager.Player.Player2 && y == boardGenerator.height - 1);

            if (isValidTile && !IsTileOccupied(x, y))
            {
                // Resaltar la nueva casilla
                if (lastHighlightedTile != null && lastHighlightedTile != tile)
                {
                    ResetTileColor(lastHighlightedTile);
                }

                if (tile != lastHighlightedTile)
                {
                    HighlightTile(tile);
                    lastHighlightedTile = tile;
                }
            }
            else if (lastHighlightedTile != null)
            {
                // Restaurar el color de la última casilla resaltada
                ResetTileColor(lastHighlightedTile);
                lastHighlightedTile = null;
            }
        }
        else if (lastHighlightedTile != null)
        {
            // Si el ratón no está sobre ninguna casilla, restaurar el color
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

        // Verifica si la casilla es válida y no está ocupada
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
        GameObject troopPrefab;

        if (turnManager.currentPlayer == TurnManager.Player.Player1)
        {
            troopPrefab = player1Troops[player1TroopIndex];
            player1TroopIndex = (player1TroopIndex + 1) % player1Troops.Length; // Avanzar al siguiente índice
        }
        else
        {
            troopPrefab = player2Troops[player2TroopIndex];
            player2TroopIndex = (player2TroopIndex + 1) % player2Troops.Length; // Avanzar al siguiente índice
        }

        Vector3 position = new Vector3(
            x * (boardGenerator.tileSize + boardGenerator.tileSpacingX),
            0.5f,
            y * (boardGenerator.tileSize + boardGenerator.tileSpacingZ)
        );

        Quaternion rotation = (turnManager.currentPlayer == TurnManager.Player.Player1)
            ? Quaternion.identity
            : Quaternion.Euler(0, 180, 0);

        GameObject newTroop = Instantiate(troopPrefab, position, rotation);
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

        Collider[] colliders = Physics.OverlapSphere(position, 0.1f);
        foreach (Collider collider in colliders)
        {
            if (collider.GetComponent<Troop>() != null)
            {
                return true; // Hay una tropa en la casilla
            }
        }
        return false; // No hay tropas en la casilla
    }

    void HighlightTile(GameObject tile)
    {
        Renderer renderer = tile.GetComponent<Renderer>();
        if (renderer != null)
        {
            originalTileColor = renderer.material.color; // Guarda el color original
            renderer.material.color = highlightColor; // Cambia el color a resaltado
        }
    }

    void ResetTileColor(GameObject tile)
    {
        Renderer renderer = tile.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = originalTileColor; // Restaura el color original
        }
    }

    private Camera GetActiveCamera()
    {
        return turnManager.currentPlayer == TurnManager.Player.Player1
            ? turnManager.player1Camera
            : turnManager.player2Camera;
    }

    void MoveTroopsForward()
    {
        // Filtramos las tropas para obtener solo las del jugador correspondiente.
        Troop[] allTroops = FindObjectsOfType<Troop>();

        foreach (Troop troop in allTroops)
        {
            // Obtener la tropa del jugador correcto
            bool isPlayer1Troop = (turnManager.currentPlayer == TurnManager.Player.Player1 && troop.CompareTag("Player1Troop"));
            bool isPlayer2Troop = (turnManager.currentPlayer == TurnManager.Player.Player2 && troop.CompareTag("Player2Troop"));

            if (isPlayer1Troop || isPlayer2Troop)
            {
                // Obtener la posición actual de la tropa en coordenadas del tablero
                int x = Mathf.RoundToInt(troop.transform.position.x / (boardGenerator.tileSize + boardGenerator.tileSpacingX));
                int y = Mathf.RoundToInt(troop.transform.position.z / (boardGenerator.tileSize + boardGenerator.tileSpacingZ));

                // Determinar dirección de movimiento según el jugador
                int direction = (turnManager.currentPlayer == TurnManager.Player.Player1) ? 1 : -1;

                for (int i = 0; i < troop.speed; i++)
                {
                    int newY = y + direction;

                    // Verificar si la tropa puede moverse (dentro del tablero y sin colisiones)
                    if (newY >= 0 && newY < boardGenerator.height)
                    {
                        if (!IsTileOccupied(x, newY))
                        {
                            Vector3 newPosition = new Vector3(
                                x * (boardGenerator.tileSize + boardGenerator.tileSpacingX),
                                0.5f,
                                newY * (boardGenerator.tileSize + boardGenerator.tileSpacingZ)
                            );

                            // Mover usando DOTween para una animación suave
                            troop.transform.DOMove(newPosition, 0.5f).SetEase(Ease.OutQuad);
                            y = newY; // Actualizar la posición actual
                        }
                        else
                        {
                            Troop enemyTroop = GetTroopAtPosition(x, newY);
                            if (enemyTroop != null)
                            {
                                if (troop.troopType == enemyTroop.troopType)
                                {
                                    // Si son del mismo tipo, ambas tropas se quedan quietas
                                    break;
                                }
                                else
                                {
                                    // Si la tropa en frente es más poderosa, destruir la más débil
                                    if (troop.damage > enemyTroop.damage)
                                    {
                                        Destroy(enemyTroop.gameObject);
                                    }
                                    else
                                    {
                                        Destroy(troop.gameObject);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        break; // Detener el movimiento si no se puede avanzar más
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

        Collider[] colliders = Physics.OverlapSphere(position, 0.1f);
        foreach (Collider collider in colliders)
        {
            Troop troop = collider.GetComponent<Troop>();
            if (troop != null)
            {
                return troop;
            }
        }
        return null;
    }
}
