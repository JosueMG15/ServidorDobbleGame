using Logica;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DobbleServicio
{
    [ServiceContract(CallbackContract = typeof(IPartidaCallback))]
    public interface IGestionPartida
    {
        [OperationContract]
        bool CrearNuevaPartida(string codigoSala);
        [OperationContract(IsOneWay = true)]
        void UnirJugadoresAPartida(string codigoSala);
        [OperationContract]
        bool AbandonarPartida(string nombreUsuario, string codigoSala);
        [OperationContract(IsOneWay = true)]
        void NotificarActualizacionDeJugadoresEnPartida(string codigoSala);

    }

    [ServiceContract]
    interface IPartidaCallback
    {
        [OperationContract(IsOneWay = true)]
        void ActualizarJugadoresEnPartida(List<CuentaUsuario> jugadoresConectados);
    }
}
