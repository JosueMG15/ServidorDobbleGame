using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Modelo
{
    [DataContract]
    public class Sala
    {
        public const int MaximoJugadores = 4;
        public const int MinimoJugadoresParaIniciarPartida = 2;
        [DataMember]
        public List<CuentaUsuario> CuentasUsuarios;
        [DataMember]
        public string CodigoSala {  get; set; }
        [DataMember]
        public string NombreAnfitrion {  get; set; }

    }
}
