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
    }

    public static void CellSelected(int x, int y)
    {
        int currentPlayer = instance.board.playerTurn;
        TroopType selectedTroop = instance.board.nextTroop;

        instance.board.rows[x].cells[y].troop = selectedTroop;
        instance.board.rows[x].cells[y].player = currentPlayer;

        instance.MoveOpponentTroops(currentPlayer);

        instance.RegenerateBoard();
    }

    private void MoveOpponentTroops(int currentPlayer)
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
                if (cell.player != 0 && cell.player != currentPlayer) // Solo mover tropas enemigas
                {
                    int newX = i + direction;
                    if (newX >= 0 && newX < rows.Count)
                    {
                        // Verificamos si la nueva celda está vacía antes de mover la tropa
                        if (rows[newX].cells[j].troop == TroopType.None)
                        {
                            // Mover la tropa a la nueva posición
                            rows[newX].cells[j] = new BoardCell { troop = cell.troop, player = cell.player };
                            rows[i].cells[j] = new BoardCell { troop = TroopType.None, player = 0 };
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

        switch (cell.troop)
        {
            case TroopType.Small:
                cellUnit = Instantiate(cell.player == 1 ? smallTroopP1Prefab : smallTroopP2Prefab, new Vector3(separacion * y, 0, separacion * x), Quaternion.identity, transform);
                break;
            case TroopType.Medium:
                cellUnit = Instantiate(cell.player == 1 ? mediumTroopP1Prefab : mediumTroopP2Prefab, new Vector3(separacion * y, 0, separacion * x), Quaternion.identity, transform);
                break;
            case TroopType.Large:
                cellUnit = Instantiate(cell.player == 1 ? largeTroopP1Prefab : largeTroopP2Prefab, new Vector3(separacion * y, 0, separacion * x), Quaternion.identity, transform);
                break;
            case TroopType.None:
                cellUnit = Instantiate(emptyPrefab, new Vector3(separacion * y, 0, separacion * x), Quaternion.identity, transform);
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
}
