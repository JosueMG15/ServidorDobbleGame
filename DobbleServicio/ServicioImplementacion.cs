using DataAccess;
using Logica;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DobbleServicio
{
    public partial class ServicioImplementacion : IGestionJugador
    {
        public bool RegistrarUsuario(CuentaUsuario cuentaUsuario)
        {
            bool resultado = false;
            try
            {
                return RegistroUsuario.RegistrarUsuario(cuentaUsuario);
            }
            catch (EntityException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return resultado;
        }

        public bool ExisteNombreUsuario(string nombreUsuario)
        {
            bool existeNombre = true;
            try
            {
                existeNombre = RegistroUsuario.ExisteNombreUsuario(nombreUsuario); 
            }
            catch (EntityException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return existeNombre;
        }

        public bool ExisteCorreoAsociado(string correo)
        {
            bool existeCorreo = true;
            try
            {
                existeCorreo = RegistroUsuario.ExisteCorreoAsociado(correo);
            }
            catch (EntityException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return existeCorreo;
        }

        public CuentaUsuario IniciarSesionJugador(string nombreUsuario, string contraseña)
        {
            CuentaUsuario cuentaUsuario = null;

            try
            {
                cuentaUsuario = RegistroUsuario.IniciarSesion(nombreUsuario, contraseña);
            }
            catch (EntityException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return cuentaUsuario;
        }

        public bool ModificarNombreUsuario(int idUsuario, String nombreUsuario)
        {
            bool resultado = false;
            try
            {
                ModificarUsuario.ModificarNombreUsuario(idUsuario, nombreUsuario);
                resultado = true;
            }
            catch (EntityException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return resultado;
        }

        public bool ModificarContraseñaUsuario(int idUsuario, String contraseñaUsuario)
        {
            bool resultado = false;
            try
            {
                ModificarUsuario.ModificarContraseñaUsuario(idUsuario, contraseñaUsuario);
                resultado = true;
            }
            catch (EntityException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return resultado;
        }

        public bool ModificarFotoUsuario(int idCuenta, byte[] fotoUsuario)
        {
            bool resultado = false;
            try
            {
                ModificarUsuario.ModificarFotoUsuario(idCuenta, fotoUsuario);
                resultado = true;
            }
            catch (EntityException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return resultado;
        }

        public bool ValidarContraseña(int idUsuario, String contraseñaIngresada)
        {
            bool resultado = false;
            try
            {
                ModificarUsuario.ValidarContraseña(idUsuario, contraseñaIngresada);
                bool resultadoValidacion = ModificarUsuario.ValidarContraseña(idUsuario, contraseñaIngresada);
                return resultadoValidacion;
            }
            catch (EntityException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return resultado;
        }
    }


    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public partial class ServicioImplementacion : IGestionSala
    {
        public void EnviarMensajeSala(string mensaje)
        {
            try
            {
                OperationContext.Current.GetCallbackChannel<ISalaCallback>().SalaResponse(mensaje);
            } 
            catch(Exception ex) 
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
        }
    }
}
