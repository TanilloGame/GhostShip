using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    public int width = 5; // Ancho del tablero (número de casillas en X)
    public int height = 7; // Alto del tablero (número de casillas en Z)
    public GameObject tilePrefab; // Prefab de la casilla
    public float tileSize = 1.0f; // Tamaño de cada casilla
    public float tileSpacingX = 1.2f; // Separación entre casillas en X
    public float tileSpacingZ = 1.2f; // Separación entre casillas en Z

    [Header("Animación de Onda")]
    public float waveSpeed = 1.0f; // Velocidad de la onda
    public float waveFrequency = 1.0f; // Frecuencia de la onda
    public float waveHeight = 0.5f; // Altura máxima de la onda (amplitud del movimiento)

    private GameObject[,] board; // Matriz para almacenar las casillas
    private List<Troop> troops = new List<Troop>(); // Lista para almacenar las tropas

    void Start()
    {
        GenerateBoard();
    }

    void GenerateBoard()
    {
        board = new GameObject[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Calcula la posición con el espaciado
                Vector3 position = new Vector3(
                    x * (tileSize + tileSpacingX), // Posición en X
                    0, // Posición en Y
                    y * (tileSize + tileSpacingZ)  // Posición en Z
                );

                // Instancia la casilla en la posición calculada
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                tile.name = $"Tile_{x}_{y}"; // Asigna un nombre único
                board[x, y] = tile; // Almacena la casilla en la matriz
            }
        }
    }

    void Update()
    {
        AnimateBoard();
        UpdateTroopPositions();
    }

    void AnimateBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject tile = board[x, y];
                Vector3 originalPosition = tile.transform.position;

                // Calcula el desplazamiento de la onda
                float waveOffset = Mathf.Sin((x + y) * waveFrequency + Time.time * waveSpeed);

                // Aplica el desplazamiento en el eje Y
                Vector3 newPosition = originalPosition;
                newPosition.y = waveOffset * waveHeight;

                // Actualiza la posición de la casilla
                tile.transform.position = newPosition;
            }
        }
    }

    public GameObject GetTile(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return board[x, y];
        }
        return null;
    }

    public void RegisterTroop(Troop troop)
    {
        troops.Add(troop);
    }

    public void UnregisterTroop(Troop troop)
    {
        troops.Remove(troop);
    }

    private void UpdateTroopPositions()
    {
        foreach (var troop in troops)
        {
            if (troop != null)
            {
                int x = Mathf.RoundToInt(troop.transform.position.x / (tileSize + tileSpacingX));
                int y = Mathf.RoundToInt(troop.transform.position.z / (tileSize + tileSpacingZ));

                GameObject tile = GetTile(x, y);
                if (tile != null)
                {
                    Vector3 newPosition = tile.transform.position;
                    newPosition.y += 0.5f; // Ajusta la altura para que la tropa esté ligeramente por encima de la casilla
                    troop.transform.position = newPosition;
                }
            }
        }
    }
}
