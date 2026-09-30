using UnityEngine;

/// <summary>
/// Ambient Music Manager - Administrador de música de ambiente procedural/aleatoria
/// Selecciona aleatoriamente una de las pistas al iniciar, la reproduce en bucle 
/// a un volumen bajo para no opacar los efectos de sonido del juego.
/// </summary>
public class AmbientMusicManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [Range(0f, 1f)]
    [SerializeField] private float ambientVolume = 0.3f; // Volumen bajo por defecto para ambiente

    [Header("Ambient Tracks")]
    [SerializeField] private AudioClip[] musicTracks; // Aquí arrastrarás tus 2 canciones en el Inspector

    void Start()
    {
        // 1. Configurar o encontrar el AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("AmbientMusicManager: Se añadió un componente AudioSource automáticamente.");
            }
        }

        // 2. Configurar propiedades del AudioSource para música de fondo
        audioSource.loop = true;          // Para que se repita en bucle infinito
        audioSource.playOnAwake = false;  // Lo controlaremos por código para elegir al azar

        // 3. Reproducir música aleatoria si hay pistas asignadas
        PlayRandomAmbientTrack();
    }

    /// <summary>
    /// Selecciona y reproduce una pista al azar del arreglo de canciones
    /// </summary>
    public void PlayRandomAmbientTrack()
    {
        if (musicTracks == null || musicTracks.Length == 0)
        {
            Debug.LogWarning("AmbientMusicManager: No hay pistas de música asignadas en el Inspector.");
            return;
        }

        // Elegir un índice al azar entre 0 y el número de canciones disponibles
        int randomIndex = Random.Range(0, musicTracks.Length);
        AudioClip selectedTrack = musicTracks[randomIndex];

        if (selectedTrack != null)
        {
            audioSource.clip = selectedTrack;
            audioSource.volume = ambientVolume;
            audioSource.Play();

            Debug.Log("Reproduciendo música ambiental aleatoria: " + selectedTrack.name);
        }
        else
        {
            Debug.LogError("AmbientMusicManager: La pista seleccionada al azar es nula (vacia).");
        }
    }

    /// <summary>
    /// Permite cambiar el volumen ambiental dinámicamente desde otro script si lo necesitas
    /// </summary>
    public void SetAmbientVolume(float newVolume)
    {
        ambientVolume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = ambientVolume;
        }
    }
}