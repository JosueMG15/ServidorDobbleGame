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
    public static class GestorCorreo
    {
        public static bool EnviarCorreo(string correo, string codigo)
        {
            try
            {
                string usuarioSmtp = Environment.GetEnvironmentVariable("USUARIO_SMTP");
                string contraseñaSmtp = Environment.GetEnvironmentVariable("CONTRASEÑA_SMTP");
                
                if (string.IsNullOrEmpty(usuarioSmtp) || string.IsNullOrEmpty(contraseñaSmtp))
                {
                    Registro.Error("Las credenciales SMTP no están configuradas correctamente.");
                    return false;
                }

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(usuarioSmtp, contraseñaSmtp),
                    EnableSsl = true,
                };
                smtpClient.Send(usuarioSmtp, correo, "Código de Verificación", $"Tu código es: {codigo}");
                return true;
            }
            catch (SmtpException ex)
            {
                Registro.Error($"Error SMTP: {ex.Message}.\nTraza: {ex.StackTrace}");
                return false;
            }
            catch (Exception ex)
            {
                Registro.Error($"Error general: {ex.Message}.\nTraza: {ex.StackTrace}");
                return false;
            }
        }
    }
}
