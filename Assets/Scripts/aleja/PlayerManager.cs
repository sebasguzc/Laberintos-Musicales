using UnityEngine;

namespace Juego2.Scripts
{
    public class PlayerManager : MonoBehaviour
    {
        public GameObject player1Prefab; // Prefab de personaje1
        public GameObject player2Prefab; // Prefab de personaje2
        public Transform spawnPoint;     // Opcional: punto donde aparecerá

        void Start()
        {
            string character = PlayerPrefs.GetString("character", "personaje1"); // "personaje1" por defecto
            SpawnCharacter(character);
        }

        void SpawnCharacter(string character)
        {
            Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero;

            if (character == "personaje1")
            {
                Instantiate(player1Prefab, position, Quaternion.identity);
            }
            else if (character == "personaje2")
            {
                Instantiate(player2Prefab, position, Quaternion.identity);
            }
        }
    }
}