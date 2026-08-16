using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic; // IMPORTANTE
using UnityEngine.SceneManagement;

public class SequenceReader : MonoBehaviour
{
    [Header("Configuración Principal")]
    public Transform dropZone; 
    public Button botonPlay;
    public RobotController rob;

    [Header("Configuración de la Meta")]
    public Transform meta;
    public string nombreEscenaNiveles = "MenuNiveles";
    public float distanciaAceptacion = 0.5f;

    [Header("Comunicación")]
    public GestionDialogos sistemaDialogos;

    [Header("Configuración de Límites (Tutorial)")]
    public bool limitarCantidadBloques = false;
    public int maximoBloquesPermitidos = 3;
    [TextArea(2, 3)]
    public string mensajeExcesoBloques = "Estás usando demasiados bloques. Intenta llegar con menos.";

    private void ApagarTodosLosBloques(Transform contenedor)
    {
        for (int i = 0; i < contenedor.childCount; i++)
        {
            BlockCommand bc = contenedor.GetChild(i).GetComponent<BlockCommand>();
            if (bc != null) bc.Apagar();
        }
    }

    public void IniciarSecuencia()
    {
        if (botonPlay != null) botonPlay.interactable = false;
        StartCoroutine(LeerYEjecutar());
    }

    private IEnumerator LeerYEjecutar()
    {
        rob.errorDeMovimiento = false;
        rob.chocaConObstaculo = false; 

        ApagarTodosLosBloques(dropZone);

        // Convertimos los hijos directos a una lista para procesarlos fácilmente
        List<BlockCommand> listaPrincipal = new List<BlockCommand>();
        for (int i = 0; i < dropZone.childCount; i++)
        {
            BlockCommand bc = dropZone.GetChild(i).GetComponent<BlockCommand>();
            if (bc != null) listaPrincipal.Add(bc);
        }

        if (limitarCantidadBloques && listaPrincipal.Count > maximoBloquesPermitidos)
        {
            sistemaDialogos.MostrarMensaje(mensajeExcesoBloques, Expresion.Pensativo);
            sistemaDialogos.MostrarMensaje("Cambia los bloques y prueba de nuevo.", Expresion.Feliz);
            StartCoroutine(PausaYReiniciar());
            yield break; 
        }

        yield return StartCoroutine(EjecutarListaDeBloques(listaPrincipal));

        if (rob.errorDeMovimiento || rob.chocaConObstaculo)
        {
            StartCoroutine(PausaYReiniciar());
        }
        else
        {
            ComprobarMeta();
        }
    }

