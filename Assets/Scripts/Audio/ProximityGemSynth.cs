using UnityEngine;

/// <summary>
/// Proximity Gem Synth - Síntesis por wavetable con sonido brillante que aumenta con la proximidad
/// Usado para indicar cercanía a instrumentos correctos o fragmentos musicales
/// </summary>
public class ProximityGemSynth : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private int sampleRate = 44100;
    [SerializeField] private float masterVolume = 0.8f;

    [Header("Pad Settings")]
    [SerializeField] private float padFrequency = 1318.5f;
    [SerializeField] private float padMinVolume = 0.04f;
    [SerializeField] private float padMaxVolume = 0.35f;
    [SerializeField] private float vibratoMinRateHz = 3.0f;
    [SerializeField] private float vibratoMaxRateHz = 7.0f;
    [SerializeField] private float vibratoMinDepthHz = 2.0f;
    [SerializeField] private float vibratoMaxDepthHz = 12.0f;

    [Header("Sparkle Settings")]
    [SerializeField] private float sparkleMinRatePerSecond = 0.5f;
    [SerializeField] private float sparkleMaxRatePerSecond = 10.0f;
    [SerializeField] private float sparkleMinFrequency = 2800.0f;
    [SerializeField] private float sparkleMaxFrequency = 6000.0f;
    [SerializeField] private float sparkleMinLevel = 0.05f;
    [SerializeField] private float sparkleMaxLevel = 0.35f;

    [Header("Proximity Settings")]
    [SerializeField] private float demoDuration = 5.0f;
    [SerializeField] private int sparkleSeed = 1;

    private float[] shimmerTable;
    private float[] sineTable;
    private const int tableSize = 1024;

    void Start()
    {
        Debug.Log("ProximityGemSynth inicializado correctamente");

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("ProximityGemSynth: No hay AudioSource en este GameObject. Añade un componente AudioSource.");
                return;
            }
            Debug.Log("AudioSource encontrado automáticamente con GetComponent");
        }

        if (audioSource != null)
            Debug.Log("AudioSource conectado correctamente");

        InitializeWavetables();
    }

    void InitializeWavetables()
    {
        shimmerTable = new float[tableSize];
        sineTable = new float[tableSize];

        for (int i = 0; i < tableSize; i++)
        {
            float phase = (float)i / tableSize;
            float sine = Mathf.Sin(2f * Mathf.PI * phase);
            float triangle = 2f * Mathf.Abs(2f * phase - 1f) - 1f;
            shimmerTable[i] = 0.7f * sine + 0.3f * triangle;
            sineTable[i] = Mathf.Sin(2f * Mathf.PI * phase);
        }
    }

    /// <summary>
    /// Genera sonido basado en curva de proximidad (0 = lejos, 1 = cerca)
    /// </summary>
    public void PlayProximitySound(float proximity)
    {
        AudioClip clip = GenerateProximitySound(demoDuration, proximity);
        audioSource.clip = clip;
        audioSource.volume = masterVolume;
        audioSource.Play();
    }

    /// <summary>
    /// Genera sonido con curva de proximidad que cambia con el tiempo
    /// </summary>
    public void PlayApproachingSound()
    {
        AudioClip clip = GenerateApproachingSound(demoDuration);
        audioSource.clip = clip;
        audioSource.volume = masterVolume;
        audioSource.Play();
    }

    private AudioClip GenerateProximitySound(float duration, float fixedProximity)
    {
        int totalFrames = Mathf.CeilToInt(duration * sampleRate);
        float[] samples = new float[totalFrames];

        float phase = 0f;
        System.Random rng = new System.Random(sparkleSeed);

        for (int i = 0; i < totalFrames; i++)
        {
            float t = (float)i / sampleRate;
            float proximity = Mathf.Clamp01(fixedProximity);

            // Vibrato
            float vibratoRate = Mathf.Lerp(vibratoMinRateHz, vibratoMaxRateHz, proximity);
            float vibratoDepth = Mathf.Lerp(vibratoMinDepthHz, vibratoMaxDepthHz, proximity);
            float vibratoOffset = vibratoDepth * Mathf.Sin(2f * Mathf.PI * vibratoRate * t);

            float instantFreq = padFrequency + vibratoOffset;
            float increment = instantFreq * tableSize / sampleRate;

            phase += increment;
            if (phase >= tableSize)
                phase -= tableSize;

            float raw = shimmerTable[Mathf.FloorToInt(phase)];
            float volume = Mathf.Lerp(padMinVolume, padMaxVolume, proximity);
            samples[i] = raw * volume;
        }

        // Add sparkles
        AddSparkles(samples, duration, fixedProximity, rng);

        // Normalize
        NormalizeAudio(samples);

        // ¡CORRECCIÓN CRUCIAL: Crear y volcar los datos al AudioClip para que suene!
        AudioClip clip = AudioClip.Create("ProximitySound", totalFrames, 1, sampleRate, false);
        clip.SetData(samples, 0);

        return clip;
    }

    private AudioClip GenerateApproachingSound(float duration)
    {
        int totalFrames = Mathf.CeilToInt(duration * sampleRate);
        float[] samples = new float[totalFrames];

        float phase = 0f;
        System.Random rng = new System.Random(sparkleSeed);

        for (int i = 0; i < totalFrames; i++)
        {
            float t = (float)i / sampleRate;
            float normT = t / duration;
            float proximity = Mathf.Clamp01(0.05f + normT * 0.95f);

            // Vibrato
            float vibratoRate = Mathf.Lerp(vibratoMinRateHz, vibratoMaxRateHz, proximity);
            float vibratoDepth = Mathf.Lerp(vibratoMinDepthHz, vibratoMaxDepthHz, proximity);
            float vibratoOffset = vibratoDepth * Mathf.Sin(2f * Mathf.PI * vibratoRate * t);

            float instantFreq = padFrequency + vibratoOffset;
            float increment = instantFreq * tableSize / sampleRate;

            phase += increment;
            if (phase >= tableSize)
                phase -= tableSize;

            float raw = shimmerTable[Mathf.FloorToInt(phase)];
            float volume = Mathf.Lerp(padMinVolume, padMaxVolume, proximity);
            samples[i] = raw * volume;
        }

        // Add sparkles with varying proximity
        AddSparklesWithVaryingProximity(samples, duration, rng);

        // Normalize
        NormalizeAudio(samples);

        // ¡CORRECCIÓN CRUCIAL: Crear y volcar los datos al AudioClip para que suene!
        AudioClip clip = AudioClip.Create("ApproachingSound", totalFrames, 1, sampleRate, false);
        clip.SetData(samples, 0);

        return clip;
    }

    private void AddSparkles(float[] samples, float duration, float fixedProximity, System.Random rng)
    {
        float t = 0f;
        while (t < duration)
        {
            float rate = Mathf.Lerp(sparkleMinRatePerSecond, sparkleMaxRatePerSecond, fixedProximity);
            float meanInterval = 1f / Mathf.Max(rate, 0.01f);
            float interval = -Mathf.Log(1f - (float)rng.NextDouble()) * meanInterval;
            t += interval;

            if (t >= duration)
                break;

            float freq = sparkleMinFrequency + (float)(rng.NextDouble() * (sparkleMaxFrequency - sparkleMinFrequency));
            float level = Mathf.Lerp(sparkleMinLevel, sparkleMaxLevel, fixedProximity) * (0.7f + 0.3f * (float)rng.NextDouble());
            float sparkleDuration = 0.05f + (float)(rng.NextDouble() * 0.05f);

            RenderSparkle(samples, t, sparkleDuration, freq, level);
        }
    }

    private void AddSparklesWithVaryingProximity(float[] samples, float duration, System.Random rng)
    {
        float t = 0f;
        while (t < duration)
        {
            float normT = t / duration;
            float proximity = Mathf.Clamp01(0.05f + normT * 0.95f);

            float rate = Mathf.Lerp(sparkleMinRatePerSecond, sparkleMaxRatePerSecond, proximity);
            float meanInterval = 1f / Mathf.Max(rate, 0.01f);
            float interval = -Mathf.Log(1f - (float)rng.NextDouble()) * meanInterval;
            t += interval;

            if (t >= duration)
                break;

            float freq = sparkleMinFrequency + (float)(rng.NextDouble() * (sparkleMaxFrequency - sparkleMinFrequency));
            float level = Mathf.Lerp(sparkleMinLevel, sparkleMaxLevel, proximity) * (0.7f + 0.3f * (float)rng.NextDouble());
            float sparkleDuration = 0.05f + (float)(rng.NextDouble() * 0.05f);

            RenderSparkle(samples, t, sparkleDuration, freq, level);
        }
    }

    private void RenderSparkle(float[] samples, float startTime, float noteDuration, float frequency, float level)
    {
        int startSample = Mathf.FloorToInt(startTime * sampleRate);
        int frames = Mathf.FloorToInt(noteDuration * sampleRate);

        float phase = 0f;
        float increment = frequency * tableSize / sampleRate;

        for (int i = 0; i < frames; i++)
        {
            int targetIndex = startSample + i;
            if (targetIndex < 0 || targetIndex >= samples.Length)
                continue;

            float t = (float)i / sampleRate;
            phase += increment;
            if (phase >= tableSize)
                phase -= tableSize;

            float raw = sineTable[Mathf.FloorToInt(phase)];
            float env = CalculateSparkleEnvelope(t, noteDuration);
            samples[targetIndex] += raw * env * level;
        }
    }

    private float CalculateSparkleEnvelope(float t, float duration)
    {
        float attack = 0.001f;
        float decay = duration * 0.5f;
        float sustain = 0f;
        float release = duration * 0.5f;

        if (t < attack)
            return t / Mathf.Max(attack, 0.0001f);
        if (t < attack + decay)
        {
            float decayProgress = (t - attack) / Mathf.Max(decay, 0.0001f);
            return 1f + (sustain - 1f) * decayProgress;
        }

        float releaseStart = duration - release;
        if (t < releaseStart)
            return sustain;

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

    // Controles por teclado de prueba (J, K, L) para no interferir con otros instrumentos
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("Proximidad: Lejos (0.15)");
            PlayProximitySound(0.15f); 
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Proximidad: Cerca (0.9)");
            PlayProximitySound(0.9f); 
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("Proximidad: Acercándose dinámicamente");
            PlayApproachingSound(); 
        }
    }
}