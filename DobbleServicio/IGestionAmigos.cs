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
    public interface IGestionAmigos
    {
        [OperationContract]
        RespuestaServicio<bool> EnviarSolicitudAmistad(int idUsuarioPrincipal, String nombreUsuarioAmigo);

        [OperationContract]
        RespuestaServicio<bool> AmistadYaExiste(int idUsuarioPrincipal, String nombreUsuarioAmigo);

        [OperationContract]
        RespuestaServicio<List<Amistad>> ObtenerSolicitudesPendientes(int idUsuarioAmigo);

        [OperationContract]
        RespuestaServicio<CuentaUsuario> ObtenerUsuario(int idUsuario);

        [OperationContract]
        RespuestaServicio<bool> AceptarSolicitud(int idAmistad, String nombreUsuarioAmigo);

        [OperationContract]
        RespuestaServicio<bool> EliminarAmistad(int idAmistad, String nombreUsuario, String nombreUsuarioAmigo);

        [OperationContract]
        RespuestaServicio<List<Amistad>> ObtenerAmistades(int idUsuario);

        [OperationContract]
        RespuestaServicio<Amistad> ObtenerAmistad(int idAmistad);

        [OperationContract]
        RespuestaServicio<Amistad> ObtenerSolicitud();

        [OperationContract]
        RespuestaServicio<bool> UsuarioConectado(string nombreUsuario);
        [OperationContract]
        bool TieneInvitacionPendiente(string nombreUsuario);
        [OperationContract]
        void ReestablecerInvitacionPendiente(string nombreUsuario);

        [OperationContract]
        void NotificarCambios(string nombreUsuario);

        [OperationContract]
        void NotificarDesconexion(string nombreUsuario);

        [OperationContract]
        void NotificarBotonInvitacion(string nombreUsuario);

        [OperationContract(IsOneWay = true)]
        void NotificarInvitacion(string nombreUsuario, string nombreUsuarioInvitacion, string codigoSala);
    }
}