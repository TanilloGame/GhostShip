using UnityEngine;

public class BoardCellController : MonoBehaviour
{
    private void OnMouseDown()
    {
        // Verificar si BoardController.instance es nulo o si está desactivado
        if (BoardController.instance == null || !BoardController.instance.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("BoardController está desactivado o no existe. No se puede seleccionar una celda.");
            return;
        }

        int x = (int)transform.position.z / BoardController.instance.separacion;
        int y = (int)transform.position.x / BoardController.instance.separacion;

        BoardController.CellSelected(x, y);
    }
}
