using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    public static class GestorErrores
    {
        public static RespuestaServicio<T> Ejecutar<T>(Func<T> accion)
        {
            StackTrace stackTrace = new StackTrace();
            var metodo = stackTrace.GetFrame(1).GetMethod();
            string detallesLlamada = $"Método llamante: {metodo.DeclaringType?.FullName}.{metodo.Name}";
            Registro.Informacion(detallesLlamada);

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
            catch (EntityException ex)
            {
                Registro.Error($"Excepción de EntityException: {ex.Message}. " +
                    $"\nTraza: {ex.StackTrace}. \nFuente: {ex.Source}.");
            }
            catch (Exception ex)
            {
                Registro.Error($"Excepción no manejada: {ex.Message}. " +
                    $"\nTraza: {ex.StackTrace}. \nFuente: {ex.Source}.");
            }

            return respuesta;
        }

        public static T EjecutarConManejoDeExcepciones<T>(Func<T> funcion, T valorPorDefecto = default)
        {
            try
            {
                StackTrace stackTrace = new StackTrace();
                var metodo = stackTrace.GetFrame(1).GetMethod();
                string detallesLlamada = $"Método llamante: {metodo.DeclaringType?.FullName}.{metodo.Name}";
                Registro.Informacion(detallesLlamada);

                return funcion();
            }
            catch (CommunicationException ex)
            {
                Registro.Error($"Excepción de CommunicationException: {ex.Message}. " +
                    $"\nTraza: {ex.StackTrace}. \nFuente: {ex.Source}.");
            }
            catch (TimeoutException ex)
            {
                Registro.Error($"Excepción de TimeoutException: {ex.Message}. " +
                    $"\nTraza: {ex.StackTrace}. \nFuente: {ex.Source}.");
            }
            catch (Exception ex)
            {
                Registro.Error($"Excepción no manejada: {ex.Message}. " +
                    $"\nTraza: {ex.StackTrace}. \nFuente: {ex.Source}.");
            }

            return valorPorDefecto;
        }

        public static void EjecutarConManejoDeExcepciones(Action accion)
        {
            try
            {
                StackTrace stackTrace = new StackTrace();
                var metodo = stackTrace.GetFrame(1).GetMethod();
                string detallesLlamada = $"Método llamante: {metodo.DeclaringType?.FullName}.{metodo.Name}";
                Registro.Informacion(detallesLlamada);

                accion();
            }
            catch (CommunicationException ex)
            {
                Registro.Error($"Excepción de CommunicationException: {ex.Message}. " +
                    $"\nTraza: {ex.StackTrace}. \nFuente: {ex.Source}.");
            }
            catch (TimeoutException ex)
            {
                Registro.Error($"Excepción de TimeoutException: {ex.Message}. " +
                    $"\nTraza: {ex.StackTrace}. \nFuente: {ex.Source}.");
            }
            catch (Exception ex)
            {
                Registro.Error($"Excepción no manejada: {ex.Message}. " +
                    $"\nTraza: {ex.StackTrace}. \nFuente: {ex.Source}.");
            }
        }

    }
}
