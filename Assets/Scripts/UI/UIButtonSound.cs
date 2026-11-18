using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Controla exclusivamente el sonido de Hover (pasar el mouse) con pitch ajustable por slider.
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour, IPointerEnterHandler
{
    [Header("Audio Settings")]
    public AudioClip hoverSound; // Sonido al pasar el mouse

    [Header("Pitch Settings")]
    [Range(0.1f, 3f)] 
    public float hoverPitch = 1.0f; // Control deslizante (slider) para modificar el tono/pitch en el Inspector

    [Header("Audio Source")]
    public AudioSource audioSource;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
    }

    // Se ejecuta automáticamente cuando el mouse pasa por encima del botón (Hover)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null && audioSource != null)
        {
            audioSource.pitch = hoverPitch; // Aplica el tono configurado en el slider
            audioSource.PlayOneShot(hoverSound);
        }
    }
}