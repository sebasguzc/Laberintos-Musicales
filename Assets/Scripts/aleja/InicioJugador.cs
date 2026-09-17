using UnityEngine;

namespace Juego2.Scripts
{
    public class InicioJugador : MonoBehaviour
    {
        [Header("Prefabs de Personajes")]
        public GameObject personaje1Prefab; // Arrastra el prefab de personaje1 (el chico)
        public GameObject personaje2Prefab; // Arrastra el prefab de personaje2 (la chica)

        private void Start()
        {
            // Leemos el nombre del personaje guardado previamente en CharacterMenu.cs
            string personajeElegido = PlayerPrefs.GetString("character", "personaje1");
            Debug.Log("Personaje seleccionado cargado: " + personajeElegido);

            InstanciarPersonaje(personajeElegido);
        }

        private void InstanciarPersonaje(string nombrePersonaje)
        {
            GameObject prefabAInstanciar = null;

            if (nombrePersonaje == "personaje1")
            {
                prefabAInstanciar = personaje1Prefab;
            }
            else if (nombrePersonaje == "personaje2")
            {
                prefabAInstanciar = personaje2Prefab;
            }

            if (prefabAInstanciar != null)
            {
                Debug.Log("Instanciando personaje: " + prefabAInstanciar.name);
                // Instancia el personaje exactamente en la posición donde se ubique este objeto (SpawnPoint)
                Instantiate(prefabAInstanciar, transform.position, Quaternion.identity);
            }
            else
            {
                Debug.LogError("El prefab del personaje seleccionado no está asignado en el Inspector.");
            }
        }
    }
}