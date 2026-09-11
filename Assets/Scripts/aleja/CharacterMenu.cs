using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Juego2.Scripts
{
    public class CharacterMenu : MonoBehaviour
    {
        public Button player1Button; // personaje1 (el chico de las maracas)
        public Button player2Button; // personaje2 (la chica del teclado)

        void Start()
        {
            player1Button.onClick.AddListener(ChoosePlayer1);
            player2Button.onClick.AddListener(ChoosePlayer2);
        }

        void ChoosePlayer1()
        {
            SavePlayerAs("personaje1");
            LoadNextLevel();
        }

        void ChoosePlayer2()
        {
            SavePlayerAs("personaje2");
            LoadNextLevel();
        }

        private void SavePlayerAs(string character)
        {
            PlayerPrefs.SetString("character", character);
            PlayerPrefs.Save(); // Asegura que se guarde de inmediato
        }

        private void LoadNextLevel()
        {
            // Cambia "NombreDeTuEscenaMapa" por el nombre exacto de tu escena de Selección de Mapa
            SceneManager.LoadScene("Nivel");
        }
    }
}