using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Controla la mecánica del nivel:
/// 1) Al inicio genera una melodía al azar de 2 notas (una de guitarra y una de piano).
/// 2) El jugador la escucha con el botón Play inicial (que se quita con la X) o con el botón fijo de repetir.
/// 3) Cerca de un instrumento, las teclas 1-5 tocan Do-Re-Mi-Fa-Sol (iguales para todos los instrumentos).
/// 4) Con el botón "Seleccionar" confirma su nota. Si acierta, debe buscar el siguiente instrumento.
/// </summary>
public class MelodyLevelManager : MonoBehaviour
{
    [System.Serializable]
    public struct MelodyStep
    {
        public InstrumentType instrument;
        public NoteName note;
    }

    [Header("Jugador")]
    [Tooltip("Si se deja vacío, se busca un objeto con el tag 'Player'")]
    [SerializeField] private Transform player;

    [Header("Sintetizadores (cada uno en su propio GameObject con su AudioSource)")]
    [SerializeField] private ProceduralGuitarSynth guitarSynth;
    [SerializeField] private PianoSynthesizer pianoSynth;

    [Header("Reglas")]
    [Tooltip("Si está activo, hay que resolver los instrumentos en el mismo orden en que suena la melodía")]
    [SerializeField] private bool requireMelodyOrder = true;
    [SerializeField] private float delayBetweenMelodyNotes = 1.5f;

    [Header("Teclas (índice 0 = Do, 1 = Re, 2 = Mi, 3 = Fa, 4 = Sol)")]
    [SerializeField]
    private KeyCode[] noteKeys =
    {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5
    };
    [SerializeField] private string keysHintLabel = "1=Do  2=Re  3=Mi  4=Fa  5=Sol";

    [Header("UI - Inicio del nivel")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Button startPlayButton;
    [SerializeField] private Button startCloseButton;

    [Header("UI - Siempre disponible")]
    [Tooltip("Botón pequeño fijo para volver a escuchar la melodía (aparece al cerrar el panel inicial)")]
    [SerializeField] private Button replayButton;
    [Tooltip("Botón para confirmar la nota elegida (solo visible cerca de un instrumento)")]
    [SerializeField] private Button selectButton;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text progressText;

    [Header("Eventos")]
    public UnityEvent onLevelCompleted;
    /// <summary>Cuántos instrumentos se han resuelto: 0, 1 o 2.</summary>
    public static event System.Action<int> OnProgressChanged;

    private readonly List<MelodyStep> melody = new List<MelodyStep>();
    private readonly HashSet<InstrumentType> solvedInstruments = new HashSet<InstrumentType>();

    private InstrumentStation currentStation;
    private NoteName? selectedNote;
    private Coroutine melodyRoutine;
    private bool levelCompleted;
    private string lastResult = "";

    // =========================================================
    // CICLO DE VIDA
    // =========================================================

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
            else Debug.LogError("MelodyLevelManager: asigna el Player o ponle el tag 'Player'.");
        }

        Bind(startPlayButton, PlayMelody);
        Bind(startCloseButton, CloseStartPanel);
        Bind(replayButton, PlayMelody);
        Bind(selectButton, ConfirmSelection);

