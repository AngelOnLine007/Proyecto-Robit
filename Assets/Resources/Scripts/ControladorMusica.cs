using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ControladorMusica : MonoBehaviour
{
    public static ControladorMusica instancia;

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            AudioClip cancionVieja = instancia.GetComponent<AudioSource>().clip;
            AudioClip cancionNueva = this.GetComponent<AudioSource>().clip;

            if (cancionVieja == cancionNueva)
            {
                Destroy(this.gameObject);
                return;
            }
            else
            {
                Destroy(instancia.gameObject);
            }
        }

        instancia = this;
        DontDestroyOnLoad(this.gameObject);
    }
}