using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    public int width = 5; // Ancho del tablero (n�mero de casillas en X)
    public int height = 7; // Alto del tablero (n�mero de casillas en Z)
    public GameObject tilePrefab; // Prefab de la casilla
    public float tileSize = 1.0f; // Tama�o de cada casilla
    public float tileSpacing = 1.2f; // Separaci�n entre casillas

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
                // Calcula la posici�n con el espaciado
                Vector3 position = new Vector3(
                    x * (tileSize + tileSpacing), // Posici�n en X
                    0, // Posici�n en Y
                    y * (tileSize + tileSpacing)  // Posici�n en Z
                );

                // Instancia la casilla en la posici�n calculada
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                tile.name = $"Tile_{x}_{y}"; // Asigna un nombre �nico
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