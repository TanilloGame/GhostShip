using UnityEngine;

public class SeleccionarCasilla : MonoBehaviour
{
    public GameObject barcoPrefab;   // Prefab del barco
    public Material materialNormal;  // Material de la casilla por defecto
    public Material materialSeleccionada; // Material cuando la casilla es seleccionada
    public int filas = 5;            // Número de filas
    public int columnas = 7;         // Número de columnas
    public float distanciaCasilla = 2f; // Distancia entre casillas (tamaño de la cuadrícula)

    private GameObject casillaSeleccionada = null; // Casilla seleccionada
    private Renderer rendererSeleccionada;  // Renderer de la casilla seleccionada

    void Start()
    {
        // Asegurarse de que el contenedor del tablero esté en (0, 0, 0)
        transform.position = Vector3.zero;
        CrearTableroDeJuego();
    }

    void Update()
    {
        DetectarCasillaSeleccionada();
        InstanciarBarco();
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
                GameObject casilla = new GameObject("Casilla_" + i + "_" + j); // Cambié a GameObject vacío
                casilla.transform.position = posicion;
                casilla.transform.parent = transform;

                // Agregamos un MeshRenderer para hacer la casilla visible
                MeshRenderer renderer = casilla.AddComponent<MeshRenderer>();
                MeshFilter filter = casilla.AddComponent<MeshFilter>();
                filter.mesh = CreateQuad(); // Creamos un cuadrado 2D como casilla
                renderer.material = materialNormal; // Material normal de la casilla

                // Colocamos un Collider para que detecte los clics
                BoxCollider collider = casilla.AddComponent<BoxCollider>();
                collider.size = new Vector3(distanciaCasilla, 1, distanciaCasilla);

                // Añadimos un tag para la casilla
                casilla.tag = "Casilla";

                // Agregar un script para que la casilla sea interactiva
                casilla.AddComponent<CasillaInteractiva>().asignarMaterial(materialSeleccionada, materialNormal);
            }
        }
    }

    // Crear un cuadrado simple 2D para representar la casilla
    Mesh CreateQuad()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(1, 0, 1),
            new Vector3(0, 0, 1),
        };

        mesh.triangles = new int[]
        {
            0, 1, 2,
            0, 2, 3
        };

        mesh.RecalculateNormals();
        return mesh;
    }

    // Detecta si el ratón pasa sobre una casilla y cambia su color
    void DetectarCasillaSeleccionada()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Verifica si el raycast ha tocado una casilla en la primera fila
            if (hit.collider != null && hit.collider.CompareTag("Casilla") && hit.collider.transform.position.z == 0)
            {
                GameObject casilla = hit.collider.gameObject;

                if (casilla != casillaSeleccionada)
                {
                    // Si el ratón pasa sobre una nueva casilla, revertimos el color de la anterior y cambiamos la nueva
                    if (casillaSeleccionada != null)
                    {
                        // Verificar que el Renderer de la casilla seleccionada no sea nulo
                        if (rendererSeleccionada != null)
                        {
                            rendererSeleccionada.material = materialNormal;
                        }
                    }
                    casillaSeleccionada = casilla;
                    rendererSeleccionada = casillaSeleccionada.GetComponent<Renderer>();
                    rendererSeleccionada.material = materialSeleccionada; // Cambiar color
                }
            }
        }
    }

    // Instancia el barco al hacer clic sobre la casilla seleccionada
    void InstanciarBarco()
    {
        if (Input.GetMouseButtonDown(0) && casillaSeleccionada != null)
        {
            Vector3 posicionBarco = casillaSeleccionada.transform.position;
            Instantiate(barcoPrefab, posicionBarco + new Vector3(0, 0.5f, 0), Quaternion.identity);
            casillaSeleccionada.GetComponent<Renderer>().material = materialNormal; // Revertir color
            casillaSeleccionada = null;  // Resetear casilla seleccionada
        }
    }
}
