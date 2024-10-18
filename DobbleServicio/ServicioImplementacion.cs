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
                RegistroUsuario.RegistrarUsuario(cuentaUsuario);
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

        public bool ExisteNombreUsuario(string nombreUsuario)
        {
            bool existeNombre = true;
            try
            {
                RegistroUsuario.ExisteNombreUsuario(nombreUsuario);
                existeNombre = false;
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
