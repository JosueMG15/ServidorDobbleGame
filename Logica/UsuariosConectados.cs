using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    public static class UsuariosConectados
    {
        private static readonly List<string> listaUsuarios = new List<string>();

        // Método para agregar un usuario a la lista de conectados
        public static void AgregarUsuario(string nombreUsuario)
        {
            if (!listaUsuarios.Contains(nombreUsuario))
            {
                listaUsuarios.Add(nombreUsuario);
            }
        }

        // Método para eliminar un usuario de la lista de conectados
        public static void QuitarUsuario(string nombreUsuario)
        {
            if (listaUsuarios.Contains(nombreUsuario))
            {
                listaUsuarios.Remove(nombreUsuario);
            }
        }

        // Método para obtener la lista de usuarios conectados
        public static bool ObtenerUsuarioConectado(string nombreUsuario)
        {
            if (listaUsuarios.Contains(nombreUsuario))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
