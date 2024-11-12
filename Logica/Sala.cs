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
        public const int MINIMO_JUGADORES_PARA_INICIAR_PARTIDA = 2;
        public readonly object BloqueoSala = new object();
        public string CodigoSala { get; set; }
        public List<CuentaUsuario> Usuarios { get; set; }
        public Partida PartidaSala { get; set; }

        public Sala(string codigoSala)
        {
            Usuarios = new List<CuentaUsuario>();
            CodigoSala = codigoSala;
        }

        public bool HayEspacioEnSala()
        {
            if (Usuarios.Count < MAXIMO_JUGADORES)
            {
                return true;
            }
            return false;
        }
    }
}
