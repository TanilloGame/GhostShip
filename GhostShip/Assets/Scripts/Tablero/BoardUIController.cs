using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoardUIController : MonoBehaviour
{
    [SerializeField] private BoardState board;
    [SerializeField] private TextMeshProUGUI boardInfoText;
    [SerializeField] private RectTransform boardImage;
    [SerializeField] private GameObject smallTroopImage;
    [SerializeField] private GameObject mediumTroopImage;
    [SerializeField] private GameObject largeTroopImage;
    [SerializeField] private Color player1Color = Color.blue;
    [SerializeField] private Color player2Color = Color.red;

    private Dictionary<(int, int), GameObject> troopImages = new Dictionary<(int, int), GameObject>();

    void Start()
    {
        UpdateBoardUI(board);
    }

    void Update()
    {
        // Aquí puedes agregar lógica para actualizar la UI en tiempo real si es necesario
    }

    public void UpdateBoardUI(BoardState boardToRepresent)
    {
        // Limpiar la representación anterior
        foreach (var troopImage in troopImages.Values)
        {
            Destroy(troopImage);
        }
        troopImages.Clear();

        // Actualizar el texto con la información del tablero
        boardInfoText.text = $"Turno del Jugador: {boardToRepresent.playerTurn}\nPróxima Tropa: {boardToRepresent.nextTroop}";

        // Representar el tablero
        for (int y = 0; y < boardToRepresent.rows.Count; y++)
        {
            for (int x = 0; x < boardToRepresent.rows[y].cells.Count; x++)
            {
                BoardCell cell = boardToRepresent.rows[y].cells[x];
                Vector3 position = new Vector3(x * 50, -y * 50, 0); // Ajusta la posición según tus necesidades

                // Representar la tropa si existe
                if (cell.troop != TroopType.None)
                {
                    GameObject troopImage = null;
                    switch (cell.troop)
                    {
                        case TroopType.Small:
                            troopImage = Instantiate(smallTroopImage, position, Quaternion.identity, boardImage);
                            break;
                        case TroopType.Medium:
                            troopImage = Instantiate(mediumTroopImage, position, Quaternion.identity, boardImage);
                            break;
                        case TroopType.Large:
                            troopImage = Instantiate(largeTroopImage, position, Quaternion.identity, boardImage);
                            break;
                    }

                    if (troopImage != null)
                    {
                        // Asignar el color según el jugador
                        Image troopImageComponent = troopImage.GetComponent<Image>();
                        if (troopImageComponent != null)
                        {
                            troopImageComponent.color = cell.player == 1 ? player1Color : player2Color;
                        }

                        troopImages[(x, y)] = troopImage;
                    }
                }
            }
        }
    }
}
