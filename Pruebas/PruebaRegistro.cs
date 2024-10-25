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
        [TestMethod]
        public void RegistrarUsuarioPruebaExitoso()
        {
            // Arrange
            var cuentaUsuario = new CuentaUsuario
            {
                Usuario = "UsuarioPrueba",
                Correo = "correo@hotmail.com",
                Contraseña = "Dobble1234",
                Foto = null,
                Puntaje = 0,
                Estado = true
            };

            // Act
            var resultado = RegistroUsuario.RegistrarUsuario(cuentaUsuario); 

            // Assert
            Assert.IsTrue(resultado);
        }

        [TestMethod]
        public void RegistrarUsuarioPruebaNoExitoso()
        {
            // Arrange
            var cuentaUsuario = new CuentaUsuario
            {
                Usuario = "usuario1",
                Correo = "",
                Contraseña = "1234",
                Foto = null,
                Puntaje = 0,
                Estado = false
            };

            // Act
            var resultado = RegistroUsuario.RegistrarUsuario(cuentaUsuario);

            // Assert
            Assert.IsFalse(resultado);
        }
    }
}
