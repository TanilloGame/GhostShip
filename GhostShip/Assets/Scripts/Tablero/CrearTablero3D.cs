using UnityEngine;

public class CrearTablero : MonoBehaviour
{
    public GameObject casillaPrefab;   // Prefab de la casilla
    public int filas = 5;              // N�mero de filas
    public int columnas = 7;           // N�mero de columnas
    public float distanciaCasilla = 2f; // Distancia entre casillas (tama�o de la cuadr�cula)

    // M�todo para crear el tablero de juego
    void Start()
    {
        // Asegurarse de que el contenedor del tablero est� en (0, 0, 0)
        transform.position = Vector3.zero;

        CrearTableroDeJuego();
    }

    // M�todo para crear la cuadr�cula
    void CrearTableroDeJuego()
    {
        for (int i = 0; i < filas; i++)  // Itera por las filas
        {
            for (int j = 0; j < columnas; j++)  // Itera por las columnas
            {
                // Calcula la posici�n para cada casilla
                Vector3 posicion = new Vector3(i * distanciaCasilla, 0, j * distanciaCasilla);

                // Instancia la casilla en la posici�n calculada
                GameObject casilla = Instantiate(casillaPrefab, posicion, Quaternion.identity, transform);

                // Asigna un nombre a cada casilla (opcional, �til para debug)
                casilla.name = "Casilla_" + i + "_" + j;

                // Aqu� puedes agregar m�s l�gica para cada casilla si es necesario (por ejemplo, informaci�n adicional)
            }
        }
    }

    // M�todo para dibujar la cuadr�cula en el Editor con Gizmos
    void OnDrawGizmos()
    {
        // Establecemos el color de los Gizmos (por ejemplo, un color gris claro)
        Gizmos.color = Color.gray;

        // Dibujamos cada casilla de la cuadr�cula
        for (int i = 0; i < filas; i++)  // Itera por las filas
        {
            for (int j = 0; j < columnas; j++)  // Itera por las columnas
            {
                // Calcula la posici�n de la casilla
                Vector3 posicion = new Vector3(i * distanciaCasilla, 0, j * distanciaCasilla);

                // Dibujamos un cubo Gizmo en la posici�n de cada casilla
                Gizmos.DrawWireCube(posicion, new Vector3(distanciaCasilla, 0.1f, distanciaCasilla));  // Puedes ajustar el tama�o del cubo
            }
        }
    }
}
