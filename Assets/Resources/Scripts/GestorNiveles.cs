using System.Collections;
using UnityEngine;

public class GestorNivelesAvanzado : MonoBehaviour
{
    [Header("Paneles de la Interfaz")]
    public RectTransform panelTutoriales; 
    public RectTransform panelNuevosNiveles; 

    [Header("Flechas de Navegación")]
    public RectTransform flechaSiguiente; 
    public RectTransform flechaVolver;     

    [Header("Configuración del Deslizamiento")]
    public float tiempoDeslizamiento = 0.5f;
    public float distanciaDesplazamiento = 1500f;

    [Header("Configuración de la Oscilación (Vibración)")]
    public float amplitudVibracion = 30f; 
    public float velocidadVibracion = 6f; 

    private Vector2 posBaseSiguiente;
    private Vector2 posBaseVolver;
    
    // NUEVA VARIABLE: Aquí guardaremos la altura original de tu diseño
    private float posicionYBase; 
    
    private bool estaAnimando = false; 

    void Start()
    {
        posBaseSiguiente = flechaSiguiente.anchoredPosition;
        posBaseVolver = flechaVolver.anchoredPosition;

        // Leemos en qué posición Y pusiste los tutoriales en el Editor y la guardamos
        posicionYBase = panelTutoriales.anchoredPosition.y;

        panelTutoriales.gameObject.SetActive(true);
        panelNuevosNiveles.gameObject.SetActive(false);
        flechaSiguiente.gameObject.SetActive(true);
        flechaVolver.gameObject.SetActive(false);
    }

    void Update()
    {
        float movimiento = Mathf.Sin(Time.time * velocidadVibracion) * amplitudVibracion;

        if (flechaSiguiente.gameObject.activeSelf)
        {
            flechaSiguiente.anchoredPosition = posBaseSiguiente + new Vector2(movimiento, 0);
        }

        if (flechaVolver.gameObject.activeSelf)
        {
            flechaVolver.anchoredPosition = posBaseVolver + new Vector2(-movimiento, 0);
        }
    }

    public void IrANuevosNiveles()
    {
        if (estaAnimando) return;
        StartCoroutine(AnimarCambio(panelTutoriales, panelNuevosNiveles, flechaSiguiente, flechaVolver));
    }

    public void VolverATutoriales()
    {
        if (estaAnimando) return;
        StartCoroutine(AnimarCambio(panelNuevosNiveles, panelTutoriales, flechaVolver, flechaSiguiente));
    }

    private IEnumerator AnimarCambio(RectTransform panelSalir, RectTransform panelEntrar, RectTransform flechaApagar, RectTransform flechaEncender)
    {
        estaAnimando = true;
        flechaApagar.gameObject.SetActive(false);

        // AQUÍ ESTÁ LA CORRECCIÓN: 
        // En lugar de Vector2.zero, usamos X en 0, pero mantenemos tu Y original.
        Vector2 posCentro = new Vector2(0, posicionYBase); 
        
        Vector2 posFueraIzquierda = posCentro + new Vector2(-distanciaDesplazamiento, 0);
        Vector2 posFueraDerecha = posCentro + new Vector2(distanciaDesplazamiento, 0);

        panelEntrar.gameObject.SetActive(true);

        // Determinamos la dirección automáticamente. 
        // Si vamos a Nuevos Niveles, salen a la izquierda y entran por la derecha.
        // Si Volvemos a Tutoriales, salen a la derecha y entran por la izquierda.
        Vector2 iniSalir = posCentro;
        Vector2 finSalir = (panelEntrar == panelNuevosNiveles) ? posFueraIzquierda : posFueraDerecha;
        
        Vector2 iniEntrar = (panelEntrar == panelNuevosNiveles) ? posFueraDerecha : posFueraIzquierda;
        Vector2 finEntrar = posCentro;

        float tiempo = 0f;

        while (tiempo < tiempoDeslizamiento)
        {
            tiempo += Time.deltaTime;
            float porcentaje = tiempo / tiempoDeslizamiento;
            float ease = Mathf.SmoothStep(0, 1, porcentaje); 

            panelSalir.anchoredPosition = Vector2.Lerp(iniSalir, finSalir, ease);
            panelEntrar.anchoredPosition = Vector2.Lerp(iniEntrar, finEntrar, ease);

            yield return null;
        }

        panelSalir.anchoredPosition = finSalir;
        panelEntrar.anchoredPosition = finEntrar;
        panelSalir.gameObject.SetActive(false);

        flechaEncender.gameObject.SetActive(true);
        estaAnimando = false;
    }
}