using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Pausa el juego con un botón en pantalla o con la tecla Escape / P.
/// Congela el tiempo, pausa el audio y muestra el panel de pausa.
/// </summary>
public class MenuPausa : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Panel que aparece al pausar (con los botones Reanudar, Reiniciar, Menú...)")]
    public GameObject panelPausa;
    [Tooltip("Opcional: el botón de pausa de la esquina, se oculta mientras el juego está pausado")]
    public GameObject botonPausa;

    [Header("Audio")]
    [Tooltip("Si está activo, todo el audio del juego se detiene al pausar")]
    public bool pausarAudio = true;
    [Tooltip("Opcional: música que SÍ debe seguir sonando en el menú de pausa")]
    public AudioSource musicaDePausa;

    [Header("Escenas")]
    [Tooltip("Nombre exacto de la escena del menú principal (como aparece en Build Profiles)")]
    public string escenaMenuPrincipal = "LevelMenu";

    public static bool EstaPausado { get; private set; }

    void Start()
    {
        EstaPausado = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (panelPausa) panelPausa.SetActive(false);
        if (botonPausa) botonPausa.SetActive(true);
        if (musicaDePausa) musicaDePausa.ignoreListenerPause = true;
    }

    void Update()
    {
        if (TeclaPausaPresionada()) AlternarPausa();
    }

    bool TeclaPausaPresionada()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        if (k != null && (k.escapeKey.wasPressedThisFrame || k.pKey.wasPressedThisFrame)) return true;
        var g = Gamepad.current;
        if (g != null && g.startButton.wasPressedThisFrame) return true;
        return false;
#else
        return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
#endif
    }

    // ---------- Métodos para conectar a los botones (OnClick) ----------

    public void AlternarPausa()
    {
        if (EstaPausado) Reanudar();
        else Pausar();
    }

    public void Pausar()
    {
        // No pausar encima de la pantalla de Game Over
        if (Time.timeScale == 0f && !EstaPausado) return;

        EstaPausado = true;
        Time.timeScale = 0f;
        if (pausarAudio) AudioListener.pause = true;
        if (musicaDePausa) musicaDePausa.Play();

        if (panelPausa) panelPausa.SetActive(true);
        if (botonPausa) botonPausa.SetActive(false);
    }

    public void Reanudar()
    {
        EstaPausado = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (musicaDePausa) musicaDePausa.Stop();

        if (panelPausa) panelPausa.SetActive(false);
        if (botonPausa) botonPausa.SetActive(true);
    }

    public void Reiniciar()
    {
        Reanudar();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void IrAlMenuPrincipal()
    {
        Reanudar();
        SceneManager.LoadScene(escenaMenuPrincipal);
    }

    public void Salir()
    {
        Reanudar();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}