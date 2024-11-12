using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    public class GestorCorreo
    {
        public static bool EnviarCorreo(string correo, string codigo)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("dobblegame11@gmail.com", "vmnm nqfu ercs sazf"),
                    EnableSsl = true,
                };
                smtpClient.Send("dobblegame11@gmail.com", correo, "Código de Verificación", $"Tu código es: {codigo}");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
