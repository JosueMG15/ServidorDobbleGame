using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    [DataContract]
    public class Amistad
    {
        [DataMember]
        public int idAmistad { get; set; }

        [DataMember]
        public bool estadoSolicitud { get; set; }

        [DataMember]
        public int UsuarioPrincipalId { get; set; }

        [DataMember]
        public int UsuarioAmigoId { get; set; }
    }
}
