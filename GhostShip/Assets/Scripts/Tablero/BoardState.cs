using System.Collections;
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

    private int turnsPlayedInCurrentRound = 0; // Contador de turnos jugados en la ronda actual

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
            turnsPlayedInCurrentRound = this.turnsPlayedInCurrentRound,
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

    // Método para verificar si una casilla es válida para colocar una tropa
    public bool IsCellValidForPlacement(int x, int y, int player)
    {
        // Verificar si la casilla está en la primera fila del jugador y está vacía
        if (player == 1 && x == 0 && rows[x].cells[y].troop == TroopType.None)
        {
            return true;
        }
        else if (player == 2 && x == rows.Count - 1 && rows[x].cells[y].troop == TroopType.None)
        {
            return true;
        }
        return false;
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
            // Copiar la tropa a la nueva posición
            rows[toX].cells[toY].troop = rows[fromX].cells[fromY].troop;
            rows[toX].cells[toY].player = rows[fromX].cells[fromY].player;

            // Limpiar la posición anterior
            rows[fromX].cells[fromY].troop = TroopType.None;
            rows[fromX].cells[fromY].player = 0;
        }
    }

    // Método para cambiar al siguiente turno y actualizar el tipo de tropa
    public void NextTurn()
    {
        // Cambiar el turno entre los jugadores (1 y 2)
        playerTurn = (playerTurn == 1) ? 2 : 1;

        // Incrementar el contador de turnos jugados en la ronda actual
        turnsPlayedInCurrentRound++;

        // Si ambos jugadores han jugado su turno en la ronda actual, cambiar el tipo de tropa
        if (turnsPlayedInCurrentRound >= 2)
        {
            UpdateNextTroop();
            turnsPlayedInCurrentRound = 0; // Reiniciar el contador de turnos
        }
    }

    // Método para actualizar el tipo de tropa para la siguiente ronda
    private void UpdateNextTroop()
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
}