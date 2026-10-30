using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sintetizador aditivo de guitarra acústica.
/// Ya NO escucha el teclado: quien decide cuándo sonar es MelodyLevelManager.
/// </summary>
public class ProceduralGuitarSynth : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private int sampleRate = 44100;
    [SerializeField] private float masterVolume = 0.8f;

    [Header("ADSR Envelope")]
    [SerializeField] private float attack = 0.005f;
    [SerializeField] private float decay = 0.25f;
    [SerializeField, Range(0f, 1f)] private float sustain = 0.30f;
    [SerializeField] private float release = 0.80f;

    [Header("Pick Noise Settings")]
    [SerializeField] private float noiseIntensity = 0.015f;

    [Header("Harmonics - Guitar Acoustic Characteristics")]
    [SerializeField]
    private float[] harmonicAmplitudes =
    {
        1.00f, // Fundamental
        0.62f, // 2º armónico
        0.45f, // 3º
        0.30f, // 4º
        0.20f, // 5º
        0.12f, // 6º
        0.08f, // 7º
        0.05f  // 8º
    };

    [Header("Note Duration")]
    [SerializeField] private float noteDuration = 2.5f;

    private System.Random noiseRandom;
    private readonly Dictionary<NoteName, AudioClip> clipCache = new Dictionary<NoteName, AudioClip>();

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // sonido 2D: se oye igual en todo el mapa

        noiseRandom = new System.Random();
        PreGenerateClips();
    }

    [ContextMenu("Regenerar clips")]
    public void PreGenerateClips()
    {
        if (noiseRandom == null) noiseRandom = new System.Random();

        clipCache.Clear();
        foreach (NoteName note in System.Enum.GetValues(typeof(NoteName)))
        {
            clipCache[note] = GenerateGuitarSound(NoteUtils.GetFrequency(note));
        }
    }

    /// <summary>Reproduce una de las notas del juego.</summary>
    public void PlayNote(NoteName note)
    {
        if (!clipCache.TryGetValue(note, out AudioClip clip))
        {
            clip = GenerateGuitarSound(NoteUtils.GetFrequency(note));
            clipCache[note] = clip;
        }

        PlayClip(clip);
    }

    /// <summary>Reproduce una frecuencia libre (no se guarda en caché).</summary>
    public void PlayFrequency(float frequency)
    {
        PlayClip(GenerateGuitarSound(frequency));
    }

    private void PlayClip(AudioClip clip)
    {
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.volume = masterVolume;
        audioSource.Play();
    }

    /// <summary>Genera un AudioClip con el sonido de guitarra para la frecuencia dada.</summary>
    public AudioClip GenerateGuitarSound(float frequency)
    {
        int totalSamples = Mathf.CeilToInt(noteDuration * sampleRate);
        float[] samples = new float[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;
            float sample = 0f;

            // 1. Síntesis aditiva (armónicos)
            for (int h = 0; h < harmonicAmplitudes.Length; h++)
            {
                float harmonicFreq = frequency * (h + 1);
                sample += harmonicAmplitudes[h] * Mathf.Sin(2f * Mathf.PI * harmonicFreq * t);
            }

            // 2. Ruido de la uñeta al atacar la cuerda
            float pickNoise = Mathf.Exp(-25f * t);
            float noise = (float)(noiseRandom.NextDouble() * 2.0 - 1.0) * noiseIntensity * pickNoise;
            sample += noise;

            // 3. Envolvente ADSR
            sample *= CalculateADSREnvelope(t, noteDuration);

            samples[i] = sample;
        }

        // 4. Anti-clipping
        NormalizeAudio(samples);

        // 5. Crear el clip y volcar las muestras
        AudioClip clip = AudioClip.Create("Guitar_" + frequency + "Hz", totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private float CalculateADSREnvelope(float t, float duration)
    {
        // Attack
        if (t < attack)
            return t / Mathf.Max(attack, 0.0001f);

        // Decay
        if (t < attack + decay)
        {
            float d = (t - attack) / Mathf.Max(decay, 0.0001f);
            return (1f - d) + d * sustain;
        }

        // Sustain
        float sustainStart = attack + decay;
        float sustainEnd = Mathf.Max(sustainStart, duration - release);
        if (t < sustainEnd)
            return sustain;

        // Release
        if (t <= duration)
        {
            float r = (t - sustainEnd) / Mathf.Max(release, 0.0001f);
            return sustain * (1f - r);
        }

        return 0f;
    }

    private void NormalizeAudio(float[] samples)
    {
        float peak = 0f;
        foreach (float s in samples)
            peak = Mathf.Max(peak, Mathf.Abs(s));

        if (peak > 1f)
        {
            for (int i = 0; i < samples.Length; i++)
                samples[i] /= peak;
        }
    }

    /// <summary>Cambia la envolvente y regenera los clips.</summary>
    public void SetADSR(float newAttack, float newDecay, float newSustain, float newRelease)
    {
        attack = newAttack;
        decay = newDecay;
        sustain = newSustain;
        release = newRelease;
        PreGenerateClips();
    }

    /// <summary>Cambia los armónicos (mismo largo que el arreglo actual) y regenera los clips.</summary>
    public void SetHarmonics(float[] newHarmonics)
    {
        if (newHarmonics == null || newHarmonics.Length != harmonicAmplitudes.Length) return;

        for (int i = 0; i < harmonicAmplitudes.Length; i++)
            harmonicAmplitudes[i] = newHarmonics[i];

        PreGenerateClips();
    }
}