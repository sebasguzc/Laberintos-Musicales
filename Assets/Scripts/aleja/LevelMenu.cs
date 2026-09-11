using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Juego2.Scripts
{
    public class LevelMenu : MonoBehaviour
    {
        [Header("Botones de Nivel")]
        public Button level1Button; // Botón para el Nivel 1 (12x12)
        public Button level2Button; // Botón para el Nivel 2 (18x18)

        [Header("Nombres de las Escenas en Build Settings")]
        public string level1SceneName = "Nivel1"; // Cambia al nombre exacto de tu escena del Nivel 1
        public string level2SceneName = "Nivel2"; // Cambia al nombre exacto de tu escena del Nivel 2

        void Start()
        {
            if (level1Button != null)
                level1Button.onClick.AddListener(SelectLevel1);

            if (level2Button != null)
                level2Button.onClick.AddListener(SelectLevel2);
        }

        void SelectLevel1()
        {
            SaveSelectedLevel(1);
            SceneManager.LoadScene(level1SceneName);
        }

        void SelectLevel2()
        {
            SaveSelectedLevel(2);
            SceneManager.LoadScene(level2SceneName);
        }

        private void SaveSelectedLevel(int levelIndex)
        {
            PlayerPrefs.SetInt("selectedLevel", levelIndex);
            PlayerPrefs.Save(); // Forzar el guardado inmediato en disco
        }
    }
}