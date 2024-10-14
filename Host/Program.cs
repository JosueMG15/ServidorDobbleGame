using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Host
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(DobbleServicio.ServicioImplementacion)))
            {
                host.Open();
                Console.WriteLine("El servidor esta corriendo");
                Console.ReadLine();
            }
        }
    }
}
