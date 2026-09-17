using UnityEngine;

namespace Juego2.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class JugadorMovimiento2D : MonoBehaviour
    {
        [Header("Configuración de Movimiento")]
        public float velocidadMovimiento = 5f;

        private Rigidbody2D rb;
        private Vector2 entradasMovimiento;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            // Captura las flechas del teclado o las teclas WASD
            float movX = Input.GetAxisRaw("Horizontal");
            float movY = Input.GetAxisRaw("Vertical");

            entradasMovimiento = new Vector2(movX, movY).normalized;
        }

        void FixedUpdate()
        {
            // Aplica el movimiento físico al Rigidbody2D
            rb.MovePosition(rb.position + entradasMovimiento * velocidadMovimiento * Time.fixedDeltaTime);
        }
    }
}