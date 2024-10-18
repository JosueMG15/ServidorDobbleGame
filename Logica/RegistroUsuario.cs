using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    public class RegistroUsuario
    {
        public static bool RegistrarUsuario(CuentaUsuario cuentaUsuario)
        {
            bool resultado = false;
            using (var contexto = new DobbleBDEntidades())
            {
                Cuenta nuevaCuentaUsuario = new Cuenta()
                {
                    idCuenta = cuentaUsuario.IdCuentaUsuario,
                    nombreUsuario = cuentaUsuario.Usuario,
                    correo = cuentaUsuario.Correo,
                    contraseña = cuentaUsuario.Contraseña,
                };

                Usuario usuario = new Usuario()
                {
                    idUsuario = cuentaUsuario.IdCuentaUsuario,
                    foto = cuentaUsuario.Foto,
                    puntaje = cuentaUsuario.Puntaje,
                };

                contexto.Cuenta.Add(nuevaCuentaUsuario);
                contexto.Usuario.Add(usuario);
                resultado = contexto.SaveChanges() > 0;
            }
            return resultado;
        }

        public static bool ExisteNombreUsuario(string nombreUsuario)
        {
            using (var contexto = new DobbleBDEntidades())
            {
                return (from cuentaUsuario in contexto.Cuenta
                        where cuentaUsuario.nombreUsuario == nombreUsuario
                        select cuentaUsuario).Any();
            }
        }
    }
}
