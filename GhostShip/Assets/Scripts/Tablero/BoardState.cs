using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TroopType { None, Small, Medium, Large };

[System.Serializable]
public class BoardCell 
{
    

    public TroopType troop;
    public int player; 

}
[System.Serializable]
public class BoardRow
{
    public List<BoardCell> cells;
}

[System.Serializable]
public class BoardState
{
    public List<BoardRow> rows;

    public int playerTurn;

    public TroopType nextTroop;

   
}
