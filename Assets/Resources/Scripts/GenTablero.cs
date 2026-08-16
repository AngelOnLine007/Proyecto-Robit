using UnityEngine;

public class GeneradorTablero : MonoBehaviour
{
    [Header("Dimensiones del Tablero")]
    public int columnas = 5;
    public int filas = 5;
    
    [Tooltip("Debe ser igual a la 'Distancia Movimiento' de ROBIT")]
    public float tamañoCasilla = 1f; 

    [Header("Apariencia")]
    [Tooltip("Arrastra aquí el sprite 'Square' por defecto de Unity")]
    public Sprite spriteCuadrado;
    public Color colorCasilla = new Color(1f, 1f, 1f, 0.5f); // Blanco semitransparente

    // Este atributo crea una opción en el menú del componente en el Inspector
    [ContextMenu("Generar Cuadricula Ahora")]
    public void GenerarCuadricula()
    {
        // 1. Limpiamos el tablero anterior. 
        // En el modo edición, Unity requiere usar DestroyImmediate en lugar de Destroy
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        // 2. Generamos la nueva cuadrícula
        for (int x = 0; x < columnas; x++)
        {
            for (int y = 0; y < filas; y++)
            {
                GameObject casilla = new GameObject($"Casilla_{x}_{y}");
                casilla.transform.SetParent(this.transform);

                // Usamos localPosition para que la cuadrícula se mueva junto con el objeto padre
                casilla.transform.localPosition = new Vector3(x * tamañoCasilla, y * tamañoCasilla, 0);

                SpriteRenderer sr = casilla.AddComponent<SpriteRenderer>();
                sr.sprite = spriteCuadrado;
                sr.color = colorCasilla;
                sr.sortingOrder = 6; 

                float escalaVisual = tamañoCasilla * 0.95f; 
                casilla.transform.localScale = new Vector3(escalaVisual, escalaVisual, 1f);
            }
        }
    }
}