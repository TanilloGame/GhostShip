using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    public int width = 5; // Ancho del tablero (número de casillas en X)
    public int height = 7; // Alto del tablero (número de casillas en Z)
    public GameObject tilePrefab; // Prefab de la casilla
    public float tileSize = 1.0f; // Tamaño de cada casilla
    public float tileSpacingX = 1.2f; // Separación entre casillas en X
    public float tileSpacingZ = 1.2f; // Separación entre casillas en Z

    private GameObject[,] board; // Matriz para almacenar las casillas

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

    public GameObject GetTile(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return board[x, y];
        }
        return null;
    }
}
