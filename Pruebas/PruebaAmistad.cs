using DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Logica;
using System;
using Moq;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;

namespace Pruebas
{
    [TestClass]
    public class PruebaAmistad
    {
        private DobbleBDEntidades contexto;
        private int idUsuarioPrincipal = 1; 
        private string nombreUsuarioAmigo = "Killerresh"; 
        private int idUsuarioAmigo;

        [TestInitialize]
        public void Setup()
        {
            contexto = new DobbleBDEntidades();

            var usuarioAmigo = contexto.Cuenta.FirstOrDefault(c => c.nombreUsuario == nombreUsuarioAmigo);
            if (usuarioAmigo != null)
            {
                idUsuarioAmigo = usuarioAmigo.idCuenta;
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            contexto.Dispose();
        }

        [TestMethod]
        public void EnviarSolicitudAmistad_Exitosa()
        {
            // Se asegura quer no hay solicitudes previas
            var solicitudesPrevias = contexto.Amistad
                .Where(a => a.UsuarioPrincipalId == idUsuarioPrincipal && a.UsuarioAmigoId == idUsuarioAmigo)
                .ToList();
            foreach (var solicitud in solicitudesPrevias)
            {
                contexto.Amistad.Remove(solicitud);
            }
            contexto.SaveChanges();

            bool resultado = GestorAmistad.EnviarSolicitudAmistad(idUsuarioPrincipal, nombreUsuarioAmigo);

            Assert.IsTrue(resultado, "La solicitud de amistad no fue enviada correctamente.");

            // Limpieza después de la prueba
            var nuevaSolicitud = contexto.Amistad
                .FirstOrDefault(a => a.UsuarioPrincipalId == idUsuarioPrincipal && a.UsuarioAmigoId == idUsuarioAmigo);
            if (nuevaSolicitud != null)
            {
                contexto.Amistad.Remove(nuevaSolicitud);
                contexto.SaveChanges();
            }
        }

        [TestMethod]
        public void EnviarSolicitudAmistad_UsuarioAmigoInexistente()
        {
            string nombreUsuarioAmigoInexistente = "UsuarioInexistente"; 

            bool resultado = GestorAmistad.EnviarSolicitudAmistad(idUsuarioPrincipal, nombreUsuarioAmigoInexistente);

            Assert.IsFalse(resultado, "El método devolvió true cuando el usuario amigo no existe.");
        }


        [TestMethod]
        public void AmistadYaExiste_Exitoso()
        {
            // Crear una amistad existente entre los usuarios
            var amistadExistente = new DataAccess.Amistad
            {
                UsuarioPrincipalId = idUsuarioPrincipal,
                UsuarioAmigoId = idUsuarioAmigo,
                estadoSolicitud = true // Estado de amistad confirmada
            };
            contexto.Amistad.Add(amistadExistente);
            contexto.SaveChanges();

            bool resultado = GestorAmistad.AmistadYaExiste(idUsuarioPrincipal, nombreUsuarioAmigo);

            Assert.IsTrue(resultado, "El método no detectó una amistad existente.");

            // Limpiar después de la prueba
            contexto.Amistad.Remove(amistadExistente);
            contexto.SaveChanges();
        }

        [TestMethod]
        public void AmistadYaExiste_AmistadNoExistente()
        {
            // Asegurarse de que no haya amistad previa
            var amistadesPrevias = contexto.Amistad
                .Where(a => (a.UsuarioPrincipalId == idUsuarioPrincipal && a.UsuarioAmigoId == idUsuarioAmigo) ||
                            (a.UsuarioPrincipalId == idUsuarioAmigo && a.UsuarioAmigoId == idUsuarioPrincipal))
                .ToList();
            foreach (var amistad in amistadesPrevias)
            {
                contexto.Amistad.Remove(amistad);
            }
            contexto.SaveChanges();

            bool resultado = GestorAmistad.AmistadYaExiste(idUsuarioPrincipal, nombreUsuarioAmigo);

            Assert.IsFalse(resultado, "El método detectó una amistad inexistente.");
        }

        [TestMethod]
        public void AmistadYaExiste_UsuarioAmigoInexistente()
        {
            string nombreUsuarioInexistente = "UsuarioInexistente"; 

            bool resultado = GestorAmistad.AmistadYaExiste(idUsuarioPrincipal, nombreUsuarioInexistente);

            Assert.IsFalse(resultado, "El método devolvió true cuando el usuario amigo no existe.");
        }


        [TestMethod]
        public void ObtenerSolicitudesPendientes_Exitoso()
        {
            var nuevaSolicitud = new DataAccess.Amistad
            {
                UsuarioPrincipalId = 23,
                UsuarioAmigoId = 27,
                estadoSolicitud = false // Solicitud pendiente
            };
            contexto.Amistad.Add(nuevaSolicitud);
            contexto.SaveChanges();

            int usuarioAmigoId = 27;
            List<Logica.Amistad> solicitudes = GestorAmistad.ObtenerSolicitudesPendientes(usuarioAmigoId);

            Assert.IsTrue(solicitudes.Any(s => s.UsuarioPrincipalId == 23 && s.UsuarioAmigoId == usuarioAmigoId),
                          "El método no devolvió las solicitudes pendientes esperadas.");

            // Limpiar después de la prueba
            contexto.Amistad.Remove(nuevaSolicitud);
            contexto.SaveChanges();
        }

        [TestMethod]
        public void ObtenerSolicitudesPendientes_SinSolicitudesPendientes()
        {
            // Asegurarse de que no haya solicitudes pendientes para el usuario
            var solicitudesPendientes = contexto.Amistad
                .Where(a => a.UsuarioAmigoId == idUsuarioAmigo && a.estadoSolicitud == false)
                .ToList();
            foreach (var solicitud in solicitudesPendientes)
            {
                contexto.Amistad.Remove(solicitud);
            }
            contexto.SaveChanges();

            List<Logica.Amistad> solicitudes = GestorAmistad.ObtenerSolicitudesPendientes(idUsuarioAmigo);

            Assert.AreEqual(0, solicitudes.Count, "El método devolvió solicitudes pendientes inesperadas.");
        }

        [TestMethod]
        public void ObtenerSolicitudesPendientes_UsuarioAmigoInexistente()
        {
            int idUsuarioInexistente = 0; 

            List<Logica.Amistad> solicitudes = GestorAmistad.ObtenerSolicitudesPendientes(idUsuarioInexistente);

            Assert.AreEqual(0, solicitudes.Count, "El método devolvió solicitudes para un usuario inexistente.");
        }


        [TestMethod]
        public void ObtenerUsuario_Exitoso()
        {
            int idUsuarioExistente = 1;
            var usuarioOriginal = contexto.Cuenta.FirstOrDefault(c => c.idCuenta == idUsuarioExistente);
            Assert.IsNotNull(usuarioOriginal, "El usuario original no debería ser nulo para la prueba.");

            CuentaUsuario cuentaUsuario = GestorAmistad.ObtenerUsuario(idUsuarioExistente);

            Assert.IsNotNull(cuentaUsuario, "El método no devolvió un usuario.");
            Assert.AreEqual(usuarioOriginal.nombreUsuario, cuentaUsuario.Usuario, "El nombre de usuario no coincide.");
            Assert.AreEqual(usuarioOriginal.correo, cuentaUsuario.Correo, "El correo no coincide.");
        }

        [TestMethod]
        public void ObtenerUsuario_UsuarioNoExistente()
        {
            int idUsuarioInexistente = 0; 

            CuentaUsuario cuentaUsuario = GestorAmistad.ObtenerUsuario(idUsuarioInexistente);

            Assert.IsNull(cuentaUsuario, "El método debería devolver null para un usuario inexistente.");
        }

        [TestMethod]
        public void ObtenerUsuario_VerificarDatos()
        {
            int idUsuarioExistente = 1;
            var usuarioOriginal = contexto.Cuenta.FirstOrDefault(c => c.idCuenta == idUsuarioExistente);
            Assert.IsNotNull(usuarioOriginal, "El usuario original no debería ser nulo para la prueba.");

            // Guardar datos originales
            string nombreOriginal = usuarioOriginal.nombreUsuario;
            string correoOriginal = usuarioOriginal.correo;
            string contraseñaOriginal = usuarioOriginal.contraseña;

            CuentaUsuario cuentaUsuario = GestorAmistad.ObtenerUsuario(idUsuarioExistente);

            Assert.AreEqual(nombreOriginal, cuentaUsuario.Usuario, "El nombre de usuario fue alterado.");
            Assert.AreEqual(correoOriginal, cuentaUsuario.Correo, "El correo fue alterado.");
            Assert.AreEqual(contraseñaOriginal, cuentaUsuario.Contraseña, "La contraseña fue alterada.");
        }


        [TestMethod]
        public void ObtenerUsuarioPorNombre_Exitoso()
        {
            // Nombre de usuario que se sabe que existe en la base de datos
            string nombreUsuario = "paput";

            // Llamar al método
            var resultado = GestorAmistad.ObtenerUsuarioPorNombre(nombreUsuario);

            // Verificar que el resultado no es nulo y contiene datos
            Assert.IsNotNull(resultado, "El método debería devolver un usuario cuando el nombre existe.");
            Assert.AreEqual(nombreUsuario, resultado.Usuario, "El nombre de usuario en el resultado debería coincidir con el buscado.");
        }

        [TestMethod]
        public void ObtenerUsuarioPorNombre_UsuarioNoExiste()
        {
            // Nombre de usuario que se sabe que no existe en la base de datos
            string nombreUsuario = "q23tjhfbrfwfv";

            // Llamar al método
            var resultado = GestorAmistad.ObtenerUsuarioPorNombre(nombreUsuario);

            // Verificar que el resultado es nulo
            Assert.IsNull(resultado, "El método debería devolver null cuando el nombre de usuario no existe.");
        }

        [TestMethod]
        public void ObtenerUsuarioPorNombre_NombreUsuarioNulo()
        {
            // Llamar al método con un nombre de usuario nulo
            var resultadoNulo = GestorAmistad.ObtenerUsuarioPorNombre(null);

            // Verificar que el resultado es nulo
            Assert.IsNull(resultadoNulo, "El método debería devolver null cuando el nombre de usuario es nulo.");
        }


        [TestMethod]
        public void AceptarSolicitud_Exita()
        {
            int idAmistad = 3; // ID de una solicitud pendiente en la base de datos
            bool? estadoOriginal = false;

            using (var contexto = new DobbleBDEntidades())
            {
                var amistad = contexto.Amistad.FirstOrDefault(a => a.idAmistad == idAmistad);
                if (amistad != null)
                {
                    estadoOriginal = amistad.estadoSolicitud; // Guardar el estado original para revertir

                    // Llamar al método para aceptar la solicitud
                    bool resultado = GestorAmistad.AceptarSolicitud(idAmistad);

                    // Verificar que la solicitud fue aceptada correctamente
                    Assert.IsTrue(resultado, "La solicitud debería haberse aceptado correctamente.");
                    Assert.IsTrue(amistad.estadoSolicitud, "El estado de la solicitud debería ser 'aceptado'.");

                    // Revertir el cambio en la base de datos
                    amistad.estadoSolicitud = estadoOriginal;
                    contexto.SaveChanges();
                }
            }
        }

        [TestMethod]
        public void AceptarSolicitud_AmistadInexistente()
        {
            int idAmistad = 0; // ID de una solicitud que no existe

            // Llamar al método para aceptar la solicitud
            bool resultado = GestorAmistad.AceptarSolicitud(idAmistad);

            // Verificar que el resultado es false, ya que la solicitud no existe
            Assert.IsFalse(resultado, "El método debería retornar false si la solicitud no existe.");
        }

        [TestMethod]
        public void AceptarSolicitud_SolicitudYaAceptada()
        {
            int idAmistad = 1; // ID de una solicitud que ya está aceptada
            bool? estadoOriginal;

            using (var contexto = new DobbleBDEntidades())
            {
                var amistad = contexto.Amistad.FirstOrDefault(a => a.idAmistad == idAmistad);
                if (amistad != null && amistad.estadoSolicitud == true)
                {
                    estadoOriginal = amistad.estadoSolicitud; // Guardar el estado original

                    // Llamar al método para aceptar la solicitud
                    bool resultado = GestorAmistad.AceptarSolicitud(idAmistad);

                    // Verificar que el método retorna true, pero el estado no cambia
                    Assert.IsTrue(resultado, "El método debería retornar true aunque la solicitud ya esté aceptada.");
                    Assert.AreEqual(estadoOriginal, amistad.estadoSolicitud, "El estado de la solicitud no debería cambiar.");

                    // Revertir cualquier cambio en la base de datos
                    amistad.estadoSolicitud = estadoOriginal;
                    contexto.SaveChanges();
                }
            }
        }


        [TestMethod]
        public void EliminarAmistad_AmistadExiste_Exito()
        {
            int idAmistad = 1; // ID de una amistad existente
            DataAccess.Amistad amistadOriginal;

            using (var contexto = new DobbleBDEntidades())
            {
                // Guardar la amistad original para revertir
                amistadOriginal = contexto.Amistad.FirstOrDefault(a => a.idAmistad == idAmistad);
                if (amistadOriginal != null)
                {
                    var amistadCopia = new DataAccess.Amistad
                    {
                        idAmistad = amistadOriginal.idAmistad,
                        UsuarioPrincipalId = amistadOriginal.UsuarioPrincipalId,
                        UsuarioAmigoId = amistadOriginal.UsuarioAmigoId,
                        estadoSolicitud = amistadOriginal.estadoSolicitud
                    };

                    // Llamar al método para eliminar la amistad
                    bool resultado = GestorAmistad.EliminarAmistad(idAmistad);

                    // Verificar que la amistad fue eliminada
                    Assert.IsTrue(resultado, "La amistad debería haberse eliminado correctamente.");
                    Assert.IsNull(contexto.Amistad.FirstOrDefault(a => a.idAmistad == idAmistad), "La amistad debería estar eliminada de la base de datos.");

                    // Revertir el cambio en la base de datos
                    contexto.Amistad.Add(amistadCopia);
                    contexto.SaveChanges();
                }
            }
        }

        [TestMethod]
        public void EliminarAmistad_AmistadNoExiste()
        {
            int idAmistad = 0; // ID de una amistad inexistente

            // Llamar al método para eliminar la amistad
            bool resultado = GestorAmistad.EliminarAmistad(idAmistad);

            // Verificar que el resultado es false, ya que la amistad no existe
            Assert.IsFalse(resultado, "El método debería retornar false si la amistad no existe.");
        }









    }
}
