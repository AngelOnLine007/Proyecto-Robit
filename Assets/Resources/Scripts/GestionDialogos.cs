using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum Expresion { Ninguna, Feliz, Molesto, Pensativo }

// AÑADIDO: Nuevo tipo de evento para mover objetos
public enum TipoEvento { Texto, MostrarObjetoTemporal, MoverObjeto }

[System.Serializable]
public class RetratoConfig
{
    public Expresion tipo;
    public Sprite imagen;
}

public struct DatosEvento
{
    public TipoEvento tipo;
    
    // Datos para Mensajes
    public string texto;
    public Expresion expresion;
    
    // Datos para Objetos (Mostrar y Mover)
    public GameObject objetoAMostrar;
    public float tiempoEspera; // Este sigue siendo el tiempo que tarda en viajar
    
    // Datos específicos para MoverObjeto
    public Transform puntoA;
    public Transform puntoB;
    public float tiempoEsperaEnDestino; // NUEVO: Tiempo que se queda quieto en el Punto B
}

public class GestionDialogos : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject panelDialogo;
    public TextMeshProUGUI componenteTexto;
    public Image imagenRetrato;
    public Button bloqueadorInteraccion;

    [Header("Base de Datos de Expresiones")]
    public List<RetratoConfig> bibliotecaRetratos;

    [Header("Configuración")]
    public float velocidadEscritura = 0.05f;

    private bool estaEscribiendo = false;
    private bool completarTextoDeGolpe = false;
    private Coroutine corrutinaEscritura;

    private Queue<DatosEvento> colaEventos = new Queue<DatosEvento>();
    private bool dialogoActivo = false;

    void Awake()
    {
        panelDialogo.SetActive(false);
        bloqueadorInteraccion.gameObject.SetActive(false);
        bloqueadorInteraccion.onClick.AddListener(AlHacerClicEnPantalla);
    }

    // --- FUNCIONES PARA ENCOLAR EVENTOS ---

    public void MostrarMensaje(string mensaje, Expresion expresionDeseada)
    {
        colaEventos.Enqueue(new DatosEvento 
        { 
            tipo = TipoEvento.Texto, 
            texto = mensaje, 
            expresion = expresionDeseada 
        });
        RevisarInicioDialogo();
    }

    public void MostrarObjeto(GameObject objeto, float segundos)
    {
        colaEventos.Enqueue(new DatosEvento 
        { 
            tipo = TipoEvento.MostrarObjetoTemporal, 
            objetoAMostrar = objeto, 
            tiempoEspera = segundos 
        });
        RevisarInicioDialogo();
    }

    // NUEVA FUNCIÓN: Para encolar el movimiento de un objeto
    public void MoverObjeto(GameObject objeto, Transform inicio, Transform fin, float duracionViaje, float tiempoEsperaDestino)
    {
        colaEventos.Enqueue(new DatosEvento 
        { 
            tipo = TipoEvento.MoverObjeto, 
            objetoAMostrar = objeto, 
            puntoA = inicio,
            puntoB = fin,
            tiempoEspera = duracionViaje,
            tiempoEsperaEnDestino = tiempoEsperaDestino // Asignación del nuevo dato
        });
        RevisarInicioDialogo();
    }

    private void RevisarInicioDialogo()
    {
        if (!dialogoActivo)
        {
            dialogoActivo = true;
            Time.timeScale = 0f; 
            ProcesarSiguienteEvento();
        }
    }

    // --- LÓGICA DE PROCESAMIENTO ---

    private void ProcesarSiguienteEvento()
    {
        if (colaEventos.Count == 0)
        {
            CerrarDialogo();
            return;
        }

        DatosEvento eventoActual = colaEventos.Dequeue();

        if (eventoActual.tipo == TipoEvento.Texto)
        {
            EjecutarEventoTexto(eventoActual);
        }
        else if (eventoActual.tipo == TipoEvento.MostrarObjetoTemporal)
        {
            StartCoroutine(EjecutarEventoObjeto(eventoActual));
        }
        // NUEVO: Procesar el evento de movimiento
        else if (eventoActual.tipo == TipoEvento.MoverObjeto)
        {
            StartCoroutine(EjecutarEventoMovimiento(eventoActual));
        }
    }

    private void EjecutarEventoTexto(DatosEvento datos)
    {
        panelDialogo.SetActive(true);
        bloqueadorInteraccion.gameObject.SetActive(true);

        Sprite spriteAEncontrar = null;
        foreach (var config in bibliotecaRetratos)
        {
            if (config.tipo == datos.expresion)
            {
                spriteAEncontrar = config.imagen;
                break;
            }
        }

        if (spriteAEncontrar != null)
        {
            imagenRetrato.gameObject.SetActive(true);
            imagenRetrato.sprite = spriteAEncontrar;
        }
        else
        {
            imagenRetrato.gameObject.SetActive(false);
        }
        
        if (corrutinaEscritura != null) StopCoroutine(corrutinaEscritura);
        corrutinaEscritura = StartCoroutine(EscribirTexto(datos.texto));
    }

    private IEnumerator EjecutarEventoObjeto(DatosEvento datos)
    {
        panelDialogo.SetActive(false);
        bloqueadorInteraccion.gameObject.SetActive(false); 

        if (datos.objetoAMostrar != null) datos.objetoAMostrar.SetActive(true);

        yield return new WaitForSecondsRealtime(datos.tiempoEspera);

        if (datos.objetoAMostrar != null) datos.objetoAMostrar.SetActive(false);

        ProcesarSiguienteEvento();
    }

    // NUEVA CORRUTINA: Maneja el traslado del objeto usando tiempo real
    private IEnumerator EjecutarEventoMovimiento(DatosEvento datos)
    {
        panelDialogo.SetActive(false);
        bloqueadorInteraccion.gameObject.SetActive(false);

        if (datos.objetoAMostrar != null && datos.puntoA != null)
        {
            datos.objetoAMostrar.SetActive(true);
            datos.objetoAMostrar.transform.position = datos.puntoA.position;
        }

        float tiempoPasado = 0f;

        // 1. Fase de Movimiento (Lerp)
        while (tiempoPasado < datos.tiempoEspera)
        {
            tiempoPasado += Time.unscaledDeltaTime; 
            float porcentajeCompletado = tiempoPasado / datos.tiempoEspera;

            if (datos.objetoAMostrar != null && datos.puntoA != null && datos.puntoB != null)
            {
                datos.objetoAMostrar.transform.position = Vector3.Lerp(datos.puntoA.position, datos.puntoB.position, porcentajeCompletado);
            }

            yield return null; 
        }

        // Aseguramos posición final exacta en el Punto B
        if (datos.objetoAMostrar != null && datos.puntoB != null)
        {
            datos.objetoAMostrar.transform.position = datos.puntoB.position;
        }

        // 2. NUEVO: Fase de Espera en el Destino
        // Nos quedamos pausados aquí el tiempo configurado antes de ocultar el objeto
        yield return new WaitForSecondsRealtime(datos.tiempoEsperaEnDestino);

        // 3. Fase de Cierre
        if (datos.objetoAMostrar != null)
        {
            datos.objetoAMostrar.SetActive(false);
        }

        ProcesarSiguienteEvento();
    }

    private IEnumerator EscribirTexto(string mensaje)
    {
        estaEscribiendo = true;
        completarTextoDeGolpe = false;
        componenteTexto.text = "";

        foreach (char letra in mensaje.ToCharArray())
        {
            if (completarTextoDeGolpe)
            {
                componenteTexto.text = mensaje;
                break;
            }
            componenteTexto.text += letra;
            yield return new WaitForSecondsRealtime(velocidadEscritura); 
        }
        estaEscribiendo = false;
    }

    public void AlHacerClicEnPantalla()
    {
        if (estaEscribiendo) 
        {
            completarTextoDeGolpe = true;
        }
        else 
        {
            ProcesarSiguienteEvento(); 
        }
    }

    private void CerrarDialogo()
    {
        panelDialogo.SetActive(false);
        bloqueadorInteraccion.gameObject.SetActive(false);
        
        dialogoActivo = false;
        Time.timeScale = 1f; 
    }
}