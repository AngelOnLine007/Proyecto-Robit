using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class RobotController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float distanciaMovimiento = 1f; 
    public float duracionMovimiento = 0.5f;
    public Vector3 direccionActual = Vector3.right; 

    [Header("Límites del Tablero")]
    public GeneradorTablero tablero;
    
    [HideInInspector] public bool errorDeMovimiento = false;
    [HideInInspector] public bool chocaConObstaculo = false;

    [Header("Configuración de Animación")]
    public Sprite spriteParado;
    public Sprite spriteCaminar1;
    public Sprite spriteCaminar2;
    public float velocidadAnimacion = 0.15f;

    private SpriteRenderer spriteRenderer;
    private Coroutine corrutinaAnimacionCaminar;

    private Vector3 posicionInicialNivel;
    private Vector3 direccionInicialNivel;
    private Quaternion rotacionInicialNivel;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteParado != null) spriteRenderer.sprite = spriteParado;

        posicionInicialNivel = transform.position;
        direccionInicialNivel = direccionActual;
        rotacionInicialNivel = transform.rotation; 
    }

    public void VolverAlInicio()
    {
        transform.position = posicionInicialNivel;
        direccionActual = direccionInicialNivel;
        transform.rotation = rotacionInicialNivel; 
        
        errorDeMovimiento = false;
        chocaConObstaculo = false; 
        
        if (spriteParado != null) spriteRenderer.sprite = spriteParado;
    }

    public IEnumerator EjecutarComando(BlockCommand bloque)
    {
        if (bloque.command == CommandType.Avanzar) 
        {
            if (spriteCaminar1 != null && spriteCaminar2 != null)
                corrutinaAnimacionCaminar = StartCoroutine(CicloAnimacionCaminar());

            Vector3 posicionInicial = transform.position;
            Vector3 posicionDestino = Vector3.zero; 

            if (tablero == null || !ObtenerSiguienteCasilla(out posicionDestino))
            {
                Debug.Log("ROBIT no puede avanzar.");
                spriteRenderer.sprite = spriteParado;
                if(corrutinaAnimacionCaminar != null) StopCoroutine(corrutinaAnimacionCaminar);
                errorDeMovimiento = true; 
                yield break; 
            }

            // --- NUEVO: Usamos Linecast para trazar una línea entre ROBIT y su destino ---
            if (HayObstaculoEnCamino(posicionInicial, posicionDestino))
            {
                Debug.Log("ROBIT detectó un obstáculo en el trayecto.");
                spriteRenderer.sprite = spriteParado;
                if(corrutinaAnimacionCaminar != null) StopCoroutine(corrutinaAnimacionCaminar);
                chocaConObstaculo = true; 
                yield break; 
            }

            float tiempoTranscurrido = 0;
            while (tiempoTranscurrido < duracionMovimiento)
            {
                transform.position = Vector3.Lerp(posicionInicial, posicionDestino, tiempoTranscurrido / duracionMovimiento);
                tiempoTranscurrido += Time.deltaTime;
                yield return null; 
            }
            
            transform.position = posicionDestino;

            if (corrutinaAnimacionCaminar != null) StopCoroutine(corrutinaAnimacionCaminar);
            if (spriteParado != null) spriteRenderer.sprite = spriteParado;
        }
        else if (bloque.command == CommandType.Girar)
        {
            float gradosDeGiro = (bloque.direccionGiro == DireccionGiro.Derecha) ? -90f : 90f;
            Vector3 nuevaDireccion = Quaternion.Euler(0, 0, gradosDeGiro) * direccionActual;
            nuevaDireccion = new Vector3(Mathf.Round(nuevaDireccion.x), Mathf.Round(nuevaDireccion.y), 0).normalized;

            Quaternion rotacionInicial = transform.rotation;
            Quaternion rotacionDestino = transform.rotation * Quaternion.Euler(0, 0, gradosDeGiro);

            float tiempoTranscurrido = 0;
            while (tiempoTranscurrido < duracionMovimiento)
            {
                transform.rotation = Quaternion.Slerp(rotacionInicial, rotacionDestino, tiempoTranscurrido / duracionMovimiento);
                tiempoTranscurrido += Time.deltaTime;
                yield return null;
            }

            transform.rotation = rotacionDestino;
            direccionActual = nuevaDireccion;
        }
        
        yield return new WaitForSeconds(0.1f); 
    }

    private bool ObtenerSiguienteCasilla(out Vector3 destinoExacto)
    {
        destinoExacto = Vector3.zero;
        
        Transform casillaActual = null;
        float distMinimaActual = float.MaxValue;

        foreach (Transform casilla in tablero.transform)
        {
            float dist = Vector3.Distance(transform.position, casilla.position);
            if (dist < distMinimaActual)
            {
                distMinimaActual = dist;
                casillaActual = casilla; 
            }
        }

        Transform casillaDestino = null;
        float distanciaMinimaHaciaAdelante = float.MaxValue;
        
        foreach (Transform casilla in tablero.transform)
        {
            if (casilla == casillaActual) continue;

            Vector3 vectorHaciaCasilla = casilla.position - transform.position;
            Vector3 direccionHaciaCasilla = vectorHaciaCasilla.normalized;
            float alineacion = Vector3.Dot(direccionActual.normalized, direccionHaciaCasilla);
            
            if (alineacion > 0.7f) 
            {
                float distancia = vectorHaciaCasilla.magnitude;
                if (distancia < distanciaMinimaHaciaAdelante)
                {
                    distanciaMinimaHaciaAdelante = distancia;
                    casillaDestino = casilla;
                }
            }
        }

        if (casillaDestino != null)
        {
            destinoExacto = new Vector3(casillaDestino.position.x, casillaDestino.position.y, transform.position.z);
            return true;
        }

        return false;
    }

    // --- NUEVA FUNCIÓN: Comprueba una línea desde el origen al destino ---
    private bool HayObstaculoEnCamino(Vector3 origen, Vector3 destino)
    {
        // LinecastAll dibuja un rayo y devuelve todos los colliders que atraviesa
        RaycastHit2D[] impactos = Physics2D.LinecastAll(origen, destino);
        
        foreach (RaycastHit2D impacto in impactos)
        {
            // Omitimos a ROBIT por si tiene un colisionador propio para no bloquearse a sí mismo
            if (impacto.collider.gameObject == this.gameObject) continue;

            if (impacto.collider.CompareTag("Obstaculo"))
            {
                return true; // Encontramos un cable/pared en el camino
            }
        }
        return false;
    }

    private IEnumerator CicloAnimacionCaminar()
    {
        while (true)
        {
            spriteRenderer.sprite = spriteCaminar1;
            yield return new WaitForSeconds(velocidadAnimacion);
            
            spriteRenderer.sprite = spriteCaminar2;
            yield return new WaitForSeconds(velocidadAnimacion);
        }
    }
}