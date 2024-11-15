using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    public class GeneradorCartas
    {
        private static Random random = new Random();
        private const int TAMAÑO = 7;
        private static int numSimbolosPorCarta = TAMAÑO + 1;
        private static int totalCartas = TAMAÑO * TAMAÑO + TAMAÑO + 1;

        private static List<Icono> InicializarIconos()
        {
            List<Icono> iconos = new List<Icono>()
            {
                new Icono(0, "pack://application:,,,/Iconos/Arbol.png"),
                new Icono(1, "pack://application:,,,/Iconos/Auto.png"),
                new Icono(2, "pack://application:,,,/Iconos/Balon.png"),
                new Icono(3, "pack://application:,,,/Iconos/Bebe.png"),
                new Icono(4, "pack://application:,,,/Iconos/Bicicleta.png"),
                new Icono(5, "pack://application:,,,/Iconos/Bola.png"),
                new Icono(6, "pack://application:,,,/Iconos/Caballo.png"),
                new Icono(7, "pack://application:,,,/Iconos/Camara.png"),
                new Icono(8, "pack://application:,,,/Iconos/Cangrejo.png"),
                new Icono(9, "pack://application:,,,/Iconos/Casa.png"),
                new Icono(10, "pack://application:,,,/Iconos/Chanclas.png"),
                new Icono(11, "pack://application:,,,/Iconos/Circo.png"),
                new Icono(12, "pack://application:,,,/Iconos/Cohete.png"),
                new Icono(13, "pack://application:,,,/Iconos/Consola.png"),
                new Icono(14, "pack://application:,,,/Iconos/Corazon.png"),
                new Icono(15, "pack://application:,,,/Iconos/Corona.png"),
                new Icono(16, "pack://application:,,,/Iconos/Crayolas.png"),
                new Icono(17, "pack://application:,,,/Iconos/Cubeta.png"),
                new Icono(18, "pack://application:,,,/Iconos/Dragon.png"),
                new Icono(19, "pack://application:,,,/Iconos/Enojado.png"),
                new Icono(20, "pack://application:,,,/Iconos/Feliz.png"),
                new Icono(21, "pack://application:,,,/Iconos/Flor.png"),
                new Icono(22, "pack://application:,,,/Iconos/Foco.png"),
                new Icono(23, "pack://application:,,,/Iconos/Gato.png"),
                new Icono(24, "pack://application:,,,/Iconos/Globos.png"),
                new Icono(25, "pack://application:,,,/Iconos/Hamster.png"),
                new Icono(26, "pack://application:,,,/Iconos/Lapiz.png"),
                new Icono(27, "pack://application:,,,/Iconos/Loro.png"),
                new Icono(28, "pack://application:,,,/Iconos/Luna.png"),
                new Icono(29, "pack://application:,,,/Iconos/Mano.png"),
                new Icono(30, "pack://application:,,,/Iconos/Manzana.png"),
                new Icono(31, "pack://application:,,,/Iconos/Mochila.png"),
                new Icono(32, "pack://application:,,,/Iconos/Montaña.png"),
                new Icono(33, "pack://application:,,,/Iconos/Moto.png"),
                new Icono(34, "pack://application:,,,/Iconos/Naranja.png"),
                new Icono(35, "pack://application:,,,/Iconos/Niña.png"),
                new Icono(36, "pack://application:,,,/Iconos/Pastel.png"),
                new Icono(37, "pack://application:,,,/Iconos/Pato.png"),
                new Icono(38, "pack://application:,,,/Iconos/Pera.png"),
                new Icono(39, "pack://application:,,,/Iconos/Perro.png"),
                new Icono(40, "pack://application:,,,/Iconos/Pie.png"),
                new Icono(41, "pack://application:,,,/Iconos/Pila.png"),
                new Icono(42, "pack://application:,,,/Iconos/Pino.png"),
                new Icono(43, "pack://application:,,,/Iconos/Pintura.png"),
                new Icono(44, "pack://application:,,,/Iconos/Rana.png"),
                new Icono(45, "pack://application:,,,/Iconos/Regalo.png"),
                new Icono(46, "pack://application:,,,/Iconos/Regla.png"),
                new Icono(47, "pack://application:,,,/Iconos/Reloj.png"),
                new Icono(48, "pack://application:,,,/Iconos/Reno.png"),
                new Icono(49, "pack://application:,,,/Iconos/Robot.png"),
                new Icono(50, "pack://application:,,,/Iconos/Rompecabezas.png"),
                new Icono(51, "pack://application:,,,/Iconos/Santa.png"),
                new Icono(52, "pack://application:,,,/Iconos/Sofa.png"),
                new Icono(53, "pack://application:,,,/Iconos/Sol.png"),
                new Icono(54, "pack://application:,,,/Iconos/Sombrilla.png"),
                new Icono(55, "pack://application:,,,/Iconos/Tren.png"),
                new Icono(56, "pack://application:,,,/Iconos/Triste.png"),
            };

            return iconos;
        }

        private static List<Carta> GenerarCartas()
        {
            List<Carta> cartas = new List<Carta>();
            List<Icono> iconos = InicializarIconos();

            for (int i = 0; i < numSimbolosPorCarta; i++)
            {
                List<Icono> iconosCarta = new List<Icono>
                {
                    iconos[0]
                };

                for (int j = 0; j < TAMAÑO; j++)
                {
                    iconosCarta.Add(iconos[j + 1 + i + TAMAÑO]);
                }
                cartas.Add(new Carta(iconosCarta));
            }

            for (int i = 0; i < TAMAÑO; i++)
            {
                for (int j = 0; j < TAMAÑO; j++)
                {
                    List<Icono> iconosCarta = new List<Icono>
                    {
                        iconos[i + 1]
                    };
                    
                    for (int k = 0; k < TAMAÑO; k++)
                    {
                        iconosCarta.Add(iconos[(TAMAÑO + 1 + TAMAÑO * k + (i * k + j) % TAMAÑO)]);
                    }
                    cartas.Add(new Carta(iconosCarta));
                }
            }

            return cartas;
        }

        public static List<Carta> ObtenerCartasRevueltas()
        {
            List<Carta> cartasRevueltas = GenerarCartas();
            int numCartas = cartasRevueltas.Count;

            for (int i = numCartas - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                var temporal = cartasRevueltas[i];
                cartasRevueltas[i] = cartasRevueltas[j];
                cartasRevueltas[j] = temporal;
            }

            return cartasRevueltas;
        }
    }
}
