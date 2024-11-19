using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    [DataContract]
    public class Sala
    {
        public const int MAXIMO_JUGADORES = 4;
        public readonly object BloqueoSala = new object();
        public string CodigoSala { get; set; }
        public bool Disponible {  get; set; }
        public List<Jugador> Jugadores { get; set; }
        public Partida PartidaSala { get; set; }

        public Sala(string codigoSala)
        {
            Jugadores = new List<Jugador>();
            CodigoSala = codigoSala;
            Disponible = true;
        }

        public bool HayEspacioEnSala()
        {
            if (Jugadores.Count < MAXIMO_JUGADORES)
            {
                return true;
            }
            return false;
        }
    }
}
