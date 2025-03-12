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

        // Llamar al algoritmo Minimax con poda Alfa-Beta
        Minimax(board, 3, int.MinValue, int.MaxValue, true);

        // Regenerar el tablero después de que la IA haya hecho su movimiento
        BoardController.instance.RegenerateBoard();

        // Verificar si hay un ganador después del movimiento de la IA
        BoardController.instance.CheckForWinner();

        // Cambiar al siguiente jugador
        BoardController.instance.ChangePlayerTurn();
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
        // Implementar la lógica para mover las tropas enemigas
        // (Similar a la función MoveOpponentTroops en BoardController)
    }

    private int EvaluateBoard(BoardState board)
    {
        // Implementar una función de evaluación que calcule la puntuación del tablero
        // para la IA. Esto puede incluir la cantidad de tropas, su posición, etc.
        int score = 0;

        // Ejemplo simple: contar las tropas de la IA y restar las del jugador humano
        foreach (var row in board.rows)
        {
            foreach (var cell in row.cells)
            {
                if (cell.player == aiPlayer)
                {
                    score += (int)cell.troop;
                }
                else if (cell.player == humanPlayer)
                {
                    score -= (int)cell.troop;
                }
            }
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