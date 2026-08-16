using UnityEngine;
using UnityEngine.UI;
using System.Collections; 
using UnityEngine.EventSystems;
using TMPro; 

public enum CommandType {
    Avanzar,
    Girar,
    RepetirInicio, // <-- NUEVO
    RepetirFin     // <-- NUEVO
}

public enum DireccionGiro {
    Derecha,
    Izquierda
}

[RequireComponent(typeof(Image))]
public class BlockCommand : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("Selecciona qué instrucción representa este bloque visual")]
    public CommandType command;

    [Header("Configuración de Giro")]
    [Tooltip("Solo aplica si el comando es 'Girar'")]
    public DireccionGiro direccionGiro = DireccionGiro.Derecha;
    public RectTransform flechaVisual; 

    [Header("Configuración de Bucle (Repetir Fin)")]
    [Tooltip("La lista desplegable para seleccionar el número de veces (Solo para RepetirFin)")]
    public TMP_Dropdown dropdownRepeticiones; 

    [Header("Efecto de Iluminación")]
    [HideInInspector] public Color colorNormal; 
    public Color colorIluminado = new Color(1f, 0.92f, 0.016f, 1f);

    [Header("Configuración de Animación Visual")]
    public float duracionAnimacionGiro = 0.2f; 

    private Image imagenBloque;
    private Coroutine corrutinaGiro; 

    private void Awake()
    {
        imagenBloque = GetComponent<Image>();
        
        if (imagenBloque != null)
        {
            colorNormal = imagenBloque.color; 
        }
        
        Apagar();
    }

    public void Iluminar()
    {
        if (imagenBloque != null) imagenBloque.color = colorIluminado;
    }

    public void Apagar()
    {
        if (imagenBloque != null) imagenBloque.color = colorNormal;
    }

    public void AlternarDireccion()
    {
        if (command != CommandType.Girar) return;

        if (direccionGiro == DireccionGiro.Derecha)
        {
            direccionGiro = DireccionGiro.Izquierda;
            if (flechaVisual != null) IniciarAnimacionGiro(-1f);
        }
        else 
        {
            direccionGiro = DireccionGiro.Derecha;
            if (flechaVisual != null) IniciarAnimacionGiro(1f);
        }
    }

    private void IniciarAnimacionGiro(float escalaXDestino)
    {
        if (corrutinaGiro != null) StopCoroutine(corrutinaGiro);
        corrutinaGiro = StartCoroutine(AnimarGiroPaperMario(escalaXDestino));
    }

    private IEnumerator AnimarGiroPaperMario(float escalaXDestino)
    {
        Vector3 escalaInicial = flechaVisual.localScale;
        Vector3 escalaFinal = new Vector3(escalaXDestino, escalaInicial.y, escalaInicial.z);
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracionAnimacionGiro)
        {
            flechaVisual.localScale = Vector3.Lerp(escalaInicial, escalaFinal, tiempoTranscurrido / duracionAnimacionGiro);
            tiempoTranscurrido += Time.deltaTime;
            yield return null; 
        }
        flechaVisual.localScale = escalaFinal;
    }

    public int ObtenerCantidadRepeticiones()
    {
        // Ahora esto solo se lee si el bloque es el final
        if (command == CommandType.RepetirFin && dropdownRepeticiones != null)
        {
            string textoSeleccionado = dropdownRepeticiones.options[dropdownRepeticiones.value].text;
            if (int.TryParse(textoSeleccionado, out int valor))
            {
                return valor;
            }
        }
        return 1; 
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Ya no necesitamos iluminar para anidar
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Ya no necesitamos apagar al salir
    }
}