using DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Logica;
using System;
using Moq;
using System.Data.Entity;

namespace Pruebas
{
    [TestClass]
    public class PruebaRegistro
    {
        private DobbleBDEntidades contexto;

        [TestInitialize]
        public void Setup()
        {
            contexto = new DobbleBDEntidades();
        }

        [TestCleanup]
        public void Cleanup()
        {
            contexto.Dispose();
        }

        [TestMethod]
        public void ExisteCorreoAsociado_DevuelveVerdadero()
        {
            string correo = "correoprueba@hotmail.com";
            Assert.IsTrue(RegistroUsuario.ExisteCorreoAsociado(correo), "El método no devuelve un correo que ya esta asociado a una cuenta");
        }
        [TestMethod]
        public void ExisteCorreoAsociado_DevuelveFalso()
        {
            Assert.IsFalse(RegistroUsuario.ExisteCorreoAsociado("noexiste@hotmail.com"), "El método retorna true aunque el correo no existe");
        }
        [TestMethod]
        public void ExisteNombreUsuario_DevuelveVerdadero()
        {
            Assert.IsTrue(RegistroUsuario.ExisteNombreUsuario("Usuario7"), "El método no encuentra un nombre de usuario que ya esta registrado");
        }
        [TestMethod]
        public void ExisteNombreUsuario_DevuelveFalso()
        {
            Assert.IsFalse(RegistroUsuario.ExisteCorreoAsociado("Usuario100"), "El método retorna true aunque el nombre de usuario no existe");
        }
        [TestMethod]
        public void RegistrarUsuario_Exitoso()
        {
            CuentaUsuario nuevaCuentaUsuario = new CuentaUsuario()
            {
                Correo = "usuarionuevo@gmail.com",
                Usuario = "NuevoUsuario",
                Contraseña = "ContraseñaPrueba",
                Foto = null
            };
            Assert.IsTrue(RegistroUsuario.RegistrarUsuario(nuevaCuentaUsuario), "El método no registra un correo con datos válidos");
        }

        [TestMethod]
        public void RegistrarUsuario_CamposNulosDevuelveFalso()
        {
            CuentaUsuario nuevaCuentaUsuarioValoresNulos = new CuentaUsuario()
            {
                Correo = null,
                Usuario = null,
                Contraseña = null,
                Foto = null
            };
            Assert.IsFalse(RegistroUsuario.RegistrarUsuario(nuevaCuentaUsuarioValoresNulos), "El método registra un correo aunque tenga datos inválidos, como datos nulos");
        }

        [TestMethod]
        public void IniciarSesion_Exitoso()
        {
            string nombreUsuario = "osopanda";
            string contraseña = "Dobble1234";

            CuentaUsuario cuentaUsuarioExistente = RegistroUsuario.IniciarSesion(nombreUsuario, contraseña);
            Assert.IsNotNull(cuentaUsuarioExistente, "El método no retorna la cuenta del usuario aunque si existe");
        }

        [TestMethod]
        public void IniciarSesion_DatosIncorrectos()
        {
            string nombreUsuario = "usuarioInexistente";
            string contraseña = "Contraseña12";

            CuentaUsuario cuentaUsuarioInexistente = RegistroUsuario.IniciarSesion(nombreUsuario, contraseña);
            Assert.IsNull(cuentaUsuarioInexistente, "El método retorna una cuenta de usuario aunque esta no exista");
        }

        [TestMethod]
        public void RegistrarPuntosGanados_Exitoso()
        {
            string nombreUsuario = "osopanda";
            int puntosGanados = 10;

            Assert.IsTrue(RegistroUsuario.RegistrarPuntosGanados(nombreUsuario, puntosGanados), "El método no esta registrando los puntos correctamente");
        }

        [TestMethod]
        public void RegistrarPuntosGanados_NoExisteElUsuario()
        {
            string nombreUsuario = "usuarioInexistente";
            int puntosGanados = 10;

            Assert.IsFalse(RegistroUsuario.RegistrarPuntosGanados(nombreUsuario, puntosGanados), "El método esta registrando puntos a un usuario que no existe");
        }

        [TestMethod]
        public void ObtenerPuntosUsuario_Exitoso()
        {
            string nombreUsuario = "osopanda";
            int? puntos = RegistroUsuario.ObtenerPuntosUsuario(nombreUsuario);

            Assert.IsTrue(puntos >= 0, "El método devolvió un valor negativo, lo cual no es válido para los puntos del usuario.");
        }

        [TestMethod]
        public void ObtenerPuntosUsuario_UsuarioInexistente()
        {
            string nombreUsuario = "usuarioInexistente";
            int? puntos = RegistroUsuario.ObtenerPuntosUsuario(nombreUsuario);

            Assert.IsNull(puntos, "El método no devolvió null como se esperaba para un usuario inexistente.");
        }
    }
}
