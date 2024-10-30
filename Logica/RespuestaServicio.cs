using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    public class RespuestaServicio<T>
    {
        public T Resultado { get; set; }
        public bool Exitoso { get; set; }
        public bool ErrorBD { get; set; }
    }
}
