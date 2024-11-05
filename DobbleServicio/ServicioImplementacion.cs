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

    public partial class ServicioImplementacion : IGestionAmigos
    {
        public RespuestaServicio<bool> EnviarSolicitudAmistad(int idUsuarioPrincipal, String nombreUsuarioAmigo)
        {
            return GestorErrores.Ejecutar(() =>
            {
                return GestorAmistad.EnviarSolicitudAmistad(idUsuarioPrincipal, nombreUsuarioAmigo);
            });
        }

        public RespuestaServicio<bool> AmistadYaExiste(int idUsuarioPrincipal, String nombreUsuarioAmigo)
        {
            return GestorErrores.Ejecutar(() =>
            {
                return GestorAmistad.AmistadYaExiste(idUsuarioPrincipal, nombreUsuarioAmigo);
            });
        }

        public RespuestaServicio<List<Logica.Amistad>> ObtenerSolicitudesPendientes(int idUsuarioAmigo)
        {
            return GestorErrores.Ejecutar(() =>
            {
                var solicitudesPendientes = GestorAmistad.ObtenerSolicitudesPendientes(idUsuarioAmigo);
                return solicitudesPendientes;
            });
        }

        public RespuestaServicio<CuentaUsuario> ObtenerUsuario(int idUsuario)
        {
            return GestorErrores.Ejecutar(() =>
            {
                var obtenerUsuario = GestorAmistad.ObtenerUsuario(idUsuario);
                return obtenerUsuario;
            });
        }

        public RespuestaServicio<bool> EliminarAmistad(int idAmistad)
        {
            return GestorErrores.Ejecutar(() =>
            {
                return GestorAmistad.EliminarAmistad(idAmistad);
            });
        }

        public RespuestaServicio<bool> AceptarSolicitud(int idAmistad)
        {
            return GestorErrores.Ejecutar(() =>
            {
                return GestorAmistad.AceptarSolicitud(idAmistad);
            });
        }

        public RespuestaServicio<List<Logica.Amistad>> ObtenerAmistades(int idUsuario)
        {
            return GestorErrores.Ejecutar(() =>
            {
                var amistades = GestorAmistad.ObtenerAmistades(idUsuario);
                return amistades;
            });
        }

        public RespuestaServicio<Logica.Amistad> ObtenerAmistad(int idAmistad)
        {
            return GestorErrores.Ejecutar(() =>
            {
                var amistad = GestorAmistad.ObtenerAmistad(idAmistad);
                return amistad;
            });
        }
    }


    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
                    ConcurrencyMode = ConcurrencyMode.Multiple)]
    public partial class ServicioImplementacion : IGestionSala
    {
        private readonly List<Sala> salas = new List<Sala>();

        public bool AbandonarSala(string nombreUsuario, string codigoSala, string mensaje)
        {
            var sala = salas.FirstOrDefault(s => s.CodigoSala.Equals(codigoSala));
            if (sala == null) return false;

            try
            {
                lock (sala.BloqueoSala)
                {
                    CuentaUsuario cuentaUsuario = sala.CuentasUsuarios.FirstOrDefault(c => c.Usuario.Equals(nombreUsuario));
                    if (cuentaUsuario == null) return false;

                    sala.CuentasUsuarios.Remove(cuentaUsuario);

                    if (sala.CuentasUsuarios.Count == 0)
                    {
                        salas.Remove(sala);
                    }
                    else
                    {
                        EnviarMensajeConexionSala(nombreUsuario, codigoSala, mensaje);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public bool CrearNuevaSala(string nombreAnfitrion, string codigoSala)
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

            try
            {
                lock (nuevaSala.BloqueoSala)
                {
                    salas.Add(nuevaSala);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public void EnviarMensajeSala(string nombreUsuario, string codigoSala, string mensaje)
        {
            var sala = salas.FirstOrDefault(s => s.CodigoSala.Equals(codigoSala));
            if (sala == null) return;

            lock (sala.BloqueoSala)
            {
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
                    }
                    catch (CommunicationException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    catch (TimeoutException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
        }

        public void EnviarMensajeConexionSala(string nombreUsuario, string codigoSala, string mensaje)
        {
            var sala = salas.FirstOrDefault(s => s.CodigoSala.Equals(codigoSala));
            if (sala == null) return;

            lock (sala.BloqueoSala)
            {
                string respuesta = $"{nombreUsuario} {mensaje}";

                foreach (var usuario in  sala.CuentasUsuarios)
                {
                    try
                    {
                        if (usuario.ContextoOperacion != null)
                        {
                            var callback = usuario.ContextoOperacion.GetCallbackChannel<ISalaCallback>();
                            callback.MostrarMensajeSala(respuesta);
                        }
                    }
                    catch (CommunicationException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    catch (TimeoutException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
        }

        public string GenerarCodigoNuevaSala()
        {
            return Guid.NewGuid().ToString();  
        }

        public bool UnirseASala(string nombreUsuario, int puntaje, byte[] foto, string codigoSala, string mensaje)
        {
            bool respuesta = false;

            var sala = salas.FirstOrDefault(s => s.CodigoSala.Equals(codigoSala));
            if (sala == null) return false;

            CuentaUsuario cuentaUsuario = new CuentaUsuario()
            {
                Usuario = nombreUsuario,
                Puntaje = puntaje,
                Foto = foto,
                ContextoOperacion = OperationContext.Current
            };

            try
            {
                lock (sala.BloqueoSala)
                {
                    if (sala.CuentasUsuarios.Count > 0)
                    {
                        EnviarMensajeConexionSala(nombreUsuario, codigoSala, mensaje);
                    }
                    sala.CuentasUsuarios.Add(cuentaUsuario);

                    respuesta = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return respuesta;
        }
    }

}
