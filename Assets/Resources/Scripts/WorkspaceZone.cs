using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Image))] 
public class WorkspaceZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler //
{
    [Header("Configuración de Zona")]
    public bool esZonaPrincipal = true; //

    [Header("Límites de Capacidad")]
    [Tooltip("Activa esta casilla para limitar la cantidad máxima de bloques que se pueden colocar aquí.")]
    public bool limitarCapacidad = false;
    
    [Tooltip("Cantidad máxima de bloques permitidos si el límite está activado.")]
    public int capacidadMaximaBloques = 10;

    [Header("Escalado Dinámico de Bloques")]
    [Tooltip("Tamaño por defecto de los bloques al entrar en esta dropzone.")]
    [Range(0.1f, 4f)]
    public float escalaBaseBloques = 4f;

    [Tooltip("Cantidad de bloques antes de empezar a reducir su tamaño.")]
    public int limiteBloquesNormales = 6;
    
    [Tooltip("Porcentaje que se reduce por cada bloque extra (ej: 0.05 = 5% más pequeños).")]
    [Range(0.01f, 0.5f)]
    public float reduccionPorBloqueExtra = 0.28f;
    
    [Tooltip("Tamaño mínimo que pueden alcanzar los bloques para que sigan siendo legibles.")]
    [Range(0.1f, 2f)]
    public float escalaMinima = 1.8f;

    [Header("Feedback Visual (Opcional)")]
    [Tooltip("Activa esto solo si quieres que esta zona específica cambie de color al pasar un bloque por encima. Para la Dropzone y el contenedor interno se recomienda dejarlo desactivado.")] //
    public bool usarResaltadoVisual = false; //[cite: 1]
    public Color colorResaltado = new Color(0.5f, 1f, 0.5f, 0.4f); //[cite: 1]

    private RectTransform rectTransform; //[cite: 1]
    private HorizontalLayoutGroup layoutGroup; //[cite: 1]
    private Image fondoZona; //[cite: 1]
    private Color colorOriginalDiseno; // Guarda el color exacto que configuraste en el Inspector[cite: 1]

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>(); //[cite: 1]
        layoutGroup = GetComponent<HorizontalLayoutGroup>(); //[cite: 1]
        fondoZona = GetComponent<Image>(); //[cite: 1]
        
        if (fondoZona != null) //[cite: 1]
        {
            // CAPTURA CRUCIAL: Guardamos el color de diseño (así se mantiene transparente si así lo configuraste)[cite: 1]
            colorOriginalDiseno = fondoZona.color; //[cite: 1]
        }
    }

    // Evento nativo que detecta cuando un bloque entra o sale de la zona
    void OnTransformChildrenChanged()
    {
        if (Application.isPlaying && gameObject.activeInHierarchy)
        {
            AjustarEscalaBloques();
        }
    }

    private void AjustarEscalaBloques()
    {
        if (transform.childCount == 0) return;

        float nuevaEscala = escalaBaseBloques;

        // Si superamos el límite visual, calculamos cuánto encogerlos a partir de su escala base
        if (transform.childCount > limiteBloquesNormales)
        {
            int bloquesExtra = transform.childCount - limiteBloquesNormales;
            nuevaEscala = escalaBaseBloques - (bloquesExtra * reduccionPorBloqueExtra);
            
            // Limitamos la escala para que no se hagan invisibles o demasiado pequeños
            nuevaEscala = Mathf.Max(nuevaEscala, escalaMinima);
        }

        // Aplicamos la escala a todos los bloques hijos
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.localScale = new Vector3(nuevaEscala, nuevaEscala, 1f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) //[cite: 1]
    {
        // Cancelar el resaltado si la zona ya está en su capacidad máxima
        if (limitarCapacidad && transform.childCount >= capacidadMaximaBloques)
        {
            return; 
        }

        // Solo altera el color si activaste explícitamente la casilla en el Inspector[cite: 1]
        if (usarResaltadoVisual && ScratchBlock.blockBeingDragged != null && fondoZona != null) //[cite: 1]
        {
            if (eventData.pointerCurrentRaycast.gameObject == gameObject) //[cite: 1]
            {
                fondoZona.color = colorResaltado; //[cite: 1]
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData) //[cite: 1]
    {
        if (usarResaltadoVisual && fondoZona != null) //[cite: 1]
        {
            fondoZona.color = colorOriginalDiseno; //[cite: 1]
        }
    }

    public void OnDrop(PointerEventData eventData) //[cite: 1]
    {
        if (ScratchBlock.blockBeingDragged != null) //[cite: 1]
        {
            // NUEVO: Comprobar si hemos alcanzado o superado la capacidad máxima permitida
            if (limitarCapacidad && transform.childCount >= capacidadMaximaBloques)
            {
                // Retornamos inmediatamente para rechazar el bloque.
                // Tu script ScratchBlock debería encargarse de devolverlo a su posición original al fallar el drop.
                return; 
            }

            ScratchBlock block = ScratchBlock.blockBeingDragged; //[cite: 1]
            
            int newIndex = 0; //[cite: 1]
            for (int i = 0; i < transform.childCount; i++) //[cite: 1]
            {
                Transform child = transform.GetChild(i); //[cite: 1]
                if (child == block.transform) continue; //[cite: 1]

                if (eventData.position.x > child.position.x) //[cite: 1]
                {
                    newIndex = i + 1; //[cite: 1]
                }
            }

            block.transform.SetParent(this.transform); //[cite: 1]
            block.transform.SetSiblingIndex(newIndex); //[cite: 1]
            block.SetInWorkspace(true); //[cite: 1]

            // LÓGICA AISLADA: Para bloques directos en la zona de trabajo principal[cite: 1]
            StartCoroutine(ActualizarAcomodoSimple(this.transform)); //[cite: 1]
        }
    }

    // --- RUTINA LIGERA PARA AVANZAR/GIRAR Y LA ZONA PRINCIPAL ---[cite: 1]
    private IEnumerator ActualizarAcomodoSimple(Transform zonaPrincipal) //[cite: 1]
    {
        // Solo esperamos que caiga y actualizamos la fila horizontal[cite: 1]
        yield return new WaitForEndOfFrame(); //[cite: 1]
        
        AjustarEscalaBloques(); 

        RectTransform rect = zonaPrincipal.GetComponent<RectTransform>(); //[cite: 1]
        if (rect != null) //[cite: 1]
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect); //[cite: 1]
        }
    }
    
    private bool CheckOverflow() //[cite: 1]
    {
        if (!esZonaPrincipal) return false;  //[cite: 1]
        if (layoutGroup == null) return false; //[cite: 1]

        float totalWidth = layoutGroup.padding.left + layoutGroup.padding.right; //[cite: 1]
        
        for (int i = 0; i < transform.childCount; i++) //[cite: 1]
        {
            RectTransform childRect = transform.GetChild(i) as RectTransform; //[cite: 1]
            if (childRect != null && childRect.gameObject != ScratchBlock.blockBeingDragged?.gameObject) //[cite: 1]
            {
                totalWidth += childRect.rect.width * childRect.localScale.x; 
            }
        }
        
        if (transform.childCount > 1) //[cite: 1]
        {
            totalWidth += (transform.childCount - 1) * layoutGroup.spacing; //[cite: 1]
        }

        return totalWidth > rectTransform.rect.width; //[cite: 1]
    }
}