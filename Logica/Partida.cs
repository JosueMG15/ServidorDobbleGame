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
        public List<CuentaUsuario> CuentasEnPartida {  get; set; }
    }
}
