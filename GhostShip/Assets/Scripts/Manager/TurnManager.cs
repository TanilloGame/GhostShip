using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public enum Player { Player1, Player2 }
    public Player currentPlayer;
    public Camera player1Camera;
    public Camera player2Camera;
    public Text turnText; // Referencia al texto de la UI

    void Start()
    {
        currentPlayer = Player.Player1;
        SetCamera();
        UpdateTurnText();
    }

    public void EndTurn()
    {
        currentPlayer = (currentPlayer == Player.Player1) ? Player.Player2 : Player.Player1;
        SetCamera();
        UpdateTurnText();
    }

    void SetCamera()
    {
        player1Camera.gameObject.SetActive(currentPlayer == Player.Player1);
        player2Camera.gameObject.SetActive(currentPlayer == Player.Player2);
    }

    void UpdateTurnText()
    {
        if (turnText != null)
        {
            turnText.text = $"Turno: {currentPlayer}";
        }
    }
}