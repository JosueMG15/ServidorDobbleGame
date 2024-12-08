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
        string GenerarCodigoNuevaSala();
        [OperationContract]
        bool CrearNuevaSala(string nombreUsuario, string codigoSala);
        [OperationContract]
        bool UnirseASala(string nombreUsuario, string codigoSala, string mensaje, bool esAnfitrion);
        [OperationContract]
        bool AbandonarSala(string nombreUsuario, string codigoSala, string mensaje);
        [OperationContract(IsOneWay = true)]
        void ExpulsarJugador(string nombreUsuario, string codigoSala, string mensaje);

        [OperationContract(IsOneWay = true)]
        void EnviarMensajeSala(string nombreUsuario, string codigoSala, string mensaje);
        [OperationContract(IsOneWay = true)]
        void EnviarMensajeConexionSala(string nombreUsuario, string codigoSala, string mensaje);
        [OperationContract(IsOneWay = true)]
        void NotificarUsuarioConectado(string codigoSala);
        [OperationContract]
        bool HayEspacioSala(string codigoSala);
        [OperationContract]
        bool ExisteSala(string codigoSala);
        [OperationContract]
        bool EsSalaDisponible(string codigoSala);
        [OperationContract(IsOneWay = true)]
        void CambiarVentanaParaTodos(string codigoSala);
        [OperationContract(IsOneWay = true)]
        void NotificarJugadorListo(string nombreUsuario, string codigoSala);
        [OperationContract]
        bool TodosLosJugadoresEstanListos(string codigoSala);
    }

    [ServiceContract]
    interface ISalaCallback
    {
        [OperationContract(IsOneWay = true)]
        void MostrarMensajeSala(string mensaje);
        [OperationContract(IsOneWay = true)]
        void ActualizarUsuariosConectados(List<Jugador> usuariosConectados);
        [OperationContract(IsOneWay = true)]
        void ConvertirEnAnfitrion(string nombreUsuario);
        [OperationContract(IsOneWay = true)]
        void NotificarExpulsionAJugador();
        [OperationContract(IsOneWay = true)]
        void MostrarJugadorListo(string nombreUsuario, bool estaListo);
        [OperationContract(IsOneWay = true)]
        void CambiarVentana();
        [OperationContract]
        bool PingSala();
    }
}
