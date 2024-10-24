using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    [DataContract]
    public class Sala
    {
        public const int CantidadMaximaJugadores = 4;
        public const int CantidadMinimaJugadoresParaIniciarPartida = 2;
        [DataMember]
        public string CodigoSala {  get; set; }
        [DataMember]
        public string NombreAnfitrion {  get; set; }
        [DataMember]
        public int ContadorJugadores { get; set; }
    }
}
