using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    [DataContract]
    public class Jugador
    {
        [DataMember]
        public string Usuario { get; set; }
        [DataMember]
        public byte[] Foto { get; set; }
        [DataMember]
        public int Puntaje { get; set; }
        [DataMember]
        public bool EsAnfitrion {  get; set; }
        [DataMember]
        public int PuntosEnPartida { get; set; }
        [DataMember]
        public int NumeroJugador { get; set; }
        public bool CartaBloqueada { get; set; } = false;
        public OperationContext ContextoOperacion { get; set; }
    }
}
