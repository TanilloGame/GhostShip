using UnityEngine;

public class BoardCellController : MonoBehaviour
{
    private void OnMouseDown()
    {
        int x = (int)transform.position.z / BoardController.instance.separacion;
        int y = (int)transform.position.x / BoardController.instance.separacion;

        BoardController.CellSelected(x, y);

    }
}