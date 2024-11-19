using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    public class GeneradorCartas
    {
        private readonly static int semilla = (int)DateTime.Now.Ticks;
        private readonly static Random random = new Random(semilla);
        private const int TAMAÑO = 2;
        private readonly static int numSimbolosPorCarta = TAMAÑO + 1;

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

        private static List<List<int>> AlgoritmoDobble()
        {
            List<List<int>> cartas = new List<List<int>>();
            

            for (int i = 0; i <= TAMAÑO; i++)
            {
                List<int> carta = new List<int> { 1 };

                for (int j = 1; j <= TAMAÑO; j++)
                {
                    int indice = TAMAÑO + TAMAÑO * (i - 1) + (j + 1);
                    carta.Add(indice);
                }
                cartas.Add(carta);
            }

            for (int i = 1; i <= TAMAÑO; i++)
            {
                for (int j = 1; j <= TAMAÑO; j++)
                {
                    List<int> carta = new List<int> { i + 1 };
                    for (int k = 1; k <= TAMAÑO; k++)
                    {
                        int indice = (numSimbolosPorCarta + 1) + TAMAÑO * (k - 1)
                            + (((i - 1) * (k - 1) + (j - 1))) % TAMAÑO;
                        carta.Add(indice);
                    }
                    cartas.Add(carta);
                }
            }

            return cartas;
        }

        private static void Revolver<T>(List<T> lista)
        {
            int numeroIconos = lista.Count;
            while (numeroIconos > 1)
            {
                numeroIconos--;
                int i = random.Next(numeroIconos + 1);
                T icono = lista[i];
                lista[i] = lista[numeroIconos];
                lista[numeroIconos] = icono;
            }
        }

        private static List<Carta> GenerarCartasDobble()
        {
            List<List<int>> cartasAlgoritmo = AlgoritmoDobble();
            List<Carta> cartasDobble = new List<Carta>();
            List<Icono> iconos = InicializarIconos();
            int numeroCarta = 1;

            foreach (var cartaAlgoritmo in cartasAlgoritmo)
            {
                Revolver(cartaAlgoritmo);
                List<Icono> cartaDobble = new List<Icono>();
                Console.WriteLine($"Carta {numeroCarta}:");
                foreach (var icono in cartaAlgoritmo)
                {
                    // Puedes imprimir la ruta del icono o un nombre representativo
                    if (icono <= iconos.Count)
                    {
                        Console.WriteLine($"- {iconos[icono - 1].Ruta}");
                        cartaDobble.Add(iconos[icono - 1]);
                    }
                }
                Console.WriteLine(); // Espacio entre cartas
                cartasDobble.Add(new Carta(cartaDobble));
                numeroCarta++;
            }

            return cartasDobble;
        }

        public static List<Carta> ObtenerCartasRevueltas()
        {
            List<Carta> cartasDobble = GenerarCartasDobble();
            Revolver(cartasDobble);

            return cartasDobble;
        }
    }
}
