using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    [SerializeField] private BoardState board;

    [Header("Estado del Tablero (Solo para Debug)")]
    [SerializeField] private List<TroopData> debugBoardState = new List<TroopData>();

    private Dictionary<(int, int), TroopData> boardState = new Dictionary<(int, int), TroopData>();

    void Update()
    {
        UpdateBoardState(); // Se ejecuta cada frame para reflejar cambios en tiempo real
    }

    private void UpdateBoardState()
    {
        bool hasChanged = false;
        Dictionary<(int, int), TroopData> newBoardState = new Dictionary<(int, int), TroopData>();
        List<TroopData> newDebugList = new List<TroopData>();

        for (int y = 0; y < board.rows.Count; y++)
        {
            for (int x = 0; x < board.rows[y].cells.Count; x++)
            {
                BoardCell cell = board.rows[y].cells[x];

                if (cell.troop != TroopType.None)
                {
                    TroopData troopData = new TroopData
                    {
                        position = new Vector2Int(x, y),
                        troopType = cell.troop,
                        playerID = cell.player
                    };

                    newBoardState[(x, y)] = troopData;
                    newDebugList.Add(troopData);

                    // Detectar cambios en la información del tablero
                    if (!boardState.ContainsKey((x, y)) || !boardState[(x, y)].Equals(troopData))
                    {
                        hasChanged = true;
                    }
                }
            }
        }

        // Si el estado del tablero ha cambiado, actualizar la información
        if (hasChanged || boardState.Count != newBoardState.Count)
        {
            boardState = newBoardState;
            debugBoardState = newDebugList;
        }
    }

    public TroopData GetTroopAtPosition(int x, int y)
    {
        if (boardState.TryGetValue((x, y), out TroopData troopData))
        {
            return troopData;
        }
        return null;
    }

    public List<TroopData> GetAllTroops()
    {
        return new List<TroopData>(boardState.Values);
    }
}

// Estructura para almacenar datos de cada tropa y hacerla visible en el Inspector
[System.Serializable]
public class TroopData
{
    public Vector2Int position;
    public TroopType troopType;
    public int playerID;

    public override bool Equals(object obj)
    {
        if (obj is TroopData other)
        {
            return position == other.position && troopType == other.troopType && playerID == other.playerID;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return position.GetHashCode() ^ troopType.GetHashCode() ^ playerID.GetHashCode();
    }

    public override string ToString()
    {
        return $"Player {playerID} - {troopType} at {position}";
    }
}
