using UnityEngine;

namespace Juego2.Scripts
{
    public class CamaraSigueJugador : MonoBehaviour
    {
        [Header("Configuraci�n de Seguimiento")]
        public float velocidadSuavizado = 5f;
        public Vector3 desfase = new Vector3(0f, 0f, -10f); // Mantiene la c�mara por delante en el eje Z

        private Transform objetivoJugador;

        void LateUpdate()
        {
            // Si la c�mara a�n no ha encontrado al jugador, lo busca en la escena
            if (objetivoJugador == null)
            {
                BuscarJugador();
                return;
            }

            // Posici�n a la que debe moverse la c�mara
            Vector3 posicionDeseada = objetivoJugador.position + desfase;

            // Transici�n suave entre la posici�n actual de la c�mara y la del jugador
            Vector3 posicionSuave = Vector3.Lerp(transform.position, posicionDeseada, velocidadSuavizado * Time.deltaTime);

            transform.position = posicionSuave;
        }

        private void BuscarJugador()
        {
            // Busca en la escena cualquier objeto que tenga el script de movimiento
            JugadorMovimiento2D jugador = FindAnyObjectByType<JugadorMovimiento2D>();

            if (jugador != null)
            {
                objetivoJugador = jugador.transform;
            }
        }
    }
}