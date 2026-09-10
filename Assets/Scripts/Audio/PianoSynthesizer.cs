using UnityEngine;

public class PianoSynthesizer : MonoBehaviour
{
    // =========================================================
    // CONFIGURACIÓN GENERAL
    // =========================================================

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private int sampleRate = 44100;

    [SerializeField] private float noteDuration = 2.0f;

    [SerializeField] private float masterVolume = 0.8f;


    // =========================================================
    // ADSR
    // =========================================================

    [Header("ADSR")]

    [SerializeField] private float attack = 0.01f;

    [SerializeField] private float decay = 0.30f;

    [Range(0f, 1f)]
    [SerializeField] private float sustain = 0.20f;

    [SerializeField] private float release = 0.50f;


    // =========================================================
    // ARMÓNICOS
    // =========================================================

    [Header("Armónicos del Piano")]

    [SerializeField]
    private float[] harmonicAmplitudes =
    {
        1.00f,
        0.55f,
        0.35f,
        0.20f,
        0.12f,
        0.08f,
        0.05f,
        0.03f
    };


    // =========================================================
    // NOTAS
    // =========================================================

    public enum Note
    {
        Do,
        Re,
        Mi,
        Fa,
        Sol
    }


    // =========================================================
    // FRECUENCIAS
    // =========================================================

    private float GetFrequency(Note note)
    {
        switch (note)
        {
            case Note.Do:
                return 261.63f;

            case Note.Re:
                return 293.66f;

            case Note.Mi:
                return 329.63f;

            case Note.Fa:
                return 349.23f;

            case Note.Sol:
                return 392.00f;
        }

        return 261.63f;
    }


    // =========================================================
    // REPRODUCIR NOTA
    // =========================================================

    public void PlayNote(Note note)
    {
        float frequency = GetFrequency(note);

        AudioClip clip = GeneratePianoNote(
            note.ToString(),
            frequency
        );

        audioSource.clip = clip;
        audioSource.volume = masterVolume;

        audioSource.Play();
    }


    // =========================================================
    // GENERACIÓN DEL PIANO
    // =========================================================

    private AudioClip GeneratePianoNote(
        string noteName,
        float fundamentalFrequency)
    {
        int sampleCount =
            Mathf.CeilToInt(noteDuration * sampleRate);

        float[] samples = new float[sampleCount];


        // -----------------------------------------------------
        // GENERACIÓN DE LAS MUESTRAS
        // -----------------------------------------------------

        for (int i = 0; i < sampleCount; i++)
        {
            float time = (float)i / sampleRate;

            float sample = 0f;


            // -------------------------------------------------
            // SÍNTESIS ADITIVA
            // -------------------------------------------------

            for (int h = 0;
                 h < harmonicAmplitudes.Length;
                 h++)
            {
                int harmonicNumber = h + 1;

                float harmonicFrequency =
                    fundamentalFrequency * harmonicNumber;

                float harmonicAmplitude =
                    harmonicAmplitudes[h];


                sample +=
                    harmonicAmplitude *
                    Mathf.Sin(
                        2f *
                        Mathf.PI *
                        harmonicFrequency *
                        time
                    );
            }


            // -------------------------------------------------
            // NORMALIZACIÓN
            // -------------------------------------------------

            sample /= CalculateAmplitudeSum();


            // -------------------------------------------------
            // ADSR
            // -------------------------------------------------

            float envelope =
                GetADSR(time, noteDuration);

            sample *= envelope;


            // -------------------------------------------------
            // VOLUMEN
            // -------------------------------------------------

            sample *= masterVolume;


            // -------------------------------------------------
            // GUARDAR MUESTRA
            // -------------------------------------------------

            samples[i] = sample;
        }


        // =====================================================
        // CREAR AUDIOCLIP
        // =====================================================

        AudioClip clip =
            AudioClip.Create(
                "Piano_" + noteName,
                sampleCount,
                1,
                sampleRate,
                false
            );


        clip.SetData(samples, 0);

        return clip;
    }


    // =========================================================
    // ADSR
    // =========================================================

    private float GetADSR(
        float time,
        float duration)
    {
        // -----------------------------------------------------
        // ATTACK
        // -----------------------------------------------------

        if (time < attack)
        {
            return time / attack;
        }


        // -----------------------------------------------------
        // DECAY
        // -----------------------------------------------------

        if (time < attack + decay)
        {
            float decayTime =
                (time - attack) / decay;

            return Mathf.Lerp(
                1f,
                sustain,
                decayTime
            );
        }


        // -----------------------------------------------------
        // SUSTAIN
        // -----------------------------------------------------

        float releaseStart =
            duration - release;

        if (time < releaseStart)
        {
            return sustain;
        }


        // -----------------------------------------------------
        // RELEASE
        // -----------------------------------------------------

        float releaseTime =
            (time - releaseStart) / release;

        return Mathf.Lerp(
            sustain,
            0f,
            releaseTime
        );
    }


    // =========================================================
    // SUMA DE AMPLITUDES
    // =========================================================

    private float CalculateAmplitudeSum()
    {
        float sum = 0f;

        for (int i = 0;
             i < harmonicAmplitudes.Length;
             i++)
        {
            sum += Mathf.Abs(
                harmonicAmplitudes[i]
            );
        }

        return Mathf.Max(sum, 0.001f);
    }


    // =========================================================
    // PRUEBAS CON TECLADO
    // =========================================================

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayNote(Note.Do);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayNote(Note.Re);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PlayNote(Note.Mi);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            PlayNote(Note.Fa);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            PlayNote(Note.Sol);
        }
    }
}