using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sintetizador aditivo de piano.
/// Ya NO escucha el teclado: quien decide cuándo sonar es MelodyLevelManager.
/// </summary>
public class PianoSynthesizer : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private int sampleRate = 44100;
    [SerializeField] private float noteDuration = 2.0f;
    [SerializeField] private float masterVolume = 0.8f;

    [Header("ADSR")]
    [SerializeField] private float attack = 0.01f;
    [SerializeField] private float decay = 0.30f;
    [Range(0f, 1f)]
    [SerializeField] private float sustain = 0.20f;
    [SerializeField] private float release = 0.50f;

    [Header("Armónicos del Piano")]
    [SerializeField]
    private float[] harmonicAmplitudes =
    {
        1.00f, 0.55f, 0.35f, 0.20f, 0.12f, 0.08f, 0.05f, 0.03f
    };

    // Se generan una sola vez para no calcular miles de senos cada vez que suena una nota
    private readonly Dictionary<NoteName, AudioClip> clipCache = new Dictionary<NoteName, AudioClip>();

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // sonido 2D: se oye igual en todo el mapa

        PreGenerateClips();
    }

    [ContextMenu("Regenerar clips")]
    public void PreGenerateClips()
    {
        clipCache.Clear();
        foreach (NoteName note in System.Enum.GetValues(typeof(NoteName)))
        {
            clipCache[note] = GeneratePianoNote(note.ToString(), NoteUtils.GetFrequency(note));
        }
    }

    public void PlayNote(NoteName note)
    {
        if (!clipCache.TryGetValue(note, out AudioClip clip))
        {
            clip = GeneratePianoNote(note.ToString(), NoteUtils.GetFrequency(note));
            clipCache[note] = clip;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.volume = masterVolume;
        audioSource.Play();
    }

    private AudioClip GeneratePianoNote(string noteName, float fundamentalFrequency)
    {
        int sampleCount = Mathf.CeilToInt(noteDuration * sampleRate);
        float[] samples = new float[sampleCount];
        float amplitudeSum = CalculateAmplitudeSum();

        for (int i = 0; i < sampleCount; i++)
        {
            float time = (float)i / sampleRate;
            float sample = 0f;

            // Síntesis aditiva
            for (int h = 0; h < harmonicAmplitudes.Length; h++)
            {
                float harmonicFrequency = fundamentalFrequency * (h + 1);
                sample += harmonicAmplitudes[h] * Mathf.Sin(2f * Mathf.PI * harmonicFrequency * time);
            }

            // Normalización + ADSR
            sample /= amplitudeSum;
            sample *= GetADSR(time, noteDuration);

            samples[i] = sample;
        }

        AudioClip clip = AudioClip.Create("Piano_" + noteName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private float GetADSR(float time, float duration)
    {
        // Attack
        if (time < attack)
            return time / Mathf.Max(attack, 0.0001f);

        // Decay
        if (time < attack + decay)
        {
            float decayTime = (time - attack) / Mathf.Max(decay, 0.0001f);
            return Mathf.Lerp(1f, sustain, decayTime);
        }

        // Sustain
        float releaseStart = duration - release;
        if (time < releaseStart)
            return sustain;

        // Release
        float releaseTime = (time - releaseStart) / Mathf.Max(release, 0.0001f);
        return Mathf.Lerp(sustain, 0f, releaseTime);
    }

    private float CalculateAmplitudeSum()
    {
        float sum = 0f;
        for (int i = 0; i < harmonicAmplitudes.Length; i++)
            sum += Mathf.Abs(harmonicAmplitudes[i]);

        return Mathf.Max(sum, 0.001f);
    }
}