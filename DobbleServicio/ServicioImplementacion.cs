using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DobbleServicio
{
    public partial class ServicioImplementacion : IGestionJugador
    {
        public bool Registro()
        {
            Console.WriteLine("Hola");
            return true;
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