        StartLevel();
    }

    private static void Bind(Button button, UnityAction action)
    {
        if (button != null) button.onClick.AddListener(action);
    }

    /// <summary>Reinicia el nivel con una melodía nueva.</summary>
    public void StartLevel()
    {
        if (melodyRoutine != null)
        {
            StopCoroutine(melodyRoutine);
            melodyRoutine = null;
        }

        levelCompleted = false;
        solvedInstruments.Clear();
        OnProgressChanged?.Invoke(0);
        currentStation = null;
        selectedNote = null;
        lastResult = "";

        foreach (InstrumentStation station in InstrumentStation.All)
            station.ResetStation();

        GenerateMelody();

        if (startPanel != null) startPanel.SetActive(true);
        if (replayButton != null) replayButton.gameObject.SetActive(false);
        SetMelodyButtonsInteractable(true);

        lastResult = "Presiona Play para escuchar la melodía.";
        RefreshUI();
    }

    // =========================================================
    // MELODÍA
    // =========================================================

    private void GenerateMelody()
    {
        melody.Clear();

        int noteCount = System.Enum.GetValues(typeof(NoteName)).Length;

        MelodyStep guitarStep = new MelodyStep
        {
            instrument = InstrumentType.Guitar,
            note = (NoteName)Random.Range(0, noteCount)
        };

        MelodyStep pianoStep = new MelodyStep
        {
            instrument = InstrumentType.Piano,
            note = (NoteName)Random.Range(0, noteCount)
        };

        // El orden en que suenan también es al azar
        if (Random.value < 0.5f)
        {
            melody.Add(guitarStep);
            melody.Add(pianoStep);
        }
        else
        {
            melody.Add(pianoStep);
            melody.Add(guitarStep);
        }

        Debug.Log($"Melodía generada: {melody[0].instrument} {melody[0].note} -> {melody[1].instrument} {melody[1].note}");
    }

    public void PlayMelody()
    {
        if (melodyRoutine != null) return; // ya está sonando
        melodyRoutine = StartCoroutine(PlayMelodyRoutine());
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }

    private IEnumerator PlayMelodyRoutine()
    {
        SetMelodyButtonsInteractable(false);

        for (int i = 0; i < melody.Count; i++)
        {
            PlayNoteOn(melody[i].instrument, melody[i].note);
            yield return new WaitForSeconds(delayBetweenMelodyNotes);
        }

        SetMelodyButtonsInteractable(true);
        melodyRoutine = null;
    }

    private void PlayNoteOn(InstrumentType instrument, NoteName note)
    {
        switch (instrument)
        {
            case InstrumentType.Guitar:
                if (guitarSynth != null) guitarSynth.PlayNote(note);
                break;
            case InstrumentType.Piano:
                if (pianoSynth != null) pianoSynth.PlayNote(note);
                break;
        }
    }

    private void SetMelodyButtonsInteractable(bool interactable)
    {
        if (startPlayButton != null) startPlayButton.interactable = interactable;
        if (replayButton != null) replayButton.interactable = interactable;
    }

    private void CloseStartPanel()
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (replayButton != null) replayButton.gameObject.SetActive(true);

        lastResult = "";
        RefreshUI();
    }

    // =========================================================
    // ACTUALIZACIÓN: CERCANÍA + TECLAS
    // =========================================================


    private void Update()
    {
        if (levelCompleted) return;

        // El jugador se instancia en tiempo de ejecución, así que se busca hasta encontrarlo
        if (player == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
            return;
        }

        UpdateNearestStation();
        HandleNoteKeys();
    }

    private void UpdateNearestStation()
    {

        IReadOnlyList<InstrumentStation> stations = InstrumentStation.All;

        InstrumentStation nearest = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < stations.Count; i++)
        {
            InstrumentStation station = stations[i];
            if (station.IsSolved) continue;

            float distance = Vector2.Distance(player.position, station.transform.position);
            if (distance <= station.InteractionRadius && distance < bestDistance)
            {
                bestDistance = distance;
                nearest = station;
            }
        }

        for (int i = 0; i < stations.Count; i++)
        {
            if (!stations[i].IsSolved)
                stations[i].SetPlayerNear(stations[i] == nearest);
        }

        if (nearest != currentStation)
        {
            currentStation = nearest;
            selectedNote = null;
            lastResult = "";
            RefreshUI();
        }
    }
    private void HandleNoteKeys()
    {
        // Las teclas solo funcionan cerca de un instrumento (y no mientras suena la melodía)
        if (currentStation == null || melodyRoutine != null) return;
        if (!CanInteract(currentStation)) return;

        int noteCount = System.Enum.GetValues(typeof(NoteName)).Length;

        for (int i = 0; i < noteKeys.Length && i < noteCount; i++)
        {
            if (Input.GetKeyDown(noteKeys[i]))
            {
                NoteName note = (NoteName)i;
                PlayNoteOn(currentStation.Instrument, note);
                selectedNote = note;
                lastResult = "";
                RefreshUI();
                break;
            }
        }
    }

    // =========================================================
    // CONFIRMAR NOTA
    // =========================================================

    private void ConfirmSelection()
    {
        if (currentStation == null || !selectedNote.HasValue || !CanInteract(currentStation)) return;

        InstrumentType type = currentStation.Instrument;
        MelodyStep target = melody.Find(step => step.instrument == type);

        if (selectedNote.Value == target.note)
        {
            solvedInstruments.Add(type);
            OnProgressChanged?.Invoke(solvedInstruments.Count);
            currentStation.SetSolved();
            currentStation = null;
            selectedNote = null;

            if (solvedInstruments.Count >= melody.Count)
            {
                levelCompleted = true;
                lastResult = "¡Melodía completa!";
                onLevelCompleted.Invoke();
            }
            else
            {
                lastResult = "¡Correcto!";
            }
        }
        else
        {
            lastResult = "Esa no es la nota. Prueba otra.";
            selectedNote = null;
        }

        RefreshUI();
    }

    // =========================================================
    // REGLAS
    // =========================================================

    private int GetNextStepIndex()
    {
        for (int i = 0; i < melody.Count; i++)
        {
            if (!solvedInstruments.Contains(melody[i].instrument)) return i;
        }
        return -1;
    }

    private bool CanInteract(InstrumentStation station)
    {
        if (station == null || station.IsSolved) return false;
        if (solvedInstruments.Contains(station.Instrument)) return false;

        if (requireMelodyOrder)
        {
            int next = GetNextStepIndex();
            return next >= 0 && melody[next].instrument == station.Instrument;
        }

        return true;
    }

    // =========================================================
    // UI
    // =========================================================

    private void RefreshUI()
    {
        bool near = currentStation != null;
        bool canInteract = near && CanInteract(currentStation);

        if (selectButton != null)
        {
            selectButton.gameObject.SetActive(canInteract && !levelCompleted);
            selectButton.interactable = selectedNote.HasValue;
        }

        if (progressText != null)
            progressText.text = $"Instrumentos: {solvedInstruments.Count}/{melody.Count}";

        if (feedbackText != null)
        {
            string hint = BuildHint(near, canInteract);
            feedbackText.text = string.IsNullOrEmpty(lastResult) ? hint : (string.IsNullOrEmpty(hint) ? lastResult : lastResult + "\n" + hint);
        }
    }

    private string BuildHint(bool near, bool canInteract)
    {
        if (levelCompleted) return "";

        if (near && !canInteract)
        {
            int next = GetNextStepIndex();
            return next >= 0
                ? $"Primero busca: {NoteUtils.GetDisplayName(melody[next].instrument)}"
                : "";
        }

        if (near)
        {
            string name = NoteUtils.GetDisplayName(currentStation.Instrument);
            return selectedNote.HasValue
                ? $"{name} - nota elegida: {selectedNote.Value}\nPulsa Seleccionar para confirmar"
                : $"{name}: prueba las notas\n{keysHintLabel}";
        }

        // Lejos de todo instrumento
        if (requireMelodyOrder)
        {
            int next = GetNextStepIndex();
            return next >= 0 ? $"Busca: {NoteUtils.GetDisplayName(melody[next].instrument)}" : "";
        }

        return "Busca los instrumentos del mapa";
    }
}
