using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    [DataContract]
    public class Icono
    {
        [DataMember]
        public int IdIcono { get; set; }
        [DataMember]
        public string Ruta { get; set; }

        public Icono(int idIcono, string ruta)
        {
            IdIcono = idIcono;
            Ruta = ruta;
        }
    }
}
