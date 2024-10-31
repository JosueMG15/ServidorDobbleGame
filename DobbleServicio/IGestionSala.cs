using Logica;
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
        [OperationContract]
        bool CrearNuevaSala(string nombreAnfitrion, string codigoSala);
        [OperationContract]
        bool UnirseASala(string nombreUsuario, int puntaje, byte[] foto, string codigoSala, string mensaje);
        [OperationContract]
        bool AbandonarSala(string nombreUsuario, string codigoSala, string mensaje);

        [OperationContract(IsOneWay = true)]
        void EnviarMensajeSala(string nombreUsuario, string codigoSala, string mensaje);
        [OperationContract(IsOneWay = true)]
        void EnviarMensajeConexionSala(string nombreUsuario, string codigoSala, string mensaje);
        [OperationContract]
        string GenerarCodigoNuevaSala();
 
    }

    [ServiceContract]
    interface ISalaCallback
    {
        [OperationContract(IsOneWay = true)]
        void MostrarMensajeSala(string mensaje);
    }
}
