using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PasoTutorial
{
    public TipoEvento tipoDePaso;

    [Header("Configuración si es TEXTO:")]
    [TextArea(2, 4)] 
    public string mensaje;
    public Expresion expresionPersonaje;

    [Header("Configuración si es OBJETO TEMPORAL o MOVER:")]
    public GameObject objetoIndicador;
    [Tooltip("Tiempo que permanece visible (Objeto Temporal) o tiempo que tarda en viajar (Mover Objeto)")]
    public float tiempo = 2f; 

    [Header("Configuración EXCLUSIVA si es MOVER OBJETO:")]
    public Transform puntoA;
    public Transform puntoB;
    [Tooltip("Segundos que se quedará quieto en el Punto B antes de continuar")]
    public float tiempoEsperaEnDestino = 1.5f; // NUEVO: Ajustable desde el Inspector
}

public class LanzadorTutorialNivel : MonoBehaviour
{
    public GestionDialogos gestionDialogos;
    public List<PasoTutorial> pasosDelTutorial;

    void Start()
    {
        if (gestionDialogos != null)
        {
            foreach (var paso in pasosDelTutorial)
            {
                if (paso.tipoDePaso == TipoEvento.Texto)
                {
                    gestionDialogos.MostrarMensaje(paso.mensaje, paso.expresionPersonaje);
                }
                else if (paso.tipoDePaso == TipoEvento.MostrarObjetoTemporal)
                {
                    gestionDialogos.MostrarObjeto(paso.objetoIndicador, paso.tiempo);
                }
                else if (paso.tipoDePaso == TipoEvento.MoverObjeto)
                {
                    // MODIFICADO: Enviamos también el nuevo parámetro de espera en destino
                    gestionDialogos.MoverObjeto(paso.objetoIndicador, paso.puntoA, paso.puntoB, paso.tiempo, paso.tiempoEsperaEnDestino);
                }
            }
        }
        else
        {
            Debug.LogWarning("Falta asignar el Gestor de Diálogos al LanzadorTutorialNivel.");
        }
    }
}