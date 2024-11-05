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

    }
}
