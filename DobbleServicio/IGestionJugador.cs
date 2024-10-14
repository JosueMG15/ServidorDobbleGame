using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DobbleServicio
{
    [ServiceContract]
    public interface IGestionJugador
    {
        [OperationContract]
        bool Registro();
    }
}
