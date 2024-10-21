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
        bool RegistrarUsuario(CuentaUsuario cuentaUsuario);
        [OperationContract]
        bool ExisteNombreUsuario(string nombreUsuario);
        [OperationContract]
        bool ExisteCorreoAsociado(string correoUsuario);
        [OperationContract]
        CuentaUsuario IniciarSesionJugador(string nombreUsuario, string contraseña);

        //[OperationContract]
        //bool ModificarNombreUsuario(String nombreUsuario);

    }
    
}
