using UnityEngine;

namespace Juego2.Scripts
{
    public class CamaraSigueJugador : MonoBehaviour
    {
        [Header("Configuración de Seguimiento")]
        public float velocidadSuavizado = 5f;
        public Vector3 desfase = new Vector3(0f, 0f, -10f); // Mantiene la cámara por delante en el eje Z

        private Transform objetivoJugador;

        void LateUpdate()
        {
            // Si la cámara aún no ha encontrado al jugador, lo busca en la escena
            if (objetivoJugador == null)
            {
                BuscarJugador();
                return;
            }

            // Posición a la que debe moverse la cámara
            Vector3 posicionDeseada = objetivoJugador.position + desfase;

            // Transición suave entre la posición actual de la cámara y la del jugador
            Vector3 posicionSuave = Vector3.Lerp(transform.position, posicionDeseada, velocidadSuavizado * Time.deltaTime);

            transform.position = posicionSuave;
        }

        private void BuscarJugador()
        {
            // Busca en la escena cualquier objeto que tenga el script de movimiento
            JugadorMovimiento2D jugador = FindObjectOfType<JugadorMovimiento2D>();

            if (jugador != null)
            {
                objetivoJugador = jugador.transform;
            }
        }
    }
}