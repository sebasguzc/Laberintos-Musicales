using UnityEngine;
using UnityEngine.SceneManagement; // Obligatorio para gestionar escenas

public class SceneChanger : MonoBehaviour
{
    // Método público para cargar la siguiente escena en el Build Index
    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    // Método alternativo para cargar la escena escribiendo su nombre exacto
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}