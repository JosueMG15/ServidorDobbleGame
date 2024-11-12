using Logica;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using DobbleServicio;

namespace DobbleServicio
{
    [ServiceContract]
    public interface IGestionCorreos
    {
        [OperationContract]
        RespuestaServicio<bool> EnviarCodigo(string correo);
    }
}
