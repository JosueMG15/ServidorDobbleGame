using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DobbleServicio
{
    [ServiceContract(CallbackContract = typeof(IGestionAmigosCallback))]
    public interface IGestionNotificacionesAmigos
    {
        [OperationContract]
        void ConectarCliente(string nombreUsuario);

        [OperationContract]
        void DesconectarCliente(string nombreUsuario);
    }

    [ServiceContract]
    public interface IGestionAmigosCallback
    {
        [OperationContract(IsOneWay = true)]
        void NotificarSolicitudAmistad();

        [OperationContract(IsOneWay = true)]
        void NotificarCambio();
    }
}
