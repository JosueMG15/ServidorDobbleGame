using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DobbleServicio
{
    // NOTA: puede usar el comando "Cambiar nombre" del menú "Refactorizar" para cambiar el nombre de interfaz "IIGestionSala" en el código y en el archivo de configuración a la vez.
    [ServiceContract(CallbackContract = typeof(ISalaCallback))]
    public interface IGestionSala
    {
        [OperationContract(IsOneWay = true)]
        void EnviarMensajeSala(String mensaje);
    }

    [ServiceContract]
    interface ISalaCallback
    {
        [OperationContract(IsOneWay = true)]
        void SalaResponse(String respuesta);
    }
}
