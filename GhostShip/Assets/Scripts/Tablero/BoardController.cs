using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    [SerializeField] private BoardState board;
    // Start is called before the first frame update
    void Start()
    {
        BoardRepresentation(board);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void BoardRepresentation(BoardState boardToRepresent)
    {

    }
}
