using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Registro
{
    public static class Registro
    {
        private static readonly ILog registro = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static Registro()
        {
            XmlConfigurator.Configure();
        }

        public static void Informacion(string mensaje)
        {
            registro.Info(mensaje);
        }

        public static void Depuracion(string mensaje)
        {
            registro.Debug(mensaje);
        }

        public static void Advertencia(string mensaje)
        {
            registro.Warn(mensaje);
        }

        public static void Error(string mensaje, Exception ex = null)
        {
            registro.Error(mensaje, ex);
        }

        public static void Fatal(string mensaje, Exception ex = null)
        {
            registro.Fatal(mensaje, ex);
        }
    }
}
