using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public static BoardController instance;

    [SerializeField] private GameObject smallTroopP1Prefab;
    [SerializeField] private GameObject smallTroopP2Prefab;
    [SerializeField] private GameObject mediumTroopP1Prefab;
    [SerializeField] private GameObject mediumTroopP2Prefab;
    [SerializeField] private GameObject largeTroopP1Prefab;
    [SerializeField] private GameObject largeTroopP2Prefab;
    [SerializeField] private GameObject emptyPrefab;
    [SerializeField] private BoardState board;
    public int separacion;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        BoardRepresentation(board);

        // Iniciar el primer turno de la IA
        if (board.playerTurn == 2)
        {
            Debug.Log("Es el turno de la IA (Jugador 2).");
            
            // Buscar una casilla válida para colocar la tropa
            AIController aiController = FindObjectOfType<AIController>();
            if (aiController != null)
            {
                aiController.MakeMove();
            }

            // Mover las tropas del jugador 1 después de que la IA coloque su tropa
            MoveOpponentTroops(1); // Mover tropas del jugador 1 (enemigo de la IA)
        }
    }

    public static void CellSelected(int x, int y)
    {
        if (instance == null || instance.board == null)
        {
            Debug.LogError("BoardController o BoardState no encontrado.");
            return;
        }

        int currentPlayer = instance.board.playerTurn;
        TroopType selectedTroop = instance.board.nextTroop;

        string validationMessage;
        if (instance.IsCellValidForPlacement(x, y, currentPlayer, out validationMessage))
        {
            // Colocar la tropa en la casilla seleccionada
            instance.board.rows[x].cells[y].troop = selectedTroop;
            instance.board.rows[x].cells[y].player = currentPlayer;

            // Mover las tropas enemigas
            instance.MoveOpponentTroops(2);

            // Regenerar el tablero
            instance.RegenerateBoard();

            // Verificar si hay un ganador después de cada movimiento
            instance.CheckForWinner();

            // Cambiar al siguiente jugador
            instance.ChangePlayerTurn();
        }
        else
        {
            // Mostrar un mensaje de error si la casilla no es válida
            Debug.Log($"Casilla no válida para colocar una tropa. Razón: {validationMessage}");
        }
    }

    // Cambiar turno y actualizar la tropa para el siguiente jugador
    public void ChangePlayerTurn()
    {
        // Cambiar el turno al siguiente jugador
        board.playerTurn = (board.playerTurn == 1) ? 2 : 1;

        // Si es el turno de la IA, hacer su movimiento automáticamente
        if (board.playerTurn == 2)
        {
            Debug.Log("Es el turno de la IA (Jugador 2).");

            // Buscar una casilla válida para colocar la tropa
            AIController aiController = FindObjectOfType<AIController>();
            if (aiController != null)
            {
                aiController.MakeMove();
            }

            // Mover las tropas del jugador 1 después de que la IA coloque su tropa
            MoveOpponentTroops(1); // Mover tropas del jugador 1 (enemigo de la IA)
        }
        else
        {
            // Actualizar el tipo de tropa para la siguiente ronda
            board.UpdateNextTroop();
        }
    }

    public GameObject GetTroopPrefab(TroopType troopType, int player)
    {
        switch (troopType)
        {
            case TroopType.Small:
                return player == 1 ? smallTroopP1Prefab : smallTroopP2Prefab;
            case TroopType.Medium:
                return player == 1 ? mediumTroopP1Prefab : mediumTroopP2Prefab;
            case TroopType.Large:
                return player == 1 ? largeTroopP1Prefab : largeTroopP2Prefab;
            default:
                return emptyPrefab; // Devuelve un prefab vacío si no es válido
        }
    }

    private bool IsCellValidForPlacement(int x, int y, int player, out string validationMessage)
    {
        // Verificar si la casilla está en la primera fila del jugador
        if (player == 1 && x != 0)
        {
            validationMessage = "El Jugador 1 solo puede colocar tropas en la primera fila (fila 0).";
            return false;
        }
        else if (player == 2 && x != board.rows.Count - 1)
        {
            validationMessage = "El Jugador 2 solo puede colocar tropas en la última fila.";
            return false;
        }

        // Verificar si la casilla está vacía
        if (board.rows[x].cells[y].troop != TroopType.None)
        {
            validationMessage = "La casilla ya está ocupada por otra tropa.";
            return false;
        }

        // Si pasa todas las validaciones, la casilla es válida
        validationMessage = "Casilla válida.";
        return true;
    }

    private void MoveOpponentTroops(int currentPlayer)
    {
        Debug.Log($"Moviendo tropas del Jugador {currentPlayer}...");
        List<BoardRow> rows = board.rows;
        int direction = (currentPlayer == 1) ? -1 : 1;

        for (int i = (direction == -1 ? 1 : rows.Count - 2);
             direction == -1 ? i < rows.Count : i >= 0;
             i -= direction)
        {
            for (int j = 0; j < rows[i].cells.Count; j++)
            {
                BoardCell cell = rows[i].cells[j];

                // Verificar si la celda tiene una tropa y pertenece al jugador enemigo
                if (cell.player != 0 && cell.player != currentPlayer && cell.troop != TroopType.None)
                {
                    int newX = i + direction;
                    if (newX >= 0 && newX < rows.Count)
                    {
                        BoardCell targetCell = rows[newX].cells[j];

                        // Verificar si hay una tropa enemiga en la celda de destino
                        if (targetCell.troop != TroopType.None && targetCell.player != cell.player)
                        {
                            // Comparar el tipo de tropa
                            if (cell.troop > targetCell.troop)
                            {
                                // La tropa actual es más fuerte, eliminar la tropa enemiga
                                board.RemoveTroop(newX, j);
                            }
                            else if (cell.troop == targetCell.troop)
                            {
                                // Las tropas son del mismo tipo, no se mueve
                                continue;
                            }
                            else
                            {
                                // La tropa enemiga es más fuerte, no se mueve
                                continue;
                            }
                        }

                        // Mover la tropa a la nueva posición solo si la celda de destino está vacía
                        if (targetCell.troop == TroopType.None)
                        {
                            board.MoveTroop(i, j, newX, j);
                        }
                    }
                }
            }
        }
    }

    private void BoardRepresentation(BoardState boardToRepresent)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        List<BoardRow> rows = boardToRepresent.rows;
        for (int i = 0; i < rows.Count; i++)
        {
            for (int j = 0; j < rows[i].cells.Count; j++)
            {
                BoardRepresentationCell(i, j);
            }
        }
    }

    private void BoardRepresentationCell(int x, int y)
    {
        BoardCell cell = board.rows[x].cells[y];
        GameObject cellUnit = null;

        // Determinar la rotación basada en el jugador
        Quaternion rotation = Quaternion.identity; // Rotación por defecto (0 grados)
        if (cell.player == 2) // Jugador 2 (IA)
        {
            rotation = Quaternion.Euler(0, 180, 0); // Rotar 180 grados en el eje Y
        }

        switch (cell.troop)
        {
            case TroopType.Small:
                cellUnit = Instantiate(cell.player == 1 ? smallTroopP1Prefab : smallTroopP2Prefab,
                                      new Vector3(separacion * y, 0, separacion * x),
                                      rotation,
                                      transform);
                break;
            case TroopType.Medium:
                cellUnit = Instantiate(cell.player == 1 ? mediumTroopP1Prefab : mediumTroopP2Prefab,
                                      new Vector3(separacion * y, 0, separacion * x),
                                      rotation,
                                      transform);
                break;
            case TroopType.Large:
                cellUnit = Instantiate(cell.player == 1 ? largeTroopP1Prefab : largeTroopP2Prefab,
                                      new Vector3(separacion * y, 0, separacion * x),
                                      rotation,
                                      transform);
                break;
            case TroopType.None:
                cellUnit = Instantiate(emptyPrefab,
                                      new Vector3(separacion * y, 0, separacion * x),
                                      Quaternion.identity,
                                      transform);
                break;
        }

        if (cellUnit != null)
        {
            cellUnit.name = $"Cell_{x}_{y}";
        }
    }

    public void RegenerateBoard()
    {
        BoardRepresentation(board);
    }

    // Método para verificar si hay un ganador
    public void CheckForWinner()
    {
        board.CheckForWinner(); // Llama al método CheckForWinner de BoardState

        if (board.winner != 0)
        {
            Debug.Log($"¡Jugador {board.winner} ha ganado!");
            EndGame(board.winner);
        }
    }

    // Método para finalizar el juego
    private void EndGame(int winner)
    {
        // Aquí puedes agregar lógica para finalizar el juego, como mostrar un mensaje de victoria o deshabilitar interacciones.
        Debug.Log($"El juego ha terminado. Ganador: Jugador {winner}");
        // Por ejemplo, puedes deshabilitar el script para evitar más interacciones.
        enabled = false;
    }

    // Método para obtener el estado del tablero
    public BoardState GetBoardState()
    {
        return board;
    }
}