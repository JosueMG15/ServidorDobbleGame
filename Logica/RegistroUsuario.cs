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
                return (from cuenta in contexto.Cuenta
                        where cuenta.nombreUsuario == nombreUsuario
                        select cuenta).Any();
            }
        }

        public static bool ExisteCorreoAsociado(string correoUsuario)
        {
            using (var contexto = new DobbleBDEntidades())
            {
                return (from cuenta in contexto.Cuenta
                        where cuenta.correo ==  correoUsuario
                        select cuenta).Any();
            }
        }

        //podría estar en otra clase
        public static CuentaUsuario IniciarSesion(string nombreUsuario, string contraseña)
        {
            CuentaUsuario cuentaUsuario = null;
            using (var contexto = new DobbleBDEntidades())
            {
                cuentaUsuario = (from cuenta in contexto.Cuenta
                                 join usuario in contexto.Usuario
                                 on cuenta.Usuario.idUsuario 
                                 equals usuario.idUsuario
                                 where cuenta.nombreUsuario == nombreUsuario
                                 && cuenta.contraseña == contraseña
                                 select new CuentaUsuario
                                 {
                                     IdCuentaUsuario = cuenta.idCuenta,
                                     Usuario = cuenta.nombreUsuario,
                                     Correo = cuenta.correo,
                                     Contraseña = cuenta.contraseña,
                                     Foto = usuario.foto,
                                     Puntaje = usuario.puntaje.Value,
                                 }).FirstOrDefault();
            }
            return cuentaUsuario;
        }
    }
}
