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

    public void RemoveTroop(int x, int y)
    {
        // Verificar que las coordenadas estén dentro de los límites del tablero
        if (x >= 0 && x < rows.Count && y >= 0 && y < rows[x].cells.Count)
        {
            // Eliminar la tropa de la celda
            rows[x].cells[y].troop = TroopType.None;
            rows[x].cells[y].player = 0;

            Debug.Log($"Tropa eliminada en ({x}, {y}).");
        }
        else
        {
            Debug.LogError($"Coordenadas fuera de los límites del tablero: ({x}, {y}).");
        }
    }
    public void MoveTroop(int fromX, int fromY, int toX, int toY)
    {
        // Verificar que las coordenadas de origen y destino estén dentro de los límites del tablero
        if (fromX >= 0 && fromX < rows.Count && fromY >= 0 && fromY < rows[fromX].cells.Count &&
            toX >= 0 && toX < rows.Count && toY >= 0 && toY < rows[toX].cells.Count)
        {
            // Obtener la celda de origen y destino
            BoardCell fromCell = rows[fromX].cells[fromY];
            BoardCell toCell = rows[toX].cells[toY];

            // Verificar si la celda de origen tiene una tropa
            if (fromCell.troop != TroopType.None)
            {
                // Verificar si la celda de destino está vacía
                if (toCell.troop == TroopType.None)
                {
                    // Mover la tropa a la celda de destino
                    toCell.troop = fromCell.troop;
                    toCell.player = fromCell.player;

                    // Limpiar la celda de origen
                    fromCell.troop = TroopType.None;
                    fromCell.player = 0;

                    Debug.Log($"Tropa movida de ({fromX}, {fromY}) a ({toX}, {toY}).");
                }
                else
                {
                    // Si hay una tropa enemiga en la celda de destino, comparar fuerzas
                    if (fromCell.troop > toCell.troop)
                    {
                        // La tropa actual es más fuerte, eliminar la tropa enemiga
                        toCell.troop = fromCell.troop;
                        toCell.player = fromCell.player;

                        // Limpiar la celda de origen
                        fromCell.troop = TroopType.None;
                        fromCell.player = 0;

                        Debug.Log($"Tropa enemiga eliminada en ({toX}, {toY}). Tropa movida de ({fromX}, {fromY}).");
                    }
                    else if (fromCell.troop == toCell.troop)
                    {
                        // Las tropas son del mismo tipo, no se mueve
                        Debug.Log($"Las tropas son del mismo tipo en ({toX}, {toY}). No se mueve.");
                    }
                    else
                    {
                        // La tropa enemiga es más fuerte, no se mueve
                        Debug.Log($"La tropa enemiga es más fuerte en ({toX}, {toY}). No se mueve.");
                    }
                }
            }
            else
            {
                Debug.LogError($"No hay tropa en la celda de origen: ({fromX}, {fromY}).");
            }
        }
        else
        {
            Debug.LogError($"Coordenadas fuera de los límites del tablero: ({fromX}, {fromY}) o ({toX}, {toY}).");
        }
    }

}
