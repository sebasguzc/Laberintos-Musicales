using UnityEngine;

/// <summary>
/// Ambient Music Manager (Persistente)
/// Elige una canción al azar al iniciar la primera escena y sobrevive a los cambios de escena 
/// sin reiniciarse ni cortarse.
/// </summary>
public class AmbientMusicManager : MonoBehaviour
{
    // Instancia estática para asegurar que solo exista un reproductor en todo el juego
    private static AmbientMusicManager instance;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [Range(0f, 1f)] 
    [SerializeField] private float ambientVolume = 0.3f;

    [Header("Ambient Tracks")]
    [SerializeField] private AudioClip[] musicTracks; // Tus canciones de ambiente

    void Awake()
    {
        // Patrón Singleton para mantener el objeto vivo entre escenas
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ¡Esto hace que NO se destruya al cambiar de escena!
        }
        else
        {
            // Si ya existe un manager en la escena anterior, destruimos este duplicado para evitar eco o conflictos
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        audioSource.loop = true;
        audioSource.playOnAwake = false;

        // Si la música aún no está sonando (es la primera vez que se inicia), elegimos una pista al azar
        if (!audioSource.isPlaying)
        {
            PlayRandomAmbientTrack();
        }
    }

    /// <summary>
    /// Selecciona y reproduce una pista al azar
    /// </summary>
    public void PlayRandomAmbientTrack()
    {
        if (musicTracks == null || musicTracks.Length == 0)
        {
            Debug.LogWarning("AmbientMusicManager: No hay pistas de música asignadas.");
            return;
        }

        int randomIndex = Random.Range(0, musicTracks.Length);
        AudioClip selectedTrack = musicTracks[randomIndex];

        if (selectedTrack != null)
        {
            audioSource.clip = selectedTrack;
            audioSource.volume = ambientVolume;
            audioSource.Play();
            Debug.Log("Música ambiental persistente sonando: " + selectedTrack.name);
        }
    }
}