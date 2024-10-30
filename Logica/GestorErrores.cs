using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    public static class GestorErrores
    {
        public static RespuestaServicio<T> Ejecutar<T>(Func<T> accion)
        {
            var respuesta = new RespuestaServicio<T>
            {
                Resultado = default,
                Exitoso = false,
                ErrorBD = true
            };

            try
            {
                respuesta.Resultado = accion();
                respuesta.Exitoso = true;
                respuesta.ErrorBD = false;
            }
            catch (EntityException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return respuesta;
        }
    }
}
