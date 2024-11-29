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
            var amistadExistente = new DataAccess.Amistad
            {
                UsuarioPrincipalId = idUsuarioPrincipal,
                UsuarioAmigoId = idUsuarioAmigo,
                estadoSolicitud = true 
            };
            contexto.Amistad.Add(amistadExistente);
            contexto.SaveChanges();

            bool resultado = GestorAmistad.AmistadYaExiste(idUsuarioPrincipal, nombreUsuarioAmigo);

            Assert.IsTrue(resultado, "El método no detectó una amistad existente.");

            contexto.Amistad.Remove(amistadExistente);
            contexto.SaveChanges();
        }

        [TestMethod]
        public void AmistadYaExiste_AmistadNoExistente()
        {
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
                estadoSolicitud = false 
            };
            contexto.Amistad.Add(nuevaSolicitud);
            contexto.SaveChanges();

            int usuarioAmigoId = 27;
            List<Logica.Amistad> solicitudes = GestorAmistad.ObtenerSolicitudesPendientes(usuarioAmigoId);

            Assert.IsTrue(solicitudes.Any(s => s.UsuarioPrincipalId == 23 && s.UsuarioAmigoId == usuarioAmigoId),
                          "El método no devolvió las solicitudes pendientes esperadas.");

            contexto.Amistad.Remove(nuevaSolicitud);
            contexto.SaveChanges();
        }

        [TestMethod]
        public void ObtenerSolicitudesPendientes_SinSolicitudesPendientes()
        {
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
            string nombreUsuario = "paput";

            var resultado = GestorAmistad.ObtenerUsuarioPorNombre(nombreUsuario);

            Assert.IsNotNull(resultado, "El método debería devolver un usuario cuando el nombre existe.");
            Assert.AreEqual(nombreUsuario, resultado.Usuario, "El nombre de usuario en el resultado debería coincidir con el buscado.");
        }

        [TestMethod]
        public void ObtenerUsuarioPorNombre_UsuarioNoExiste()
        {
            string nombreUsuario = "q23tjhfbrfwfv";

            var resultado = GestorAmistad.ObtenerUsuarioPorNombre(nombreUsuario);

            Assert.IsNull(resultado, "El método debería devolver null cuando el nombre de usuario no existe.");
        }

        [TestMethod]
        public void ObtenerUsuarioPorNombre_NombreUsuarioNulo()
        {
            var resultadoNulo = GestorAmistad.ObtenerUsuarioPorNombre(null);

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
            int idAmistad = 0; 

            bool resultado = GestorAmistad.AceptarSolicitud(idAmistad);

            Assert.IsFalse(resultado, "El método debería retornar false si la solicitud no existe.");
        }

        [TestMethod]
        public void AceptarSolicitud_SolicitudYaAceptada()
        {
            int idAmistad = 1; 
            bool? estadoOriginal;

            using (var contexto = new DobbleBDEntidades())
            {
                var amistad = contexto.Amistad.FirstOrDefault(a => a.idAmistad == idAmistad);
                if (amistad != null && amistad.estadoSolicitud == true)
                {
                    estadoOriginal = amistad.estadoSolicitud; 

                    bool resultado = GestorAmistad.AceptarSolicitud(idAmistad);

                    Assert.IsTrue(resultado, "El método debería retornar true aunque la solicitud ya esté aceptada.");
                    Assert.AreEqual(estadoOriginal, amistad.estadoSolicitud, "El estado de la solicitud no debería cambiar.");

                    amistad.estadoSolicitud = estadoOriginal;
                    contexto.SaveChanges();
                }
            }
        }


        [TestMethod]
        public void EliminarAmistad_AmistadExiste_Exito()
        {
            int idAmistad = 1; 
            DataAccess.Amistad amistadOriginal;

            using (var contexto = new DobbleBDEntidades())
            {
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

                    bool resultado = GestorAmistad.EliminarAmistad(idAmistad);

                    Assert.IsTrue(resultado, "La amistad debería haberse eliminado correctamente.");
                    Assert.IsNull(contexto.Amistad.FirstOrDefault(a => a.idAmistad == idAmistad), "La amistad debería estar eliminada de la base de datos.");

                    contexto.Amistad.Add(amistadCopia);
                    contexto.SaveChanges();
                }
            }
        }

        [TestMethod]
        public void EliminarAmistad_AmistadNoExiste()
        {
            int idAmistad = 0; 

            bool resultado = GestorAmistad.EliminarAmistad(idAmistad);

            Assert.IsFalse(resultado, "El método debería retornar false si la amistad no existe.");
        }

        [TestMethod]
        public void EliminarAmistad_IdInvalido()
        {
            int idAmistad = -1; 

            bool resultado = GestorAmistad.EliminarAmistad(idAmistad);

            Assert.IsFalse(resultado, "El método debería retornar false si el ID de amistad es inválido.");
        }


        [TestMethod]
        public void ObtenerAmistades_Exitoso()
        {
            int idUsuario = 29; 

            var amistades = GestorAmistad.ObtenerAmistades(idUsuario);

            Assert.IsNotNull(amistades, "La lista de amistades no debería ser nula.");
            Assert.IsTrue(amistades.Count > 1, "El usuario debería tener más de una amistad aceptada.");
            Assert.IsTrue(amistades.All(a => a.estadoSolicitud == true), "Todas las amistades devueltas deberían estar aceptadas.");
        }

        [TestMethod]
        public void ObtenerAmistades_ListaVacia()
        {
            int idUsuario = 2; 

            var amistades = GestorAmistad.ObtenerAmistades(idUsuario);

            Assert.IsNotNull(amistades, "La lista de amistades no debería ser nula.");
            Assert.AreEqual(0, amistades.Count, "La lista de amistades debería estar vacía para un usuario sin amistades aceptadas.");
        }

        [TestMethod]
        public void ObtenerAmistades_DevuelveListaVacia()
        {
            int idUsuario = -1; 

            var amistades = GestorAmistad.ObtenerAmistades(idUsuario);

            Assert.IsNotNull(amistades, "La lista de amistades no debería ser nula.");
            Assert.AreEqual(0, amistades.Count, "La lista de amistades debería estar vacía para un usuario inexistente.");
        }


        [TestMethod]
        public void ObtenerAmistad_Exitoso()
        {
            int idAmistad = 27; 

            var amistad = GestorAmistad.ObtenerAmistad(idAmistad);

            Assert.IsNotNull(amistad, "La amistad no debería ser nula para un ID válido.");
            Assert.AreEqual(idAmistad, amistad.idAmistad, "El ID de la amistad devuelta debería coincidir con el solicitado.");
        }

        [TestMethod]
        public void ObtenerAmistad_AmistadNula()
        {
            int idAmistad = -1; 

            var amistad = GestorAmistad.ObtenerAmistad(idAmistad);

            Assert.IsNull(amistad, "El método debería devolver null para un ID inexistente.");
        }

        [TestMethod]
        public void ObtenerAmistad_DatosMapeadosCorrectos()
        {
            int idAmistad = 27;

            var amistad = GestorAmistad.ObtenerAmistad(idAmistad);

            Assert.IsNotNull(amistad, "La amistad no debería ser nula para un ID válido.");
            Assert.AreEqual(idAmistad, amistad.idAmistad, "El ID de la amistad debería coincidir.");
            Assert.IsTrue(amistad.estadoSolicitud, "El estado de la solicitud debería ser verdadero.");
            Assert.IsTrue(amistad.UsuarioPrincipalId > 0, "El ID del usuario principal debería ser válido.");
            Assert.IsTrue(amistad.UsuarioAmigoId > 0, "El ID del usuario amigo debería ser válido.");
        }


        [TestMethod]
        public void ObtenerSolicitud_Exioso()
        {
            var ultimaAmistad = GestorAmistad.ObtenerSolicitud();

            Assert.IsNotNull(ultimaAmistad, "El método debería devolver el último registro existente.");

            Assert.IsTrue(ultimaAmistad.idAmistad > 0, "El ID de la amistad debería ser válido.");
        }

        [TestMethod]
        public void ObtenerSolicitud_ValidaUltimoRegistro()
        {
            using (var contexto = new DobbleBDEntidades())
            {
                var ultimoRegistroEsperado = contexto.Amistad.OrderByDescending(a => a.idAmistad).FirstOrDefault();

                var ultimaAmistad = GestorAmistad.ObtenerSolicitud();

                Assert.IsNotNull(ultimaAmistad, "El método debería devolver el último registro existente.");
                Assert.AreEqual(ultimoRegistroEsperado.idAmistad, ultimaAmistad.idAmistad, "El ID del registro devuelto debería coincidir con el último registro de la tabla.");
            }
        }
    }
}
