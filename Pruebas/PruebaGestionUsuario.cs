using DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Logica;
using System;
using Moq;
using System.Data.Entity;
using System.Linq;

namespace Pruebas
{
    [TestClass]
    public class PruebaGestionUsuario
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
        public void ModificarNombreUsuario_Exitoso()
        {
            int idCuenta = 1;
            string nuevoNombre = "NuevoNombre";

            string nombreOriginal;
            using (var contexto = new DobbleBDEntidades())
            {
                var cuentaUsuario = contexto.Cuenta.Find(idCuenta);
                nombreOriginal = cuentaUsuario?.nombreUsuario; 
            }

            try
            {
                bool resultado = ModificarUsuario.ModificarNombreUsuario(idCuenta, nuevoNombre);

                Assert.IsTrue(resultado, "El nombre de usuario debería haberse modificado correctamente.");
            }
            finally
            {
                ModificarUsuario.ModificarNombreUsuario(idCuenta, nombreOriginal);
            }
        }

        [TestMethod]
        public void ModificarNombreUsuario_IdNoExiste()
        {
            int idCuentaInexistente = 0; 
            string nuevoNombre = "NombrePrueba";

            bool resultado = ModificarUsuario.ModificarNombreUsuario(idCuentaInexistente, nuevoNombre);

            Assert.IsFalse(resultado, "El método debería devolver false cuando el ID de cuenta no existe.");
        }

        [TestMethod]
        public void ModificarNombreUsuario_NombreUsuarioExistente()
        {
            int idCuentaInexistente = 1;
            string nuevoNombre = "Gustavo";

            bool resultado = ModificarUsuario.ModificarNombreUsuario(idCuentaInexistente, nuevoNombre);

            Assert.IsFalse(resultado, "El método debería devolver false cuando el nombre de la cuenta es nulo.");
        }


        [TestMethod]
        public void ModificarContraseñaUsuario_Exitoso()
        {
            int idCuenta = 1; 
            var cuenta = contexto.Cuenta.FirstOrDefault(c => c.idCuenta == idCuenta);
            Assert.IsNotNull(cuenta, "La cuenta no existe en la base de datos.");

            string contraseñaOriginal = cuenta.contraseña;
            string nuevaContraseña = "nuevaContraseña"; 

            bool resultado = ModificarUsuario.ModificarContraseñaUsuario(idCuenta, nuevaContraseña);

            Assert.IsTrue(resultado, "La contraseña no fue modificada correctamente.");

            ModificarUsuario.ModificarContraseñaUsuario(idCuenta, contraseñaOriginal);
        }

        [TestMethod]
        public void ModificarContraseñaUsuario_IdNoExiste()
        {
            int idCuentaInexistente = 0; 
            string nuevaContraseña = "nuevaContraseña";

            bool resultado = ModificarUsuario.ModificarContraseñaUsuario(idCuentaInexistente, nuevaContraseña);

            Assert.IsFalse(resultado, "El método devolvió true para una cuenta inexistente.");
        }

        [TestMethod]
        public void ModificarContraseñaUsuario_ContraseñaNoModificada_DeberiaDevolverFalse()
        {
            int idCuenta = 1; 
            var cuenta = contexto.Cuenta.FirstOrDefault(c => c.idCuenta == idCuenta);
            Assert.IsNotNull(cuenta, "La cuenta no existe en la base de datos.");

            string contraseñaOriginal = cuenta.contraseña;

            bool resultado = ModificarUsuario.ModificarContraseñaUsuario(idCuenta, contraseñaOriginal);

            Assert.IsFalse(resultado, "El método devolvió true aunque la contraseña no fue modificada.");
        }


        [TestMethod]
        public void ModificarFotoUsuario_Exitoso()
        {
            int idCuenta = 1; 
            var usuario = contexto.Usuario.FirstOrDefault(c => c.idCuenta == idCuenta);
            Assert.IsNotNull(usuario, "El usuario no existe en la base de datos.");

            byte[] fotoOriginal = usuario.foto;
            byte[] nuevaFoto = new byte[] { 0x20, 0x21, 0x22 }; 

            bool resultado = ModificarUsuario.ModificarFotoUsuario(idCuenta, nuevaFoto);

            Assert.IsTrue(resultado, "La foto no fue modificada correctamente.");

            ModificarUsuario.ModificarFotoUsuario(idCuenta, fotoOriginal);
        }

        [TestMethod]
        public void ModificarFotoUsuario_IdNoExiste()
        {
            int idCuentaInexistente = 0; 
            byte[] nuevaFoto = new byte[] { 0x30, 0x31, 0x32 };

            bool resultado = ModificarUsuario.ModificarFotoUsuario(idCuentaInexistente, nuevaFoto);

            Assert.IsFalse(resultado, "El método devolvió true para un usuario inexistente.");
        }

        [TestMethod]
        public void ModificarFotoUsuario_FotoGrande()
        {
            int idCuenta = 1; 
            var usuario = contexto.Usuario.FirstOrDefault(c => c.idCuenta == idCuenta);
            Assert.IsNotNull(usuario, "El usuario no existe en la base de datos.");

            byte[] fotoOriginal = usuario.foto;
            byte[] fotoGrande = new byte[5 * 1024 * 1024];
            new Random().NextBytes(fotoGrande);

            bool resultado = ModificarUsuario.ModificarFotoUsuario(idCuenta, fotoGrande);

            Assert.IsTrue(resultado, "La foto grande no fue modificada correctamente.");

            ModificarUsuario.ModificarFotoUsuario(idCuenta, fotoOriginal);
        }


        [TestMethod]
        public void ValidarContraseña_Exitoso()
        {
            int idCuenta = 1; 
            var cuentaUsuario = contexto.Cuenta.FirstOrDefault(c => c.idCuenta == idCuenta);
            Assert.IsNotNull(cuentaUsuario, "La cuenta no existe en la base de datos.");

            string contraseñaCorrecta = cuentaUsuario.contraseña;

            bool resultado = ModificarUsuario.ValidarContraseña(idCuenta, contraseñaCorrecta);

            Assert.IsTrue(resultado, "La contraseña correcta no fue validada correctamente.");
        }

        [TestMethod]
        public void ValidarContraseña_ContraseñaIncorrecta()
        {
            int idCuenta = 1; 
            var cuentaUsuario = contexto.Cuenta.FirstOrDefault(c => c.idCuenta == idCuenta);
            Assert.IsNotNull(cuentaUsuario, "La cuenta no existe en la base de datos.");

            string contraseñaIncorrecta = "contraseñaIncorrecta123"; 

            bool resultado = ModificarUsuario.ValidarContraseña(idCuenta, contraseñaIncorrecta);

            Assert.IsFalse(resultado, "La contraseña incorrecta fue validada como correcta.");
        }

        [TestMethod]
        public void ValidarContraseña_CuentaInexistente()
        {
            int idCuentaInexistente = 0; 
            string contraseña = "cualquierContraseña";

            bool resultado = ModificarUsuario.ValidarContraseña(idCuentaInexistente, contraseña);

            Assert.IsFalse(resultado, "El método devolvió true para una cuenta inexistente.");
        }
    }
}
