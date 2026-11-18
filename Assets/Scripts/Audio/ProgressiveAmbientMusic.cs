using UnityEngine;

/// <summary>
/// Música ambiental del nivel con zonas de loop que cambian según el progreso.
///
/// Fase 0 (ningún instrumento resuelto):  al llegar a loopEndByStage[0] vuelve a loopStart.
/// Fase 1 (un instrumento resuelto):      al llegar a loopEndByStage[1] vuelve a loopStart.
/// Fase 2 (ambos resueltos):              sin zona de loop; suena hasta el final y reinicia desde el principio.
///
/// El progreso lo recibe de MelodyLevelManager.OnProgressChanged (no hay que conectar nada en el Inspector).
/// </summary>
public class ProgressiveAmbientMusic : MonoBehaviour
{
    [System.Serializable]
    public class AmbientTrack
    {
        public AudioClip clip;

        [Tooltip("Segundo al que se regresa en cada salto (0:38 = 38 s)")]
        public float loopStart = 38f;

        [Tooltip("Segundo en el que se salta de regreso, por fase. " +
                 "Elemento 0 = ningún instrumento resuelto, elemento 1 = un instrumento resuelto. " +
                 "Con todos resueltos suena hasta el final.")]
        public float[] loopEndByStage = { 125f, 138f };
    }

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [Range(0f, 1f)]
    [SerializeField] private float ambientVolume = 0.3f;

    [Header("Pistas (se elige una al azar al empezar el nivel)")]
    [Tooltip("Elemento 0 = Ambiente 0 (loop 0:38 -> 2:05, luego 2:18). Elemento 1 = Ambiente 1 (loop 0:29 -> 2:15, luego 3:14).")]
    [SerializeField]
    private AmbientTrack[] tracks =
    {
        new AmbientTrack { loopStart = 38f, loopEndByStage = new float[] { 125f, 138f } },
        new AmbientTrack { loopStart = 29f, loopEndByStage = new float[] { 135f, 194f } }
    };

    [Header("Pruebas")]
    [Tooltip("-1 = elegir al azar. 0 o 1 = forzar esa pista para probarla.")]
    [SerializeField] private int forceTrackIndex = -1;

    private AmbientTrack current;
    private int stage;

    private void OnEnable()
    {
        MelodyLevelManager.OnProgressChanged += SetStage;
    }

    private void OnDisable()
    {
        MelodyLevelManager.OnProgressChanged -= SetStage;
    }

    private void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = true;        // en la fase final, al terminar la pista vuelve al inicio
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        PlayTrack();
    }

    /// <summary>Elige una pista (al azar, o la forzada) y la reproduce desde el inicio.</summary>
    public void PlayTrack()
    {
        if (tracks == null || tracks.Length == 0)
        {
            Debug.LogWarning("ProgressiveAmbientMusic: no hay pistas configuradas.");
            return;
        }

        int index = (forceTrackIndex >= 0 && forceTrackIndex < tracks.Length)
            ? forceTrackIndex
            : Random.Range(0, tracks.Length);

        current = tracks[index];

        if (current.clip == null)
        {
            Debug.LogWarning($"ProgressiveAmbientMusic: la pista {index} no tiene AudioClip asignado.");
            current = null;
            return;
        }

        audioSource.clip = current.clip;
        audioSource.volume = ambientVolume;
        audioSource.time = 0f;
        audioSource.Play();

        Debug.Log($"Música del nivel: {current.clip.name} (pista {index})");
    }

    /// <summary>Recibe cuántos instrumentos se han resuelto (0, 1 o 2).</summary>
    public void SetStage(int solvedInstruments)
    {
        stage = Mathf.Max(0, solvedInstruments);
        Debug.Log($"Música: fase {stage}");
    }

    private void Update()
    {
        if (current == null || current.clip == null || !audioSource.isPlaying) return;

        float loopEnd = GetLoopEnd();
        if (loopEnd <= 0f) return; // fase final: suena hasta el final y reinicia sola

        int frequency = current.clip.frequency;
        int endSample = Mathf.RoundToInt(loopEnd * frequency);
        int startSample = Mathf.RoundToInt(current.loopStart * frequency);
        int nowSample = audioSource.timeSamples;

        if (nowSample >= endSample)
        {
            // Se conserva el pequeño exceso para que el salto no desfase el ritmo
            int target = startSample + (nowSample - endSample);
            audioSource.timeSamples = Mathf.Clamp(target, 0, current.clip.samples - 1);
        }
    }

    private float GetLoopEnd()
    {
        if (current.loopEndByStage == null || stage >= current.loopEndByStage.Length) return -1f;

        float end = current.loopEndByStage[stage];
        if (end <= current.loopStart || end >= current.clip.length) return -1f;

        return end;
    }
}
