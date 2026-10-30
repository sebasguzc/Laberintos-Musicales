/// <summary>
/// Notas disponibles en el juego. El orden importa: la tecla 1 = Do, 2 = Re, etc.
/// </summary>
public enum NoteName
{
    Do,
    Re,
    Mi,
    Fa,
    Sol
}

/// <summary>
/// Instrumentos que puede haber en el mapa.
/// </summary>
public enum InstrumentType
{
    Guitar,
    Piano
}

public static class NoteUtils
{
    public static float GetFrequency(NoteName note)
    {
        switch (note)
        {
            case NoteName.Do:  return 261.63f; // C4
            case NoteName.Re:  return 293.66f; // D4
            case NoteName.Mi:  return 329.63f; // E4
            case NoteName.Fa:  return 349.23f; // F4
            case NoteName.Sol: return 392.00f; // G4
            default:           return 261.63f;
        }
    }

    public static string GetDisplayName(InstrumentType instrument)
    {
        switch (instrument)
        {
            case InstrumentType.Guitar: return "Guitarra";
            case InstrumentType.Piano:  return "Piano";
            default:                    return instrument.ToString();
        }
    }
}
