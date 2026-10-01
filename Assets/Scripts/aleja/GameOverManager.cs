using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject gameOverPanel;

    [Header("Nombres de Escenas")]
    public string mainMenuSceneName = "MainMenu"; // Nombre exacto de tu escena de menú

    void Start()
    {
        // Asegura que la pantalla arranque siempre oculta al iniciar el nivel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Asegura que el tiempo del juego esté corriendo con normalidad
        Time.timeScale = 1f;
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Pausa la física y el tiempo de juego
        Time.timeScale = 0f;
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f; // Restaura el tiempo antes de recargar
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Restaura el tiempo antes de cargar el menú
        SceneManager.LoadScene(mainMenuSceneName);
    }
}