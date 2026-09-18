using UnityEngine;

/// <summary>
/// Enemy Tension Synth - Generador de sonidos de tensión ambiental para enemigos
/// Sonido grave, misterioso y envolvente que aumenta la tensión del juego
/// </summary>
public class EnemyTensionSynth : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private int sampleRate = 44100;
    [SerializeField] private float masterVolume = 0.8f;

    [Header("Synthesis Parameters")]
    // [SerializeField] private float baseFrequency = 72.0f; // Reservado para uso futuro
    [SerializeField] private float duration = 8.0f; // ¡Ahora sí se usa!
    [SerializeField] private int tableSize = 1024;
    [SerializeField] private int wavetableSeed = 42;

    [Header("ADSR Envelope")]
    [SerializeField] private float attack = 0.25f;
    [SerializeField] private float decay = 0.6f;
    [SerializeField] [Range(0f, 1f)] private float sustain = 0.6f;
    [SerializeField] private float release = 1.5f;

    [Header("LFO Settings")]
    [SerializeField] private float lfoRateHz = 1.0f;
    [SerializeField] private float filterMinCutoff = 250.0f;
    [SerializeField] private float filterMaxCutoff = 900.0f;
    [SerializeField] [Range(0f, 1f)] private float tremoloDepth = 0.3f;

    [Header("LFO Mode")]
    [SerializeField] private LFOMode lfoMode = LFOMode.Filter; // ¡Ahora sí se usa!

    public enum LFOMode
    {
        Filter,
        Tremolo,
        Both
    }

    [Header("Wavetable Settings")]
    [SerializeField] private float pulseDuty = 0.35f;

    private float[] enemyWavetable;
    private LowPassFilter lowPassFilter;

    void Start()
    {
        Debug.Log("EnemyTensionSynth inicializado correctamente");

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("EnemyTensionSynth: No hay AudioSource en este GameObject. Añade un componente AudioSource.");
                return;
            }
            Debug.Log("AudioSource encontrado automáticamente con GetComponent");
        }

        if (audioSource != null)
            Debug.Log("AudioSource conectado correctamente");

        InitializeWavetable();
        lowPassFilter = new LowPassFilter(sampleRate, filterMinCutoff, filterMaxCutoff, lfoRateHz);
    }

    void InitializeWavetable()
    {
        enemyWavetable = new float[tableSize];
        System.Random rng = new System.Random(wavetableSeed);

        for (int i = 0; i < tableSize; i++)
        {
            float phase = (float)i / tableSize;
            float saw = 2f * phase - 1f;
            float pulse = (phase < pulseDuty) ? 1f : -1f;
            float noise = (float)(rng.NextDouble() * 2f - 1f);
            enemyWavetable[i] = 0.5f * saw + 0.3f * pulse + 0.2f * noise;
        }
    }

    /// <summary>
    /// Reproduce sonido de tensión del enemigo usando los parámetros del Inspector
    /// </summary>
    public void PlayTensionSound()
    {
        AudioClip clip = GenerateEnemyTension(duration, 72.0f, lfoMode, tremoloDepth);
        audioSource.clip = clip;
        audioSource.volume = masterVolume;
        audioSource.Play();
    }

    /// <summary>
    /// Genera el sonido de tensión con parámetros personalizados
    /// </summary>
    public AudioClip GenerateEnemyTension(
        float customDuration = 8.0f,
        float customFrequency = 72.0f,
        LFOMode customLFOMode = LFOMode.Filter,
        float customTremoloDepth = 0.3f)
    {
        int totalFrames = Mathf.CeilToInt(customDuration * sampleRate);
        float[] samples = new float[totalFrames];

        float phase = 0f;
        float increment = customFrequency * tableSize / sampleRate;

        // Reset filter state
        lowPassFilter.Reset();

        bool useFilter = customLFOMode == LFOMode.Filter || customLFOMode == LFOMode.Both;
        bool useTremolo = customLFOMode == LFOMode.Tremolo || customLFOMode == LFOMode.Both;

        for (int i = 0; i < totalFrames; i++)
        {
            float t = (float)i / sampleRate;

            phase += increment;
            if (phase >= tableSize)
                phase -= tableSize;

            float raw = enemyWavetable[Mathf.FloorToInt(phase)];
            float env = CalculateADSREnvelope(t, customDuration);

            float stage = useFilter ? lowPassFilter.ProcessSample(raw, t) : raw;

            if (useTremolo)
            {
                float tremolo = (1f - customTremoloDepth) + customTremoloDepth * Mathf.Sin(2f * Mathf.PI * lfoRateHz * t);
                stage *= tremolo;
            }

            samples[i] = stage * env;
        }

        // Normalize
        NormalizeAudio(samples);

        // Crear AudioClip y volcar datos para que suene
        AudioClip clip = AudioClip.Create("EnemyTension", totalFrames, 1, sampleRate, false);
        clip.SetData(samples, 0);

        return clip;
    }

    private float CalculateADSREnvelope(float t, float noteDuration)
    {
        // Attack
        if (t < attack)
            return t / Mathf.Max(attack, 0.0001f);

        // Decay
        if (t < attack + decay)
        {
            float decayProgress = (t - attack) / Mathf.Max(decay, 0.0001f);
            return 1f + (sustain - 1f) * decayProgress;
        }

        // Sustain
        float releaseStart = noteDuration - release;
        if (t < releaseStart)
            return sustain;

        // Release
        float releaseProgress = Mathf.Clamp((t - releaseStart) / Mathf.Max(release, 0.0001f), 0f, 1f);
        return sustain + (0f - sustain) * releaseProgress;
    }

    private void NormalizeAudio(float[] samples)
    {
        float peak = 0f;
        foreach (float sample in samples)
        {
            peak = Mathf.Max(peak, Mathf.Abs(sample));
        }

        if (peak > 1f)
        {
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] /= peak;
            }
        }
    }

    // Low Pass Filter with LFO modulation
    private class LowPassFilter
    {
        private float sampleRate;
        private float minCutoff;
        private float maxCutoff;
        private float lfoRateHz;
        private float prevOutput;

        public LowPassFilter(float sampleRate, float minCutoff, float maxCutoff, float lfoRateHz)
        {
            this.sampleRate = sampleRate;
            this.minCutoff = minCutoff;
            this.maxCutoff = maxCutoff;
            this.lfoRateHz = lfoRateHz;
            this.prevOutput = 0f;
        }

        public void Reset()
        {
            prevOutput = 0f;
        }

        public float ProcessSample(float x, float t)
        {
            float lfo = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * lfoRateHz * t);
            float cutoff = minCutoff + lfo * (maxCutoff - minCutoff);
            float dt = 1f / sampleRate;
            float rc = 1f / (2f * Mathf.PI * Mathf.Max(cutoff, 20f));
            float alpha = dt / (rc + dt);
            prevOutput += alpha * (x - prevOutput);
            return prevOutput;
        }
    }

    // Controles por teclado (Z, X, C) para no interferir con la guitarra o el piano
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("Tensión (Modo Filtro) activada");
            lfoMode = LFOMode.Filter;
            PlayTensionSound();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("Tensión (Modo Tremolo) activada");
            lfoMode = LFOMode.Tremolo;
            PlayTensionSound();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Tensión (Modo Ambos) activada");
            lfoMode = LFOMode.Both;
            PlayTensionSound();
        }
    }
}