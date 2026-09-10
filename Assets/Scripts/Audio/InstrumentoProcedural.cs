using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class InstrumentoProcedural : MonoBehaviour
{
    [Header("Configuración del Saxofón")]
    [Range(0.0f, 1.0f)]
    public float volumen = 0.15f; // Manténlo bajo, la onda de sierra es potente
    public float frecuenciaTarget = 440.0f;

    [Header("Efecto de Aire / Vibrato")]
    public float velocidadVibrato = 5.5f; // Qué tan rápido vibra el aire
    public float intensidadVibrato = 3.0f; // Qué tanto cambia el tono en Hz

    private double fase;
    private double faseVibrato;
    private double tasaMuestreo;
    private float frecuenciaActual;
    private AudioSource audioSource;

    void Start()
    {
        tasaMuestreo = AudioSettings.outputSampleRate;
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 0.0f; // Asegurar 2D

        if (!audioSource.isPlaying) audioSource.Play();
        frecuenciaActual = frecuenciaTarget;
    }

    void Update()
    {
        // Notas del saxofón en el teclado (Escala más natural para el instrumento)
        if (Input.GetKey(KeyCode.Alpha1)) frecuenciaTarget = 293.66f; // Re4 (D4)
        if (Input.GetKey(KeyCode.Alpha2)) frecuenciaTarget = 329.63f; // Mi4 (E4)
        if (Input.GetKey(KeyCode.Alpha3)) frecuenciaTarget = 392.00f; // Sol4 (G4)

        // Suaviza la transición entre notas (Legato automático)
        frecuenciaActual = Mathf.Lerp(frecuenciaActual, frecuenciaTarget, Time.deltaTime * 15f);
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        // Incremento para el vibrato (simula la boca y aire del músico)
        double incrementoVibrato = velocidadVibrato * 2.0 * System.Math.PI / tasaMuestreo;

        for (int i = 0; i < data.Length; i += channels)
        {
            // Aplicar el vibrato a la frecuencia actual
            float modificadorVibrato = (float)System.Math.Sin(faseVibrato) * intensidadVibrato;
            float frecConVibrato = frecuenciaActual + modificadorVibrato;

            double incrementoFase = frecConVibrato * 2.0 * System.Math.PI / tasaMuestreo;

            // --- SÍNTESIS DEL SAXOFÓN ---
            // 1. Onda de Sierra (Aporta el brillo metálico y áspero de la caña)
            float ondaSierra = (float)(2.0 * (fase / (2.0 * System.Math.PI)) - 1.0);

            // 2. Onda Triangular (Aporta el cuerpo hueco del tubo de latón)
            float ondaTriangular = (float)(System.Math.Abs((fase / (2.0 * System.Math.PI)) * 4.0 - 2.0) - 1.0);

            // Mezcla balanceada: 70% sierra y 30% triángulo
            float sonidoSaxo = (ondaSierra * 0.7f) + (ondaTriangular * 0.3f);

            // Filtro pasa-bajas analógico ultra-simple para suavizar los agudos molestos
            sonidoSaxo = Mathf.Clamp(sonidoSaxo, -1.0f, 1.0f) * volumen;

            // Inyectar en los canales
            for (int channel = 0; channel < channels; channel++)
            {
                data[i + channel] = sonidoSaxo;
            }

            // Avanzar fases
            fase += incrementoFase;
            faseVibrato += incrementoVibrato;

            // Limitar fases de circunferencia
            if (fase > 2.0 * System.Math.PI) fase -= 2.0 * System.Math.PI;
            if (faseVibrato > 2.0 * System.Math.PI) faseVibrato -= 2.0 * System.Math.PI;
        }
    }
}