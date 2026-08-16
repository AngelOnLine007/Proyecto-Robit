using UnityEngine;
using UnityEngine.EventSystems;

public class BlockPaletteZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        // Si el usuario suelta un bloque ya clonado (no plantilla) en la paleta, lo eliminamos
        if (ScratchBlock.blockBeingDragged != null && !ScratchBlock.blockBeingDragged.isTemplate)
        {
            Destroy(ScratchBlock.blockBeingDragged.gameObject);
        }
    }
}