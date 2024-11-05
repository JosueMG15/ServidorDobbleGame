using Logica;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

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
        RespuestaServicio<bool> AceptarSolicitud(int idAmistad);

        [OperationContract]
        RespuestaServicio<bool> EliminarAmistad(int idAmistad);

        [OperationContract]
        RespuestaServicio<List<Amistad>> ObtenerAmistades(int idUsuario);

        [OperationContract]
        RespuestaServicio<Amistad> ObtenerAmistad(int idAmistad);

    }
}
