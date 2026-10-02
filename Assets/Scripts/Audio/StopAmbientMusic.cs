using UnityEngine;

/// <summary>
/// Coloca este script en una escena (como el Nivel 1 o Nivel 2) donde quieras 
/// que la música ambiental persistente se detenga y desaparezca.
/// </summary>
public class StopAmbientMusic : MonoBehaviour
{
    void Start()
    {
        // Buscamos si existe el manager de música en la escena (incluso si sobrevivió de otra escena)
        AmbientMusicManager musicManager = FindObjectOfType<AmbientMusicManager>();

        if (musicManager != null)
        {
            Debug.Log("Deteniendo y destruyendo la música ambiental para este nivel.");
            Destroy(musicManager.gameObject);
        }
        else
        {
            Debug.Log("No se encontró música ambiental activa en este nivel.");
        }
    }
}