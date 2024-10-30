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
        public void ExisteCorreoAsociado_DevuelveVerdadero()
        {
            Assert.IsTrue(RegistroUsuario.ExisteCorreoAsociado("correoprueba@hotmail.com"));
        }
        [TestMethod]
        public void ExisteCorreoAsociado_DevuelveFalso()
        {
            Assert.IsFalse(RegistroUsuario.ExisteCorreoAsociado("noexiste@hotmail.com"));
        }
    }
}
