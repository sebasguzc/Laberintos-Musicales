using UnityEngine;

/// <summary>
/// Level Music Manager - Administrador de música ambiental específica para niveles
/// Reproduce una canción al azar al cargar el nivel y la mantiene en bucle.
/// </summary>
public class LevelMusicManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [Range(0f, 1f)] 
    [SerializeField] private float levelVolume = 0.3f;

    [Header("Level Tracks")]
    [SerializeField] private AudioClip[] levelTracks; // Las canciones específicas para este nivel

    void Start()
    {
        // 1. Configurar o encontrar el AudioSource
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

        // 2. Reproducir una canción al azar para este nivel
        PlayRandomLevelTrack();
    }

    /// <summary>
    /// Selecciona y reproduce una pista al azar del nivel actual
    /// </summary>
    public void PlayRandomLevelTrack()
    {
        if (levelTracks == null || levelTracks.Length == 0)
        {
            Debug.LogWarning("LevelMusicManager: No hay pistas de música asignadas para este nivel.");
            return;
        }

        int randomIndex = Random.Range(0, levelTracks.Length);
        AudioClip selectedTrack = levelTracks[randomIndex];

        if (selectedTrack != null)
        {
            audioSource.clip = selectedTrack;
            audioSource.volume = levelVolume;
            audioSource.Play();
            Debug.Log("Música ambiental del nivel sonando: " + selectedTrack.name);
        }
    }
}