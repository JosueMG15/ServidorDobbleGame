using Logica;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DobbleServicio
{
    // NOTA: puede usar el comando "Cambiar nombre" del menú "Refactorizar" para cambiar el nombre de interfaz "IIGestionSala" en el código y en el archivo de configuración a la vez.
    [ServiceContract(CallbackContract = typeof(ISalaCallback))]
    public interface IGestionSala
    {
        [OperationContract]
        bool CrearNuevaSala(string nombreUsuario, string codigoSala);
        [OperationContract]
        bool UnirseASala(string nombreUsuario, string codigoSala, string mensaje, bool esAnfitrion);
        [OperationContract]
        bool AbandonarSala(string nombreUsuario, string codigoSala, string mensaje);

        [OperationContract(IsOneWay = true)]
        void EnviarMensajeSala(string nombreUsuario, string codigoSala, string mensaje);
        [OperationContract(IsOneWay = true)]
        void EnviarMensajeConexionSala(string nombreUsuario, string codigoSala, string mensaje);
        [OperationContract]
        string GenerarCodigoNuevaSala();
        [OperationContract(IsOneWay = true)]
        void NotificarUsuarioConectado(string codigoSala);

    }

    [ServiceContract]
    interface ISalaCallback
    {
        [OperationContract(IsOneWay = true)]
        void MostrarMensajeSala(string mensaje);
        [OperationContract(IsOneWay = true)]
        void ActualizarUsuariosConectados(List<CuentaUsuario> usuariosConectados);
        [OperationContract(IsOneWay = true)]
        void CambiarVentanaAPartida();

    }
}
