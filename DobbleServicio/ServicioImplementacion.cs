using Logica;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace DobbleServicio
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
                ConcurrencyMode = ConcurrencyMode.Multiple)]
    public partial class ServicioImplementacion : IGestionJugador
    {
        private readonly Dictionary<string, CuentaUsuario> UsuariosActivos = new Dictionary<string, CuentaUsuario>();
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
            RespuestaServicio<CuentaUsuario> respuestaServicio = new RespuestaServicio<CuentaUsuario>();
            CuentaUsuario cuentaUsuario = null;

            if (UsuariosActivos.ContainsKey(nombreUsuario))
            {
                respuestaServicio.ErrorBD = false;
                return respuestaServicio;
            }

            respuestaServicio = GestorErrores.Ejecutar(() =>
            {
                return RegistroUsuario.IniciarSesion(nombreUsuario, contraseña);
            });

            if (respuestaServicio.Resultado != null)
            {
                cuentaUsuario = respuestaServicio.Resultado;
            }

            if (cuentaUsuario != null)
            {
                cuentaUsuario.ContextoOperacion = OperationContext.Current;
                cuentaUsuario.Estado = true;
                UsuariosActivos.Add(nombreUsuario, cuentaUsuario);
            }

            return respuestaServicio;
        }

        public void CerrarSesionJugador(string nombreUsuario)
        {
            if (UsuariosActivos.ContainsKey(nombreUsuario))
            {
                UsuariosActivos.Remove(nombreUsuario);
            }
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
                        NotificarUsuarioConectado(codigoSala);
                    }

                    return true;
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

            return false;
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

            return false;
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

                foreach (var usuario in sala.CuentasUsuarios.ToList())
                {
                    try
                    {
                        if (usuario.ContextoOperacion != null)
                        {
                            var callback = usuario.ContextoOperacion.GetCallbackChannel<ISalaCallback>();

                            if (((ICommunicationObject)callback).State == CommunicationState.Opened)
                            {
                                callback.MostrarMensajeSala(respuesta);
                            }
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

                foreach (var usuario in sala.CuentasUsuarios.ToList())
                {
                    try
                    {
                        if (usuario.ContextoOperacion != null)
                        {
                            var callback = usuario.ContextoOperacion.GetCallbackChannel<ISalaCallback>();
                            if (((ICommunicationObject)callback).State == CommunicationState.Opened)
                            {
                                callback.MostrarMensajeSala(respuesta);
                            }
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

        public bool UnirseASala(string nombreUsuario, string codigoSala, string mensaje, bool esAnfitrion)
        {
            bool respuesta = false;

            var sala = salas.FirstOrDefault(s => s.CodigoSala.Equals(codigoSala));
            if (sala == null) return false;

            CuentaUsuario cuentaUsuario = UsuariosActivos[nombreUsuario];
            if (cuentaUsuario == null) return false;

            if (esAnfitrion) cuentaUsuario.EsAnfitrion = esAnfitrion;

            cuentaUsuario.ContextoOperacion = OperationContext.Current;

            try
            {
                lock (sala.BloqueoSala)
                {
                    if (sala.CuentasUsuarios.Count > 0)
                    {
                        EnviarMensajeConexionSala(nombreUsuario, codigoSala, mensaje);
                    }

                    sala.CuentasUsuarios.Add(cuentaUsuario);
                }

                respuesta = true;
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

            return respuesta;
        }

        public void NotificarUsuarioConectado(string codigoSala)
        {
            var sala = salas.FirstOrDefault(s => s.CodigoSala.Equals(codigoSala));
            if (sala == null) return;

            lock (sala.BloqueoSala)
            {
                try
                {
                    foreach (var usuario in sala.CuentasUsuarios.ToList())
                    {
                        if (usuario.ContextoOperacion != null)
                        {
                            var callback = usuario.ContextoOperacion.GetCallbackChannel<ISalaCallback>();
                            if (((ICommunicationObject)callback).State == CommunicationState.Opened)
                            {
                                callback.ActualizarUsuariosConectados(sala.CuentasUsuarios);
                            }
                        }
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

    public partial class ServicioImplementacion : IGestionPartida
    {
        public bool CrearNuevaPartida(string codigoSala)
        {
            if (string.IsNullOrEmpty(codigoSala))
            {
                return false;
            }

            var sala = salas.FirstOrDefault(s => s.CodigoSala.Equals(codigoSala));
            if (sala == null) return false;

            var partida = new Partida()
            {
                CuentasEnPartida = sala.CuentasUsuarios,
            };

            sala.partida = partida;

            try
            {
                lock (sala.partida.BloqueoPartida)
                {
                    sala.partida = partida;
                    return true;
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

            return false;
        }

        public void UnirJugadoresAPartida(string codigoSala)
        {
            var sala = salas.FirstOrDefault(s => s.CodigoSala.Equals(codigoSala));
            if (sala == null) return;

            lock (sala.partida.BloqueoPartida)
            {
                try
                {
                    foreach (var usuario in sala.partida.CuentasEnPartida.ToList())
                    {
                        if (usuario.ContextoOperacion != null)
                        {
                            var callback = usuario.ContextoOperacion.GetCallbackChannel<ISalaCallback>();
                            if (((ICommunicationObject)callback).State == CommunicationState.Opened)
                            {
                                callback.CambiarVentanaAPartida();
                            }
                        }
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
        public bool AbandonarPartida(string nombreUsuario, string codigoSala)
        {
            var sala = salas.FirstOrDefault(s => s.CodigoSala.Equals(codigoSala));
            if (sala == null) return false;

            try
            {
                lock (sala.partida.BloqueoPartida)
                {
                    CuentaUsuario cuentaUsuario = sala.partida.CuentasEnPartida.FirstOrDefault(c => c.Usuario.Equals(nombreUsuario));
                    if (cuentaUsuario == null) return false;

                    sala.partida.CuentasEnPartida.Remove(cuentaUsuario);

                    if (sala.partida.CuentasEnPartida.Count == 0)
                    {
                        sala.partida = null;
                    }
                    else
                    {
                        NotificarActualizacionDeJugadoresEnpartida(codigoSala);
                    }

                    return true;
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

            return false;
        }

        public void NotificarActualizacionDeJugadoresEnpartida(string codigoSala)
        {
            var sala = salas.FirstOrDefault(s => s.CodigoSala.Equals(codigoSala));
            if (sala == null) return;

            lock (sala.partida.BloqueoPartida)
            {
                try
                {
                    foreach (var usuario in sala.partida.CuentasEnPartida.ToList())
                    {
                        if (usuario.ContextoOperacion != null)
                        {
                            var callback = usuario.ContextoOperacion.GetCallbackChannel<IPartidaCallback>();
                            if (((ICommunicationObject)callback).State == CommunicationState.Opened)
                            {
                                callback.ActualizarJugadoresEnPartida(sala.partida.CuentasEnPartida);
                            }
                        }
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

}
