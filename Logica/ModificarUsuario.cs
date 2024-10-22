using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
