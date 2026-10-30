using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Se coloca en cada instrumento del mapa (guitarra o piano).
/// Se registra solo en una lista estática para que MelodyLevelManager sepa cuáles existen.
/// El instrumento "reconoce" al jugador cuando este entra en su radio.
/// </summary>
public class InstrumentStation : MonoBehaviour
{
    private static readonly List<InstrumentStation> all = new List<InstrumentStation>();
    public static IReadOnlyList<InstrumentStation> All => all;

    [Header("Instrumento")]
    [SerializeField] private InstrumentType instrument = InstrumentType.Guitar;
    [SerializeField] private float interactionRadius = 2f;

    [Header("Feedback visual (opcional)")]
    [Tooltip("Se activa cuando el jugador está cerca (brillo, notas flotando, icono de tecla...)")]
    [SerializeField] private GameObject nearIndicator;
    [Tooltip("Se activa cuando el jugador ya adivinó la nota de este instrumento")]
    [SerializeField] private GameObject solvedIndicator;

    [Header("Eventos (opcional)")]
    public UnityEvent onPlayerEnter;
    public UnityEvent onPlayerExit;
    public UnityEvent onSolved;

    public InstrumentType Instrument => instrument;
    public float InteractionRadius => interactionRadius;
    public bool IsSolved { get; private set; }
    public bool PlayerIsNear { get; private set; }

    private void OnEnable()
    {
        if (!all.Contains(this)) all.Add(this);
    }

    private void OnDisable()
    {
        all.Remove(this);
    }

    private void Start()
    {
        ResetStation();
    }

    public void SetPlayerNear(bool near)
    {
        if (PlayerIsNear == near) return;

        PlayerIsNear = near;
        if (nearIndicator != null) nearIndicator.SetActive(near && !IsSolved);

        if (near) onPlayerEnter.Invoke();
        else onPlayerExit.Invoke();
    }

    public void SetSolved()
    {
        IsSolved = true;
        PlayerIsNear = false;

        if (nearIndicator != null) nearIndicator.SetActive(false);
        if (solvedIndicator != null) solvedIndicator.SetActive(true);

        onSolved.Invoke();
    }

    public void ResetStation()
    {
        IsSolved = false;
        PlayerIsNear = false;

        if (nearIndicator != null) nearIndicator.SetActive(false);
        if (solvedIndicator != null) solvedIndicator.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
