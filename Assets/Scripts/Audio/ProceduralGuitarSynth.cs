using UnityEngine;

/// <summary>
/// Procedural Guitar Synthesizer - Sintetizador aditivo de guitarra acústica
/// Genera sonidos de guitarra pulsada con ataque rápido, decaimiento progresivo y resonancia cálida
/// </summary>
public class ProceduralGuitarSynth : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private int sampleRate = 44100;
    [SerializeField] private float masterVolume = 0.8f;

    [Header("Note Settings")]
    public enum GuitarNote
    {
        Do,
        Re,
        Mi,
        Fa,
        Sol,
        Custom
    }

    [Header("ADSR Envelope")]
    [SerializeField] private float attack = 0.005f;
    [SerializeField] private float decay = 0.25f;
    [SerializeField] [Range(0f, 1f)] private float sustain = 0.30f;
    [SerializeField] private float release = 0.80f;

    [Header("Pick Noise Settings")]
    [SerializeField] private float noiseIntensity = 0.015f;

    [Header("Harmonics - Guitar Acoustic Characteristics")]
    [SerializeField]
    private float[] harmonicAmplitudes = new float[]
    {
        1.00f,  // Fundamental
        0.62f,  // 2nd harmonic
        0.45f,  // 3rd harmonic
        0.30f,  // 4th harmonic
        0.20f,  // 5th harmonic
        0.12f,  // 6th harmonic
        0.08f,  // 7th harmonic
        0.05f   // 8th harmonic
    };

    [Header("Note Duration")]
    [SerializeField] private float noteDuration = 2.5f;

    private System.Random noiseRandom;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        noiseRandom = new System.Random();
    }

    /// <summary>
    /// Reproduce una nota específica de guitarra
    /// </summary>
    public void PlayNote(GuitarNote note, float customFrequency = 261.63f)
    {
        float frequency = GetNoteFrequency(note, customFrequency);
        AudioClip clip = GenerateGuitarSound(frequency);
        audioSource.clip = clip;
        audioSource.volume = masterVolume;
        audioSource.Play();
    }

    /// <summary>
    /// Reproduce una nota específica por nombre
    /// </summary>
    public void PlayNoteByName(string noteName)
    {
        GuitarNote note = ParseNoteName(noteName);
        float frequency = GetNoteFrequency(note, 261.63f);
        AudioClip clip = GenerateGuitarSound(frequency);
        audioSource.clip = clip;
        audioSource.volume = masterVolume;
        audioSource.Play();
    }

    /// <summary>
    /// Genera un AudioClip con el sonido de guitarra especificado
    /// </summary>
    public AudioClip GenerateGuitarSound(float frequency)
    {
        int totalSamples = Mathf.CeilToInt(noteDuration * sampleRate);
        float[] samples = new float[totalSamples];

        // 1. Síntesis aditiva (armónicos)
        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;
            float sample = 0f;

            // Suma de armónicos
            for (int h = 0; h < harmonicAmplitudes.Length; h++)
            {
                int harmonicNumber = h + 1;
                float harmonicFreq = frequency * harmonicNumber;
                float harmonicAmp = harmonicAmplitudes[h];
                sample += harmonicAmp * Mathf.Sin(2f * Mathf.PI * harmonicFreq * t);
            }

            // 2. Ruido del rasgueo / ataque de la uñeta (Pick noise)
            float pickNoise = Mathf.Exp(-25f * t);
            float noise = (float)(noiseRandom.NextDouble() * 2f - 1f) * noiseIntensity * pickNoise;
            sample += noise;

            // 3. Aplicación de la envolvente ADSR
            float envelope = CalculateADSREnvelope(t, noteDuration);
            sample *= envelope;

            samples[i] = sample;
        }

        // 4. Normalización anti-clipping
        NormalizeAudio(samples);

        return AudioClip.Create("Guitar_" + frequency + "Hz", totalSamples, 1, sampleRate, false);
    }

    private float GetNoteFrequency(GuitarNote note, float customFrequency)
    {
        switch (note)
        {
            case GuitarNote.Do:
                return 261.63f;  // C4
            case GuitarNote.Re:
                return 293.66f;  // D4
            case GuitarNote.Mi:
                return 329.63f;  // E4
            case GuitarNote.Fa:
                return 349.23f;  // F4
            case GuitarNote.Sol:
                return 392.00f;  // G4
            case GuitarNote.Custom:
                return customFrequency;
            default:
                return 261.63f;
        }
    }

    private GuitarNote ParseNoteName(string noteName)
    {
        switch (noteName.ToLower())
        {
            case "do":
            case "c":
                return GuitarNote.Do;
            case "re":
            case "d":
                return GuitarNote.Re;
            case "mi":
            case "e":
                return GuitarNote.Mi;
            case "fa":
            case "f":
                return GuitarNote.Fa;
            case "sol":
            case "g":
                return GuitarNote.Sol;
            default:
                return GuitarNote.Do;
        }
    }

    private float CalculateADSREnvelope(float t, float noteDuration)
    {
        // 1. Attack
        if (t < attack)
            return t / Mathf.Max(attack, 0.0001f);

        // 2. Decay
        if (t < attack + decay)
        {
            float d = (t - attack) / Mathf.Max(decay, 0.0001f);
            return (1f - d) + d * sustain;
        }

        // 3. Sustain
        float sustainStart = attack + decay;
        float sustainEnd = Mathf.Max(sustainStart, noteDuration - release);
        if (t < sustainEnd)
            return sustain;

        // 4. Release
        if (t <= noteDuration)
        {
            float r = (t - sustainEnd) / Mathf.Max(release, 0.0001f);
            return sustain * (1f - r);
        }

        return 0f;
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

    /// <summary>
    /// Configura los parámetros de la envolvente ADSR
    /// </summary>
    public void SetADSR(float newAttack, float newDecay, float newSustain, float newRelease)
    {
        attack = newAttack;
        decay = newDecay;
        sustain = newSustain;
        release = newRelease;
    }

    /// <summary>
    /// Configura los armónicos para diferentes timbres de guitarra
    /// </summary>
    public void SetHarmonics(float[] newHarmonics)
    {
        if (newHarmonics != null && newHarmonics.Length == harmonicAmplitudes.Length)
        {
            for (int i = 0; i < harmonicAmplitudes.Length; i++)
            {
                harmonicAmplitudes[i] = newHarmonics[i];
            }
        }
    }

    // For testing in editor
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayNote(GuitarNote.Do);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayNote(GuitarNote.Re);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PlayNote(GuitarNote.Mi);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            PlayNote(GuitarNote.Fa);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            PlayNote(GuitarNote.Sol);
        }
    }
}