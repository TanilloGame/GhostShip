using UnityEngine;

public class CasillaInteractiva : MonoBehaviour
{
    private Material materialSeleccionada;
    private Material materialNormal;

    public void asignarMaterial(Material seleccionada, Material normal)
    {
        materialSeleccionada = seleccionada;
        materialNormal = normal;
    }
}
