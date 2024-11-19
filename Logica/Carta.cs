using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    [DataContract]
    public class Carta
    {
        [DataMember]
        public List<Icono> Iconos { get; private set; }
        
        public Carta() { }
        public Carta(List<Icono> iconos)
        {
            Iconos = iconos;
        }

    }
}
