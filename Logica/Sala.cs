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
        public const int MaximoJugadores = 4;
        public const int MinimoJugadoresParaIniciarPartida = 2;
        public readonly object BloqueoSala = new object();
        public List<CuentaUsuario> Usuarios { get; set; }
        public Partida partida { get; set; }

        public Sala()
        {
            Usuarios = new List<CuentaUsuario>();
        }
    }
}
