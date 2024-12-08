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
        public Queue<Carta> Cartas { get; set; }
        public Carta CartaCentral { get; set; }
        public bool EstaEnCurso { get; set; } = true;

        public Partida()
        {
            JugadoresEnPartida = new List<Jugador>();
            Cartas = new Queue<Carta>(GeneradorCartas.ObtenerCartasRevueltas());
        }
    }
}
