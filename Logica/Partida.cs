using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    [DataContract]
    public class Partida
    {
        public readonly object BloqueoPartida = new object();
        [DataMember]
        public List<Jugador> JugadoresEnPartida {  get; set; }
        [DataMember]
        public List<Carta> Cartas { get; set; }
        public Carta CartaCentral { get; set; }

        public Partida()
        {
            JugadoresEnPartida = new List<Jugador>();
            Cartas = GeneradorCartas.ObtenerCartasRevueltas();
        }
    }
}
