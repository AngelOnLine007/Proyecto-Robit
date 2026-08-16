using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DropdownSiempreArriba : MonoBehaviour
{
    [Header("Configuración Visual")]
    [Tooltip("Escala de la lista al desplegarse. 1 = Tamaño normal. 0.9 = 10% más pequeño.")]
    [Range(0.5f, 2f)]
    public float escalaListaDesplegada = 1f;

    private bool yaCorregido = false;

    void Update()
    {
        // Buscamos la lista que Unity genera al hacer clic
        Transform lista = transform.Find("Dropdown List");
        
        // Si la lista aparece y no la hemos corregido en este clic
        if (lista != null && !yaCorregido)
        {
            StartCoroutine(ForzarPosicionArriba(lista.GetComponent<RectTransform>()));
            yaCorregido = true;
        }
        // Si la lista ya no existe (el usuario cerró el dropdown), reseteamos
        else if (lista == null && yaCorregido)
        {
            yaCorregido = false;
        }
    }

    private IEnumerator ForzarPosicionArriba(RectTransform listRect)
    {
        // Esperamos al final del frame para que Unity termine de crearla
        yield return new WaitForEndOfFrame();

        if (listRect != null)
        {
            // 1. Ponemos el punto de apoyo (Pivote) en la parte de abajo de la lista
            listRect.pivot = new Vector2(listRect.pivot.x, 0f);
            
            // 2. Anclamos la lista a la parte SUPERIOR del botón del Dropdown
            listRect.anchorMin = new Vector2(0f, 1f);
            listRect.anchorMax = new Vector2(1f, 1f);
            
            // 3. Posición en 0 exacto
            listRect.anchoredPosition = Vector2.zero;
            
            // --- NUEVO: Reducimos el tamaño de la lista ---
            listRect.localScale = new Vector3(escalaListaDesplegada, escalaListaDesplegada, 1f);
            
            // 4. Obligamos al motor gráfico a reconstruir el bloque visualmente
            LayoutRebuilder.ForceRebuildLayoutImmediate(listRect);
        }
    }
}