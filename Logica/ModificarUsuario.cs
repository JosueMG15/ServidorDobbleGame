using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    public class ModificarUsuario
    {
        public static bool ModificarNombreUsuario(int idCuenta,  String nombreUsuario)
        {
            bool resultado = false;
            using (var contexto = new DobbleBDEntidades())
            {
                var cuentaUsuario = contexto.Cuenta.FirstOrDefault(c => c.idCuenta == idCuenta);

                if (cuentaUsuario != null)
                {
                    cuentaUsuario.nombreUsuario = nombreUsuario;
                    resultado = contexto.SaveChanges() > 0;
                }
                return resultado;
            }
        }

        public static bool ModificarContraseñaUsuario(int idCuenta, String contraseñaUsuario)
        {
            bool resultado = false;
            using (var contexto = new DobbleBDEntidades())
            {
                var cuentaUsuario = contexto.Cuenta.FirstOrDefault(c => c.idCuenta == idCuenta);

                if (cuentaUsuario != null)
                {
                    cuentaUsuario.contraseña = contraseñaUsuario;
                    resultado = contexto.SaveChanges() > 0;
                }
                return resultado;
            }
        }

        public static bool ModificarFotoUsuario(int idCuenta, byte[] fotoUsuario)
        {
            bool resultado = false;
            using (var contexto = new DobbleBDEntidades())
            {
                var usuario = contexto.Usuario.FirstOrDefault(c => c.idCuenta == idCuenta);

                if (usuario != null)
                {
                    usuario.foto = fotoUsuario;
                    resultado = contexto.SaveChanges() > 0;
                }
                return resultado;
            }
        }

        public static bool ValidarContraseña(int idCuenta, String contraseñaIngresada)
        {
            bool resultado = false;
            using (var contexto = new DobbleBDEntidades())
            {
                var cuentaUsuario = contexto.Cuenta.FirstOrDefault(c => c.idCuenta == idCuenta);

                if (cuentaUsuario != null)
                {
                    string contraseñaAlmacenada = cuentaUsuario.contraseña;

                    resultado = contraseñaIngresada == contraseñaAlmacenada;
                    
                }
            }
            return resultado;
        }
    }
}
