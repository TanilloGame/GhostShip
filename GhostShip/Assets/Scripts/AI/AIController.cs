using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    private BoardState board;
    private int aiPlayer = 2; // El jugador 2 es la IA
    private int humanPlayer = 1; // El jugador 1 es el humano

    private void Start()
    {
        board = BoardController.instance.GetBoardState();
    }

    public void MakeMove()
    {
        // Obtener el estado actual del tablero
        board = BoardController.instance.GetBoardState();

        // Llamar al algoritmo Minimax con poda Alfa-Beta (aumentamos la profundidad a 6)
        Minimax(board, 6, int.MinValue, int.MaxValue, true);

        // Colocar la tropa de la IA en el tablero
        PlaceAITroop();

        // Regenerar el tablero después de que la IA haya hecho su movimiento
        BoardController.instance.RegenerateBoard();

        // Verificar si hay un ganador después del movimiento de la IA
        BoardController.instance.CheckForWinner();

        // Cambiar al siguiente jugador (Jugador 1)
        BoardController.instance.ChangePlayerTurn();
    }

    private void PlaceAITroop()
    {
        int lastRow = board.rows.Count - 1;
        List<int> validCells = new List<int>();

        // Buscar todas las celdas vacías en la última fila
        for (int y = 0; y < board.rows[lastRow].cells.Count; y++)
        {
            if (board.rows[lastRow].cells[y].troop == TroopType.None)
            {
                validCells.Add(y);
            }
        }

        if (validCells.Count > 0)
        {
            // Seleccionar una celda aleatoria
            int selectedCell = validCells[Random.Range(0, validCells.Count)];

            // Colocar la tropa en el tablero
            board.rows[lastRow].cells[selectedCell].troop = board.nextTroop;
            board.rows[lastRow].cells[selectedCell].player = aiPlayer;

            // Instanciar la tropa correspondiente al tipo elegido por la IA
            GameObject troopPrefab = BoardController.instance.GetTroopPrefab(board.nextTroop, aiPlayer);
            Instantiate(troopPrefab, new Vector3(BoardController.instance.separacion * selectedCell, 0, BoardController.instance.separacion * lastRow), Quaternion.identity, BoardController.instance.transform);

            Debug.Log($"La IA ha colocado una tropa {board.nextTroop} en ({lastRow}, {selectedCell})");
        }
        else
        {
            Debug.Log("La IA no encontró un lugar válido para colocar una tropa.");
        }
    }

    private int Minimax(BoardState board, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        // Verificar si hemos alcanzado la profundidad máxima o si el juego ha terminado
        if (depth == 0 || board.winner != 0)
        {
            return EvaluateBoard(board);
        }

        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;
            foreach (var move in GetPossibleMoves(board, aiPlayer))
            {
                BoardState newBoard = SimulateMove(board, move);
                int eval = Minimax(newBoard, depth - 1, alpha, beta, false);
                maxEval = Mathf.Max(maxEval, eval);
                alpha = Mathf.Max(alpha, eval);
                if (beta <= alpha)
                {
                    break; // Poda Alfa-Beta
                }
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (var move in GetPossibleMoves(board, humanPlayer))
            {
                BoardState newBoard = SimulateMove(board, move);
                int eval = Minimax(newBoard, depth - 1, alpha, beta, true);
                minEval = Mathf.Min(minEval, eval);
                beta = Mathf.Min(beta, eval);
                if (beta <= alpha)
                {
                    break; // Poda Alfa-Beta
                }
            }
            return minEval;
        }
    }

    private List<Move> GetPossibleMoves(BoardState board, int player)
    {
        List<Move> possibleMoves = new List<Move>();

        // Obtener todas las casillas disponibles para el jugador actual
        int row = (player == 1) ? 0 : board.rows.Count - 1;
        for (int j = 0; j < board.rows[row].cells.Count; j++)
        {
            if (board.rows[row].cells[j].troop == TroopType.None)
            {
                possibleMoves.Add(new Move(row, j, board.nextTroop));
            }
        }

        return possibleMoves;
    }

    private BoardState SimulateMove(BoardState board, Move move)
    {
        // Crear una copia del estado del tablero
        BoardState newBoard = board.Clone();

        // Realizar el movimiento en el nuevo tablero
        newBoard.rows[move.x].cells[move.y].troop = move.troop;
        newBoard.rows[move.x].cells[move.y].player = aiPlayer;

        // Mover las tropas enemigas
        MoveOpponentTroops(newBoard, aiPlayer);

        return newBoard;
    }

    private void MoveOpponentTroops(BoardState board, int currentPlayer)
    {
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

                        // Mover la tropa a la nueva posición
                        board.MoveTroop(i, j, newX, j);
                    }
                }
            }
        }
    }

    private int EvaluateBoard(BoardState board)
    {
        int score = 0;

        // Puntuación basada en la posición de las tropas
        for (int i = 0; i < board.rows.Count; i++)
        {
            for (int j = 0; j < board.rows[i].cells.Count; j++)
            {
                BoardCell cell = board.rows[i].cells[j];

                if (cell.player == aiPlayer)
                {
                    // Calcular la distancia al objetivo del jugador 1 (fila 0)
                    int distanceToGoal = i; // Para la IA, la distancia es la fila actual (más cerca de 0 es mejor)

                    // Dar más valor a las tropas más cercanas al objetivo
                    score += (int)cell.troop * (10 - distanceToGoal); // Tropas más cercanas valen más

                    // Dar más valor a las tropas más fuertes
                    score += (int)cell.troop * 2; // Las tropas grandes valen más
                }
                else if (cell.player == humanPlayer)
                {
                    // Calcular la distancia al objetivo de la IA (última fila)
                    int distanceToGoal = (board.rows.Count - 1) - i; // Para el jugador 1, la distancia es la fila actual (más cerca de la última fila es mejor)

                    // Restar puntos por las tropas del jugador humano
                    score -= (int)cell.troop * (10 - distanceToGoal); // Tropas más cercanas al objetivo de la IA valen menos
                }
            }
        }

        // Puntuación adicional si la IA está cerca de ganar
        if (board.winner == aiPlayer)
        {
            score += 1000; // Gran recompensa por ganar
        }
        else if (board.winner == humanPlayer)
        {
            score -= 1000; // Gran penalización por perder
        }

        return score;
    }
}

public class Move
{
    public int x;
    public int y;
    public TroopType troop;

    public Move(int x, int y, TroopType troop)
    {
        this.x = x;
        this.y = y;
        this.troop = troop;
    }
}