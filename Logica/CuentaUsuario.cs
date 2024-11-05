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
    public class CuentaUsuario
    {

        [DataMember]
        public int IdCuentaUsuario { get; set; }
        [DataMember]
        public string Usuario { get; set; }
        [DataMember]
        public string Correo { get; set; }
        [DataMember]
        public string Contraseña { get; set; }
        [DataMember]
        public byte[] Foto { get; set; }
        [DataMember]
        public int Puntaje { get; set; }
        [DataMember]
        public bool Estado {  get; set; }
        [DataMember]
        public bool EsAnfitrion {  get; set; }
        [DataMember]
        public int PuntosEnPartida { get; set; }
        public OperationContext ContextoOperacion { get; set; }
        
    }
}
