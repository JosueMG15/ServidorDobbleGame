using Logica;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DobbleServicio
{
    public partial class ServicioImplementacion : IGestionJugador
    {
        public RespuestaServicio<bool> RegistrarUsuario(CuentaUsuario cuentaUsuario)
        {
            return GestorErrores.Ejecutar(() =>
            {
                return RegistroUsuario.RegistrarUsuario(cuentaUsuario);
            });
        }

        public RespuestaServicio<bool> ExisteNombreUsuario(string nombreUsuario)
        {
            return GestorErrores.Ejecutar(() =>
            {
                return RegistroUsuario.ExisteNombreUsuario(nombreUsuario);
            });
        }

        public RespuestaServicio<bool> ExisteCorreoAsociado(string correo)
        {
            return GestorErrores.Ejecutar(() =>
            {
                return RegistroUsuario.ExisteCorreoAsociado(correo);
            });
        }

        public RespuestaServicio<CuentaUsuario> IniciarSesionJugador(string nombreUsuario, string contraseña)
        {
            return GestorErrores.Ejecutar(() =>
            {
                return RegistroUsuario.IniciarSesion(nombreUsuario, contraseña);
            });
        }

        public RespuestaServicio<bool> ModificarNombreUsuario(int idUsuario, String nombreUsuario)
        {
            return GestorErrores.Ejecutar(() =>
            {
                return ModificarUsuario.ModificarNombreUsuario(idUsuario, nombreUsuario);
            });
        }

        public RespuestaServicio<bool> ModificarContraseñaUsuario(int idUsuario, String contraseñaUsuario)
        {
            return GestorErrores.Ejecutar(() =>
            {
                return ModificarUsuario.ModificarContraseñaUsuario(idUsuario, contraseñaUsuario);
            });
        }

        public RespuestaServicio<bool> ModificarFotoUsuario(int idCuenta, byte[] fotoUsuario)
        {
            return GestorErrores.Ejecutar(() =>
            {
                return ModificarUsuario.ModificarFotoUsuario(idCuenta, fotoUsuario);
            });
        }

        public RespuestaServicio<bool> ValidarContraseña(int idUsuario, String contraseñaIngresada)
        {
            return GestorErrores.Ejecutar(() =>
            {
                return ModificarUsuario.ValidarContraseña(idUsuario, contraseñaIngresada);
            });
        }
    }


    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
                    ConcurrencyMode = ConcurrencyMode.Multiple)]
    public partial class ServicioImplementacion : IGestionSala
    {
        private readonly List<Sala> salas = new List<Sala>();
        private readonly object bloqueoObjeto = new object();

        public void AbandonarSala(string nombreUsuario, string codigoSala, string mensaje)
        {
            lock (bloqueoObjeto)
            {
                var sala = salas.FirstOrDefault(s => s.CodigoSala.Equals(codigoSala));
                if (sala == null) return;

                CuentaUsuario cuentaUsuario = null;
                cuentaUsuario = sala.CuentasUsuarios.FirstOrDefault(c => c.Usuario.Equals(nombreUsuario));

                if (cuentaUsuario != null)
                {
                    sala.CuentasUsuarios.Remove(cuentaUsuario);
                    if (sala.CuentasUsuarios.Count() == 0)
                    {
                        salas.Remove(sala);
                    }
                    else
                    {
                        EnviarMensajeSala(cuentaUsuario.Usuario, codigoSala, $"{cuentaUsuario.Usuario} {mensaje}");
                    }
                }
            }
        }

        public bool CrearNuevaSala(string nombreAnfitrion, string codigoSala)
        {
            lock (bloqueoObjeto)
            {
                if (string.IsNullOrEmpty(nombreAnfitrion) || string.IsNullOrEmpty(codigoSala))
                {
                    return false;
                }

                if (salas.Any(s => s.CodigoSala == codigoSala))
                {
                    return false;
                }

                var nuevaSala = new Sala()
                {
                    CodigoSala = codigoSala,
                    NombreAnfitrion = nombreAnfitrion,
                    CuentasUsuarios = new List<CuentaUsuario>()
                };
                salas.Add(nuevaSala);
                return true;
            }
        }

        public void EnviarMensajeSala(string nombreUsuario, string codigoSala, string mensaje)
        {
            lock (bloqueoObjeto)
            {
                var sala = salas.FirstOrDefault(s => s.CodigoSala.Equals(codigoSala));
                if (sala == null) return;

                var usuarioEmisor = sala.CuentasUsuarios.FirstOrDefault(u => u.Usuario.Equals(nombreUsuario));
                string respuesta = usuarioEmisor != null ? $"{usuarioEmisor.Usuario}: {mensaje}" : string.Empty;

                if (string.IsNullOrWhiteSpace(respuesta))
                {
                    Console.WriteLine("No se pudo enviar un mensaje vacío");
                    return;
                }

                foreach (var usuario in sala.CuentasUsuarios)
                {
                    try
                    {
                        if (usuario.ContextoOperacion != null)
                        {
                            var callback = usuario.ContextoOperacion.GetCallbackChannel<ISalaCallback>();
                            callback.MostrarMensajeSala(respuesta);
                        }
                        else
                        {
                            Console.WriteLine($"Contexto de operación no disponible para {usuario.Usuario}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public string GenerarCodigoNuevaSala()
        {
            return Guid.NewGuid().ToString();  
        }

        public void UnirseASala(string nombreUsuario, string codigoSala, string mensaje)
        {
            lock (bloqueoObjeto)
            {
                var sala = salas.FirstOrDefault(s => s.CodigoSala.Equals(codigoSala));
                if (sala == null) return;

                CuentaUsuario cuentaUsuario = new CuentaUsuario()
                {
                    Usuario = nombreUsuario,
                    ContextoOperacion = OperationContext.Current
                };

                if (sala.CuentasUsuarios.Count > 0)
                {
                    EnviarMensajeSala(cuentaUsuario.Usuario, codigoSala, $"{cuentaUsuario.Usuario}{mensaje}");
                }

                sala.CuentasUsuarios.Add(cuentaUsuario);
            }
        }
    }
}
