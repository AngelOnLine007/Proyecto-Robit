using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasGroup))]
public class ScratchBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool isTemplate = true;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    public RectTransform rectTransform;

    public static ScratchBlock blockBeingDragged;
    public bool isInWorkspace = false;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isTemplate)
        {
            GameObject clone = Instantiate(gameObject, canvas.transform);
            blockBeingDragged = clone.GetComponent<ScratchBlock>();
            blockBeingDragged.isTemplate = false;
            blockBeingDragged.canvasGroup.blocksRaycasts = false;
            blockBeingDragged.rectTransform.position = this.rectTransform.position;
        }
        else
        {
            blockBeingDragged = this;
            isInWorkspace = false;

            // CAPTURAMOS DE DÓNDE SALIÓ EL BLOQUE ANTES DE MOVERLO
            Transform padreAnterior = transform.parent; 

            transform.SetParent(canvas.transform);
            canvasGroup.blocksRaycasts = false;

            // Llamamos a la rutina de reducción de espacio
            StartCoroutine(ActualizarExtraccion(padreAnterior));
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (blockBeingDragged != null)
        {
            blockBeingDragged.rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (blockBeingDragged != null)
        {
            blockBeingDragged.canvasGroup.blocksRaycasts = true;

            if (!blockBeingDragged.isInWorkspace)
            {
                Destroy(blockBeingDragged.gameObject);
            }
            
            blockBeingDragged = null;
        }
    }

    public void SetInWorkspace(bool state)
    {
        isInWorkspace = state;
    }

    private IEnumerator ActualizarExtraccion(Transform origenAnterior)
    {
        yield return new WaitForEndOfFrame();

        Transform actual = origenAnterior;
        
        // Vamos desde el hueco que dejó el bloque hacia afuera, reduciendo los tamaños
        while (actual != null)
        {
            RectTransform rect = actual.GetComponent<RectTransform>();
            if (rect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }
            
            // Si llegamos a la zona principal, paramos
            if (actual.GetComponent<WorkspaceZone>() != null && actual.GetComponent<WorkspaceZone>().esZonaPrincipal)
            {
                break;
            }
            actual = actual.parent;
        }
    }
}