using System.Collections.Generic;
using UnityEngine;

public enum TroopType { None, Small, Medium, Large };

[System.Serializable]
public class BoardCell
{
    public TroopType troop;
    public int player; // 0 si no hay jugador, 1 para Jugador 1, 2 para Jugador 2

    // Método para clonar una celda
    public BoardCell Clone()
    {
        return new BoardCell
        {
            troop = this.troop,
            player = this.player
        };
    }
}

[System.Serializable]
public class BoardRow
{
    public List<BoardCell> cells;

    // Método para clonar una fila
    public BoardRow Clone()
    {
        BoardRow clone = new BoardRow
        {
            cells = new List<BoardCell>()
        };

        foreach (var cell in this.cells)
        {
            clone.cells.Add(cell.Clone());
        }

        return clone;
    }
}

[System.Serializable]
public class BoardState
{
    public List<BoardRow> rows;

    public int playerTurn; // 1 para Jugador 1, 2 para Jugador 2
    public TroopType nextTroop; // La próxima tropa a colocar
    public int winner; // 0 si no hay ganador, 1 para Jugador 1, 2 para Jugador 2

    // Constructor para inicializar el tablero
    public BoardState(int width, int height)
    {
        rows = new List<BoardRow>();
        for (int i = 0; i < height; i++)
        {
            BoardRow row = new BoardRow();
            row.cells = new List<BoardCell>();
            for (int j = 0; j < width; j++)
            {
                row.cells.Add(new BoardCell { troop = TroopType.None, player = 0 });
            }
            rows.Add(row);
        }

        playerTurn = 1; // El Jugador 1 comienza
        nextTroop = TroopType.Small; // La primera tropa a colocar
        winner = 0; // Inicialmente no hay ganador
    }

    // Método para clonar el estado del tablero
    public BoardState Clone()
    {
        BoardState clone = new BoardState(0, 0) // Se inicializa con 0, 0 porque se sobrescribirá
        {
            playerTurn = this.playerTurn,
            nextTroop = this.nextTroop,
            winner = this.winner,
            rows = new List<BoardRow>()
        };

        foreach (var row in this.rows)
        {
            clone.rows.Add(row.Clone());
        }

        return clone;
    }

    // Método para verificar si hay un ganador
    public void CheckForWinner()
    {
        // Verificar si el Jugador 1 ha ganado (llegó a la última fila)
        for (int x = 0; x < rows[rows.Count - 1].cells.Count; x++)
        {
            if (rows[rows.Count - 1].cells[x].player == 1)
            {
                winner = 1;
                return;
            }
        }

        // Verificar si el Jugador 2 ha ganado (llegó a la primera fila)
        for (int x = 0; x < rows[0].cells.Count; x++)
        {
            if (rows[0].cells[x].player == 2)
            {
                winner = 2;
                return;
            }
        }

        // Si no hay ganador, el campo winner sigue siendo 0
    }

    // Método para actualizar el tipo de tropa para la siguiente ronda
    public void UpdateNextTroop()
    {
        switch (nextTroop)
        {
            case TroopType.Small:
                nextTroop = TroopType.Medium;
                break;
            case TroopType.Medium:
                nextTroop = TroopType.Large;
                break;
            case TroopType.Large:
                nextTroop = TroopType.Small;
                break;
        }
    }

    // Método para eliminar una tropa en una posición específica
    public void RemoveTroop(int x, int y)
    {
        if (x >= 0 && x < rows.Count && y >= 0 && y < rows[x].cells.Count)
        {
            rows[x].cells[y].troop = TroopType.None;
            rows[x].cells[y].player = 0;
        }
    }

    // Método para mover una tropa de una posición a otra
    public void MoveTroop(int fromX, int fromY, int toX, int toY)
    {
        if (fromX >= 0 && fromX < rows.Count && fromY >= 0 && fromY < rows[fromX].cells.Count &&
            toX >= 0 && toX < rows.Count && toY >= 0 && toY < rows[toX].cells.Count)
        {
            BoardCell fromCell = rows[fromX].cells[fromY];
            BoardCell toCell = rows[toX].cells[toY];

            if (fromCell.troop != TroopType.None)
            {
                if (toCell.troop == TroopType.None)
                {
                    toCell.troop = fromCell.troop;
                    toCell.player = fromCell.player;
                    fromCell.troop = TroopType.None;
                    fromCell.player = 0;
                }
                else if (toCell.player != fromCell.player)
                {
                    if (fromCell.troop > toCell.troop)
                    {
                        toCell.troop = fromCell.troop;
                        toCell.player = fromCell.player;
                        fromCell.troop = TroopType.None;
                        fromCell.player = 0;
                    }
                    else if (fromCell.troop == toCell.troop)
                    {
                        // No se mueve si son del mismo tipo
                    }
                    else
                    {
                        // No se mueve si la tropa enemiga es más fuerte
                    }
                }
            }
        }
    }

    // Método para obtener la distancia de una celda al objetivo del jugador
    public int GetDistanceToGoal(int x, int y, int player)
    {
        if (player == 1)
        {
            return x; // Distancia a la fila 0 (objetivo del jugador 1)
        }
        else if (player == 2)
        {
            return (rows.Count - 1) - x; // Distancia a la última fila (objetivo del jugador 2)
        }
        return 0;
    }

    // Método para obtener todas las tropas de un jugador
    public List<Vector2Int> GetPlayerTroops(int player)
    {
        List<Vector2Int> troops = new List<Vector2Int>();
        for (int i = 0; i < rows.Count; i++)
        {
            for (int j = 0; j < rows[i].cells.Count; j++)
            {
                if (rows[i].cells[j].player == player)
                {
                    troops.Add(new Vector2Int(i, j));
                }
            }
        }
        return troops;
    }

    // Método para obtener todas las celdas vacías en la fila de un jugador
    public List<Vector2Int> GetEmptyCellsInPlayerRow(int player)
    {
        List<Vector2Int> emptyCells = new List<Vector2Int>();
        int row = (player == 1) ? 0 : rows.Count - 1;

        for (int j = 0; j < rows[row].cells.Count; j++)
        {
            if (rows[row].cells[j].troop == TroopType.None)
            {
                emptyCells.Add(new Vector2Int(row, j));
            }
        }
        return emptyCells;
    }

    // Método para obtener la puntuación de un jugador
    public int GetPlayerScore(int player)
    {
        int score = 0;
        foreach (var row in rows)
        {
            foreach (var cell in row.cells)
            {
                if (cell.player == player)
                {
                    score += (int)cell.troop; // Sumar el valor de la tropa
                }
            }
        }
        return score;
    }
}