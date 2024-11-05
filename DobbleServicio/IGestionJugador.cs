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
    public interface IGestionJugador
    {
        [OperationContract]
        RespuestaServicio<bool> RegistrarUsuario(CuentaUsuario cuentaUsuario);

        [OperationContract]
        RespuestaServicio<bool> ExisteNombreUsuario(string nombreUsuario);

        [OperationContract]
        RespuestaServicio<bool> ExisteCorreoAsociado(string correoUsuario);

        [OperationContract]
        RespuestaServicio<CuentaUsuario> IniciarSesionJugador(string nombreUsuario, string contraseña);
        [OperationContract]
        void CerrarSesionJugador(string nombreUsuario);

        [OperationContract]
        RespuestaServicio<bool> ModificarNombreUsuario(int idCuenta, String nombreUsuario);

        [OperationContract]
        RespuestaServicio<bool> ModificarContraseñaUsuario(int idCuenta, String contraseñaUsuario);

        [OperationContract]
        RespuestaServicio<bool> ModificarFotoUsuario(int idCuenta, byte[] fotoUsuario);

        [OperationContract]
        RespuestaServicio<bool> ValidarContraseña(int idCuenta, String contraseñaUsuario);
    }   
}