    // Nueva función que lee una lista plana de izquierda a derecha
    private IEnumerator EjecutarListaDeBloques(List<BlockCommand> lista)
    {
        for (int i = 0; i < lista.Count; i++)
        {
            BlockCommand bloqueActual = lista[i];
            bloqueActual.Iluminar();

            // CASO 1: Encontramos un bloque Final huérfano (sin un Inicio previo)
            if (bloqueActual.command == CommandType.RepetirFin)
            {
                bloqueActual.Apagar();
                rob.errorDeMovimiento = true; 
                sistemaDialogos.MostrarMensaje("Recuerda colocar el bloque inicial del repetir", Expresion.Molesto);
                yield break;
            }
            // CASO 2: Encontramos un bloque Inicial
            else if (bloqueActual.command == CommandType.RepetirInicio)
            {
                int indiceMatch = -1;
                int nivelAnidacion = 0;

                // Escaneamos hacia la derecha buscando el FIN correspondiente
                for (int j = i + 1; j < lista.Count; j++)
                {
                    if (lista[j].command == CommandType.RepetirInicio) 
                    {
                        nivelAnidacion++; // Por si hay un bucle dentro de otro bucle
                    }
                    else if (lista[j].command == CommandType.RepetirFin)
                    {
                        if (nivelAnidacion == 0) 
                        {
                            indiceMatch = j; 
                            break; 
                        }
                        else 
                        {
                            nivelAnidacion--;
                        }
                    }
                }

                // Si no encontramos la pareja...
                if (indiceMatch == -1)
                {
                    bloqueActual.Apagar();
                    rob.errorDeMovimiento = true;
                    sistemaDialogos.MostrarMensaje("Recuerda colocar el bloque final del repetir", Expresion.Molesto);
                    yield break;
                }

                // Extraemos los bloques que están en el medio del Inicio y el Fin
                List<BlockCommand> bloquesInternos = lista.GetRange(i + 1, indiceMatch - i - 1);
                int repeticiones = lista[indiceMatch].ObtenerCantidadRepeticiones();

                // Ejecutamos la sub-lista X veces
                for (int r = 0; r < repeticiones; r++)
                {
                    if (r > 0) yield return new WaitForSeconds(0.2f);
                    
                    yield return StartCoroutine(EjecutarListaDeBloques(bloquesInternos));
                    
                    if (rob.errorDeMovimiento || rob.chocaConObstaculo) break;
                }

                bloqueActual.Apagar();

                // Iluminamos el bloque FIN brevemente para dar feedback visual de que el bucle terminó
                lista[indiceMatch].Iluminar();
                yield return new WaitForSeconds(0.2f);
                lista[indiceMatch].Apagar();

                // Saltamos el iterador 'i' para que no vuelva a leer los bloques que ya ejecutamos en el bucle
                i = indiceMatch; 
            }
            // CASO 3: Avanzar o Girar
            else
            {
                yield return StartCoroutine(rob.EjecutarComando(bloqueActual));
                
                if (rob.errorDeMovimiento)
                {
                    Debug.Log("Secuencia interrumpida: ROBIT intentó salir del área.");
                    sistemaDialogos.MostrarMensaje("¡Cuidado, me puedo caer!", Expresion.Molesto);
                    sistemaDialogos.MostrarMensaje("Vamos a intentarlo de nuevo.", Expresion.Feliz);
                }
                else if (rob.chocaConObstaculo)
                {
                    Debug.Log("Secuencia interrumpida: ROBIT encontró un obstáculo.");
                    sistemaDialogos.MostrarMensaje("¡Cuidado! Me puedo hacer daño con eso si doy otro paso más D:", Expresion.Molesto);
                    sistemaDialogos.MostrarMensaje("Modifica la secuencia para esquivarlo", Expresion.Pensativo);
                    sistemaDialogos.MostrarMensaje("¡Vamos a intentarlo de nuevo!", Expresion.Feliz);
                }
                
                bloqueActual.Apagar();
            }

            if (rob.errorDeMovimiento || rob.chocaConObstaculo) yield break;
        }
    }

    private void ComprobarMeta()
    {
        float distancia = Vector3.Distance(rob.transform.position, meta.position);
        if (distancia <= distanciaAceptacion)
        {
            Debug.Log("¡ÉXITO! ROBIT llegó a la meta.");
            StartCoroutine(CelebrarYCambiarEscena());
        }
        else
        {
            Debug.Log("ROBIT no llegó a la meta. ¡Hay que revisar los bloques!");
            sistemaDialogos.MostrarMensaje("Hm... hasta aquí termina el programa que introdujiste...", Expresion.Pensativo);
            sistemaDialogos.MostrarMensaje("¡Vamos a intentarlo de nuevo! :D", Expresion.Feliz);
            StartCoroutine(PausaYReiniciar());
        }
    }

    private IEnumerator PausaYReiniciar()
    {
        yield return new WaitForSeconds(3f);
        rob.VolverAlInicio();
        if (botonPlay != null) botonPlay.interactable = true;
    }

    private IEnumerator CelebrarYCambiarEscena()
    {
        sistemaDialogos.MostrarMensaje("¡Muy bien! llegamos al portal ¿Qué será lo que nos espera detrás de él?", Expresion.Feliz);
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(nombreEscenaNiveles);
    }
}