using System.Collections.Generic;
using UnityEngine;

public class AIControllerMC : MonoBehaviour
{
    private BoardState board;
    private int aiPlayer = 2; // El jugador 2 es la IA
    private int humanPlayer = 1; // El jugador 1 es el humano
    private int maxIterations = 1000; // Número de simulaciones de Montecarlo

    private void Start()
    {
        board = BoardController.instance.GetBoardState();
    }

    public void MakeMove()
    {
        // Obtener el estado actual del tablero
        board = BoardController.instance.GetBoardState();

        // Llamar al algoritmo Monte Carlo Tree Search (MCTS)
        MCTS(board);

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

    private void MCTS(BoardState board)
    {
        // Crear un árbol de búsqueda Monte Carlo
        Node rootNode = new Node(board, aiPlayer);

        for (int i = 0; i < maxIterations; i++)
        {
            // Seleccionar una jugada con exploración
            Node selectedNode = rootNode.SelectNode();

            // Simular el resultado de esa jugada
            int result = selectedNode.Simulate();

            // Retropropagar la evaluación hacia el nodo raíz
            selectedNode.Backpropagate(result);
        }

        // Escoger el mejor movimiento basado en el número de visitas a cada nodo
        Node bestNode = rootNode.GetBestMove();
        ApplyMove(bestNode);
    }

    private void ApplyMove(Node bestNode)
    {
        // Aplica el movimiento más favorable encontrado
        Move bestMove = bestNode.move;
        board.rows[bestMove.x].cells[bestMove.y].troop = bestMove.troop;
        board.rows[bestMove.x].cells[bestMove.y].player = aiPlayer;
        Debug.Log($"La IA ha seleccionado el movimiento en ({bestMove.x}, {bestMove.y})");
    }

    private class Node
    {
        public BoardState boardState;
        public int player;
        public List<Node> children;
        public int visits;
        public int wins;
        public Move move;

        public Node(BoardState boardState, int player)
        {
            this.boardState = boardState;
            this.player = player;
            this.children = new List<Node>();
            this.visits = 0;
            this.wins = 0;
        }

        public Node SelectNode()
        {
            // Selección del nodo utilizando el algoritmo de selección UCB1
            Node bestNode = null;
            float bestValue = float.MinValue;

            foreach (Node child in children)
            {
                float ucb1Value = child.GetUCB1Value();
                if (ucb1Value > bestValue)
                {
                    bestValue = ucb1Value;
                    bestNode = child;
                }
            }

            return bestNode;
        }

        public float GetUCB1Value()
        {
            // Cálculo del valor UCB1
            if (visits == 0)
            {
                return float.MaxValue;
            }

            return (float)wins / visits + Mathf.Sqrt(2 * Mathf.Log(visits) / visits);
        }

        public int Simulate()
        {
            // Simulación de la jugada hasta el final
            BoardState simulatedBoard = boardState.Clone();
            int currentPlayer = player;

            while (simulatedBoard.winner == 0)
            {
                List<Move> possibleMoves = GetPossibleMoves(simulatedBoard, currentPlayer);
                Move randomMove = possibleMoves[Random.Range(0, possibleMoves.Count)];
                SimulateMove(simulatedBoard, randomMove, currentPlayer);
                currentPlayer = (currentPlayer == aiPlayer) ? humanPlayer : aiPlayer;
            }

            return simulatedBoard.winner == aiPlayer ? 1 : 0; // 1 si la IA gana, 0 si pierde
        }

        public void Backpropagate(int result)
        {
            // Retropropagación del resultado hacia el nodo raíz
            visits++;
            wins += result;

            if (parent != null)
            {
                parent.Backpropagate(result);
            }
        }

        public Node GetBestMove()
        {
            // Obtener el movimiento con más visitas
            Node bestNode = null;
            int maxVisits = -1;

            foreach (Node child in children)
            {
                if (child.visits > maxVisits)
                {
                    maxVisits = child.visits;
                    bestNode = child;
                }
            }

            return bestNode;
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

    private void SimulateMove(BoardState board, Move move, int player)
    {
        // Crear una copia del estado del tablero
        BoardState newBoard = board.Clone();

        // Realizar el movimiento en el nuevo tablero
        newBoard.rows[move.x].cells[move.y].troop = move.troop;
        newBoard.rows[move.x].cells[move.y].player = player;

        // Simular la respuesta del oponente (IA o humano)
        MoveOpponentTroops(newBoard, player);
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
}
