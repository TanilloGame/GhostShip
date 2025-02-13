using UnityEngine;

public class CrearTablero : MonoBehaviour
{
    public GameObject casillaPrefab;   // Prefab de la casilla
    public int filas = 5;              // Número de filas
    public int columnas = 7;           // Número de columnas
    public float distanciaCasilla = 2f; // Distancia entre casillas (tamaño de la cuadrícula)

    // Método para crear el tablero de juego
    void Start()
    {
        // Asegurarse de que el contenedor del tablero esté en (0, 0, 0)
        transform.position = Vector3.zero;

        CrearTableroDeJuego();
    }

    // Método para crear la cuadrícula
    void CrearTableroDeJuego()
    {
        for (int i = 0; i < filas; i++)  // Itera por las filas
        {
            for (int j = 0; j < columnas; j++)  // Itera por las columnas
            {
                // Calcula la posición para cada casilla
                Vector3 posicion = new Vector3(i * distanciaCasilla, 0, j * distanciaCasilla);

                // Instancia la casilla en la posición calculada
                GameObject casilla = Instantiate(casillaPrefab, posicion, Quaternion.identity, transform);

                // Asigna un nombre a cada casilla (opcional, útil para debug)
                casilla.name = "Casilla_" + i + "_" + j;

                // Aquí puedes agregar más lógica para cada casilla si es necesario (por ejemplo, información adicional)
            }
        }
    }

    // Método para dibujar la cuadrícula en el Editor con Gizmos
    void OnDrawGizmos()
    {
        // Establecemos el color de los Gizmos (por ejemplo, un color gris claro)
        Gizmos.color = Color.gray;

        // Dibujamos cada casilla de la cuadrícula
        for (int i = 0; i < filas; i++)  // Itera por las filas
        {
            for (int j = 0; j < columnas; j++)  // Itera por las columnas
            {
                // Calcula la posición de la casilla
                Vector3 posicion = new Vector3(i * distanciaCasilla, 0, j * distanciaCasilla);

                // Dibujamos un cubo Gizmo en la posición de cada casilla
                Gizmos.DrawWireCube(posicion, new Vector3(distanciaCasilla, 0.1f, distanciaCasilla));  // Puedes ajustar el tamaño del cubo
            }
        }
    }
}
