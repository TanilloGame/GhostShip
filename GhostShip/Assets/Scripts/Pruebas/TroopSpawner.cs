using UnityEngine;

public class TroopSpawner : MonoBehaviour
{
    public BoardGenerator boardGenerator;
    public GameObject smallTroopPrefab;
    public GameObject mediumTroopPrefab;
    public GameObject largeTroopPrefab;

    public int xPosition = 0;
    public int yPosition = 0;
    public Troop.TroopType troopType = Troop.TroopType.Small;

    [ContextMenu("Spawn Troop")]
    public void SpawnTroop()
    {
        if (boardGenerator == null)
        {
            Debug.LogError("BoardGenerator no asignado.");
            return;
        }

        if (xPosition < 0 || xPosition >= boardGenerator.width || yPosition < 0 || yPosition >= boardGenerator.height)
        {
            Debug.LogError("Posición fuera de los límites del tablero.");
            return;
        }

        GameObject troopPrefab = null;
        switch (troopType)
        {
            case Troop.TroopType.Small:
                troopPrefab = smallTroopPrefab;
                break;
            case Troop.TroopType.Medium:
                troopPrefab = mediumTroopPrefab;
                break;
            case Troop.TroopType.Large:
                troopPrefab = largeTroopPrefab;
                break;
        }

        if (troopPrefab == null)
        {
            Debug.LogError("Prefab de tropa no asignado.");
            return;
        }

        Vector3 position = new Vector3(
            xPosition * (boardGenerator.tileSize + boardGenerator.tileSpacingX),
            0.5f,
            yPosition * (boardGenerator.tileSize + boardGenerator.tileSpacingZ)
        );

        Instantiate(troopPrefab, position, Quaternion.identity);
        Debug.Log("Tropa instanciada en (" + xPosition + ", " + yPosition + ")");
    }
}
