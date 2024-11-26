using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    public class RegistroUsuario
    {
        public static bool RegistrarUsuario(CuentaUsuario cuentaUsuario)
        {
            bool resultado = false;

            try
            {
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
                        idCuenta = cuentaUsuario.IdCuentaUsuario,
                        foto = cuentaUsuario.Foto,
                        puntaje = cuentaUsuario.Puntaje,
                        estado = cuentaUsuario.Estado,
                    };

                    contexto.Cuenta.Add(nuevaCuentaUsuario);
                    contexto.Usuario.Add(usuario);
                    resultado = contexto.SaveChanges() > 0;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Console.WriteLine($"Propiedad: {validationError.PropertyName} - Error: {validationError.ErrorMessage}");
                    }
                }
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

        public static bool RegistrarPuntosGanados(string nombreUsuario, int puntosGanados)
        {
            using (var contexto = new DobbleBDEntidades())
            {
                var jugador = (from cuenta in contexto.Cuenta
                                join usuario in contexto.Usuario
                                on cuenta.idCuenta
                                equals usuario.idCuenta
                                where cuenta.nombreUsuario == nombreUsuario
                                select usuario).FirstOrDefault();

                if (jugador != null)
                {
                    jugador.puntaje = jugador.puntaje + puntosGanados;
                    contexto.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public static int? ObtenerPuntosUsuario(string nombreUsuario)
        {
            using (var contexto = new DobbleBDEntidades())
            {
                var puntaje = (from cuenta in contexto.Cuenta
                               join usuario in contexto.Usuario
                               on cuenta.idCuenta
                               equals usuario.idCuenta
                               where cuenta.nombreUsuario == nombreUsuario
                               select usuario.puntaje).FirstOrDefault();

                return puntaje;
            }
        }

        //Podría estar en otra clase
        public static CuentaUsuario IniciarSesion(string nombreUsuario, string contraseña)
        {
            CuentaUsuario cuentaUsuario = null;
            using (var contexto = new DobbleBDEntidades())
            {
                cuentaUsuario = (from cuenta in contexto.Cuenta
                                 join usuario in contexto.Usuario
                                 on cuenta.idCuenta
                                 equals usuario.idCuenta
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

        public static CuentaUsuario ObtenerUsuarioPorCorreo(string correo)
        {
            CuentaUsuario cuentaUsuario = null;
            using (var contexto = new DobbleBDEntidades())
            {
                cuentaUsuario = (from cuenta in contexto.Cuenta
                                 join usuario in contexto.Usuario
                                 on cuenta.idCuenta equals usuario.idCuenta
                                 where cuenta.correo == correo
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
