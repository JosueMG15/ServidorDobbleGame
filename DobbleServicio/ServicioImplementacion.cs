using Logica;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DobbleServicio
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
                ConcurrencyMode = ConcurrencyMode.Multiple)]
    public partial class ServicioImplementacion : IGestionJugador, IGestionNotificacionesAmigos
    {
        private readonly ConcurrentDictionary<string, CuentaUsuario> UsuariosActivos = new ConcurrentDictionary<string, CuentaUsuario>();
        private readonly object bloqueoInicioSesion = new Object();
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

        public RespuestaServicio<bool> ExisteCorreoAsociado(string correoUsuario)
        {
            return GestorErrores.Ejecutar(() =>
            {
                return RegistroUsuario.ExisteCorreoAsociado(correoUsuario);
            });
        }

        public RespuestaServicio<CuentaUsuario> IniciarSesionJugador(string nombreUsuario, string contraseña)
        {
            RespuestaServicio<CuentaUsuario> respuestaServicio = new RespuestaServicio<CuentaUsuario>();
            CuentaUsuario cuentaUsuario = null;

            lock (bloqueoInicioSesion)
            {
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
                    cuentaUsuario.ContextoOperacion = OperationContext.Current;
                    cuentaUsuario.Estado = true;

                    UsuariosActivos.AddOrUpdate(nombreUsuario, cuentaUsuario, (llave, valorExistente) => cuentaUsuario);
                }
            }

            return respuestaServicio;
        }

        public RespuestaServicio<CuentaUsuario> IniciarSesionInvitado(string nombreUsuario, byte[] foto)
        {
            RespuestaServicio<CuentaUsuario> respuestaServicio = new RespuestaServicio<CuentaUsuario>();
            respuestaServicio.Exitoso = false;
            respuestaServicio.ErrorBD = false;
            CuentaUsuario cuentaInvitado = null;

            lock (bloqueoInicioSesion)
            {
                if (!UsuariosActivos.ContainsKey(nombreUsuario))
                {
                    cuentaInvitado = new CuentaUsuario()
                    {
                        Usuario = nombreUsuario,
                        Foto = foto,
                        ContextoOperacion = OperationContext.Current
                    };

                    UsuariosActivos.AddOrUpdate(nombreUsuario, cuentaInvitado, (llave, valorExistente) => cuentaInvitado);

                    respuestaServicio.Resultado = cuentaInvitado;
                    respuestaServicio.Exitoso = true;
                }
            }

            return respuestaServicio;
        }

        public void CerrarSesionJugador(string nombreUsuario, string mensaje)
        {
            foreach (var sala in salas.Values)
            {
                if (sala.Jugadores.Exists(u => u.Usuario == nombreUsuario))
                {
                    AbandonarSala(nombreUsuario, sala.CodigoSala, mensaje);
                }
            }

            if (UsuariosActivos.TryRemove(nombreUsuario, out CuentaUsuario cuentaUsuario))
            {
                Console.WriteLine($"El usuario {nombreUsuario} ha sido desconectado");
            }
        }

        public RespuestaServicio<bool> ModificarNombreUsuario(int idCuenta, string nombreUsuario)
        {
            return GestorErrores.Ejecutar(() =>
            {
                bool exito = ModificarUsuario.ModificarNombreUsuario(idCuenta, nombreUsuario);

                var cuentaUsuario = UsuariosActivos.Values.FirstOrDefault(c => c.IdCuentaUsuario == idCuenta);

                if (exito && cuentaUsuario != null)
                {
                    string nombreUsuarioAntiguo = cuentaUsuario.Usuario;
                    cuentaUsuario.Usuario = nombreUsuario;

                    UsuariosActivos.TryRemove(nombreUsuarioAntiguo, out _); 
                    UsuariosActivos.AddOrUpdate(nombreUsuario, cuentaUsuario, (key, oldValue) => cuentaUsuario);

                    if (clientesConectados.TryRemove(nombreUsuarioAntiguo, out var callback))
                    {
                        clientesConectados.TryAdd(nombreUsuario, callback); 
                    }

                    foreach (var cliente in clientesConectados.Values)
                    {
                        try
                        {
                            cliente.NotificarCambio();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al notificar al cliente: {ex.Message}");
                        }
                    }
                }

                return exito;
            });
        }


        public RespuestaServicio<bool> ModificarContraseñaUsuario(int idCuenta, String contraseñaUsuario)
        {
            return GestorErrores.Ejecutar(() =>
            {
                return ModificarUsuario.ModificarContraseñaUsuario(idCuenta, contraseñaUsuario);
            });
        }

        public RespuestaServicio<bool> ModificarFotoUsuario(int idCuenta, byte[] fotoUsuario)
        {
            return GestorErrores.Ejecutar(() =>
            {
                bool exito = ModificarUsuario.ModificarFotoUsuario(idCuenta, fotoUsuario);

                var cuentaUsuario = UsuariosActivos.Values.FirstOrDefault(c => c.IdCuentaUsuario == idCuenta);
                if (exito && cuentaUsuario != null)
                {
                    cuentaUsuario.Foto = fotoUsuario;
                    UsuariosActivos.AddOrUpdate(cuentaUsuario.Usuario, cuentaUsuario, (key, oldValue) => cuentaUsuario);

                    foreach (var cliente in clientesConectados.Values)
                    {
                        cliente.NotificarCambio();
                    }
                }

                return exito;
            });
        }

        public RespuestaServicio<bool> ValidarContraseña(int idCuenta, string contraseñaUsuario)
        {
            return GestorErrores.Ejecutar(() =>
            {
                return ModificarUsuario.ValidarContraseña(idCuenta, contraseñaUsuario);
            });
        }

        public RespuestaServicio<CuentaUsuario> ObtenerUsuarioPorCorreo(string correo)
        {
            return GestorErrores.Ejecutar(() =>
            {
                var obtenerUsuario = RegistroUsuario.ObtenerUsuarioPorCorreo(correo);
                return obtenerUsuario;
            });
        }

        public bool Ping(string nombreUsuario)
        {
            return UsuariosActivos.TryGetValue(nombreUsuario, out _);
        }

        public RespuestaServicio<int?> ObtenerPuntosUsuario(string nombreUsuario)
        {
            return GestorErrores.Ejecutar(() =>
            {
                var puntosUsuario = RegistroUsuario.ObtenerPuntosUsuario(nombreUsuario);
                return puntosUsuario;
            });
        }
    }

    public partial class ServicioImplementacion : IGestionSala
    {
        private readonly ConcurrentDictionary<string, Sala> salas = new ConcurrentDictionary<string, Sala>();

        public bool AbandonarSala(string nombreUsuario, string codigoSala, string mensaje)
        {
            if (!salas.TryGetValue(codigoSala, out Sala sala))
            {
                return false;
            }

            bool resultado = GestorErrores.EjecutarConManejoDeExcepciones(() =>
            {
                lock (sala.BloqueoSala)
                {
                    Jugador cuentaUsuario = sala.Jugadores.Find(c => c.Usuario == nombreUsuario);
                    if (cuentaUsuario == null)
                    {
                        return false;
                    }

                    if (sala.Jugadores.Count > 1)
                    {
                        AsignarNuevoAnfitrion(sala, nombreUsuario);
                    }

                    sala.Jugadores.Remove(cuentaUsuario);

                    return sala.Jugadores.Count == 0 && salas.TryRemove(codigoSala, out _);
                }
            }, false);

            if (resultado && salas.ContainsKey(codigoSala))
            {
                EnviarMensajeConexionSala(nombreUsuario, codigoSala, mensaje);
                NotificarUsuarioConectado(codigoSala);
            }

            return resultado;
        }

        public void ExpulsarJugador(string nombreUsuario, string codigoSala, string mensaje)
        {
            if (!salas.TryGetValue(codigoSala, out Sala sala))
            {
                return;
            }

            bool resultado = GestorErrores.EjecutarConManejoDeExcepciones(() =>
            {
                Jugador usuarioAExpulsar = sala.Jugadores.Find(u => u.Usuario == nombreUsuario);
                if (usuarioAExpulsar == null)
                {
                    return false;
                }

                NotificarUsuarioSala(usuarioAExpulsar, callback => callback.NotificarExpulsionAJugador());

                sala.Jugadores.Remove(usuarioAExpulsar);
                return true;
            }, false);

            if (resultado && salas.ContainsKey(codigoSala))
            {
                EnviarMensajeConexionSala(nombreUsuario, codigoSala, mensaje);
                NotificarUsuarioConectado(codigoSala);
            }
        }

        private static void AsignarNuevoAnfitrion(Sala sala, string usuarioActual)
        {
            lock(sala.BloqueoSala)
            {
                GestorErrores.EjecutarConManejoDeExcepciones(() =>
                {
                    var cuentaActual = sala.Jugadores.Find(u => u.Usuario == usuarioActual && u.EsAnfitrion);
                    if (cuentaActual != null)
                    {
                        cuentaActual.EsAnfitrion = false;

                        var nuevoAnfitrion = sala.Jugadores.Find(u => u.Usuario != usuarioActual);
                        if (nuevoAnfitrion != null)
                        {
                            nuevoAnfitrion.EsAnfitrion = true;
                            NotificarUsuarioSala(nuevoAnfitrion, callback => callback.ConvertirEnAnfitrion(nuevoAnfitrion.Usuario));
                        }
                    }
                });
            }
        }

        public bool CrearNuevaSala(string nombreUsuario, string codigoSala)
        {
            if (string.IsNullOrEmpty(nombreUsuario) || string.IsNullOrEmpty(codigoSala))
            {
                return false;
            }

            return GestorErrores.EjecutarConManejoDeExcepciones(() =>
            {
                var sala = new Sala(codigoSala);
                return (salas.TryAdd(codigoSala, sala));
            }, false);
        }

        public void EnviarMensajeSala(string nombreUsuario, string codigoSala, string mensaje)
        {
            if (salas.TryGetValue(codigoSala, out Sala sala))
            {
                lock (sala.BloqueoSala)
                {
                    var usuarioEmisor = sala.Jugadores.Find(u => u.Usuario.Equals(nombreUsuario));
                    string respuesta = usuarioEmisor != null ? $"{usuarioEmisor.Usuario}: {mensaje}" : string.Empty;

                    if (string.IsNullOrWhiteSpace(respuesta))
                    {
                        Console.WriteLine("No se pudo enviar un mensaje vacío");
                        return;
                    }

                    foreach (var jugador in sala.Jugadores.ToList())
                    {
                        NotificarUsuarioSala(jugador, callback => callback.MostrarMensajeSala(respuesta));
                    }
                }
            }
        }

        public void EnviarMensajeConexionSala(string nombreUsuario, string codigoSala, string mensaje)
        {
            if (salas.TryGetValue(codigoSala, out Sala sala))
            {
                lock (sala.BloqueoSala)
                {
                    string respuesta = $"{nombreUsuario} {mensaje}";

                    foreach (var jugador in sala.Jugadores.ToList())
                    {
                        NotificarUsuarioSala(jugador, callback => callback.MostrarMensajeSala(respuesta));
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
            if (string.IsNullOrEmpty(nombreUsuario) || string.IsNullOrEmpty(codigoSala))
                return false;

            if (!UsuariosActivos.TryGetValue(nombreUsuario, out CuentaUsuario cuentaUsuario))
                return false;

            return GestorErrores.EjecutarConManejoDeExcepciones(() =>
            {
                if (salas.TryGetValue(codigoSala, out Sala sala))
                {
                    sala.Disponible = true;

                    Jugador jugador = new Jugador()
                    {
                        Usuario = cuentaUsuario.Usuario,
                        Foto = cuentaUsuario.Foto,
                        Puntaje = cuentaUsuario.Puntaje,
                        EsAnfitrion = esAnfitrion,
                        ContextoOperacion = OperationContext.Current
                    };

                    EnviarMensajeConexionSala(nombreUsuario, codigoSala, mensaje);

                    lock (sala.BloqueoSala)
                    {
                        if (!sala.Jugadores.Contains(jugador))
                        {
                            sala.Jugadores.Add(jugador);
                        }
                    }

                    return true;
                }
                return false;
            }, false);
        }

        public void NotificarUsuarioConectado(string codigoSala)
        {
            GestorErrores.EjecutarConManejoDeExcepciones(() =>
            {
                if (salas.TryGetValue(codigoSala, out Sala sala))
                {
                    lock (sala.BloqueoSala)
                    {
                        foreach (var jugador in sala.Jugadores.ToList())
                        {
                            NotificarUsuarioSala(jugador, callback => callback.ActualizarUsuariosConectados(sala.Jugadores));
                        }
                    }
                }
            });
        }

        public void NotificarJugadorListo(string nombreUsuario, string codigoSala)
        {
            GestorErrores.EjecutarConManejoDeExcepciones(() =>
            {
                if (salas.TryGetValue(codigoSala, out Sala sala))
                {
                    lock (sala.BloqueoSala)
                    {
                        var usuario = sala.Jugadores.Find(j => j.Usuario == nombreUsuario);

                        if (usuario == null)
                        {
                            return;
                        }

                        usuario.EstaListo = true;

                        foreach (var jugador in sala.Jugadores.ToList())
                        {
                            NotificarUsuarioSala(jugador, callback => callback.MostrarJugadorListo(nombreUsuario, usuario.EstaListo));
                        }
                    }
                }
            });
        }

        public bool TodosLosJugadoresEstanListos(string codigoSala)
        {
            return GestorErrores.EjecutarConManejoDeExcepciones(() =>
            {
                if (!salas.TryGetValue(codigoSala, out Sala sala))
                {
                    return false;
                }

                return sala.Jugadores.TrueForAll(j => j.EstaListo);
            });
        }

        public void CambiarVentanaParaTodos(string codigoSala)
        {
            GestorErrores.EjecutarConManejoDeExcepciones(() =>
            {
                if (salas.TryGetValue(codigoSala, out Sala sala))
                {
                    lock (sala.BloqueoSala)
                    {
                        foreach (var jugador in sala.Jugadores.ToList())
                        {
                            if (!jugador.EsAnfitrion)
                            {
                                NotificarUsuarioSala(jugador, callback => callback.CambiarVentana());
                            }
                        }
                    }
                }
            });
        }

        private static void NotificarUsuarioSala(Jugador jugador, Action<ISalaCallback> accion)
        {
            GestorErrores.EjecutarConManejoDeExcepciones(() =>
            {
                if (jugador.ContextoOperacion != null)
                {
                    var callback = jugador.ContextoOperacion.GetCallbackChannel<ISalaCallback>();
                    if (((ICommunicationObject)callback).State == CommunicationState.Opened)
                    {
                        accion(callback);
                    }
                }
            });
        }

        public bool HayEspacioSala(string codigoSala)
        {
            return salas.TryGetValue(codigoSala, out Sala sala) && sala.HayEspacioEnSala();
        }

        public bool ExisteSala(string codigoSala)
        {
            return salas.ContainsKey(codigoSala);
        }

        public bool EsSalaDisponible(string codigoSala)
        {
            return salas.TryGetValue(codigoSala, out Sala sala) && sala.Disponible;
        }

    }

    public partial class ServicioImplementacion : IGestionAmigos, IGestionNotificacionesAmigos
    {
        private readonly ConcurrentDictionary<string, IGestionAmigosCallback> clientesConectados =
            new ConcurrentDictionary<string, IGestionAmigosCallback>();

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

        public RespuestaServicio<bool> EliminarAmistad(int idAmistad, String nombreUsuario, String nombreUsuarioAmigo)
        {
            return GestorErrores.Ejecutar(() =>
            {
                bool eliminacionDeAmistad = GestorAmistad.EliminarAmistad(idAmistad);

                if(nombreUsuario != null && eliminacionDeAmistad && clientesConectados.TryGetValue(nombreUsuario, out var callback))
                {
                    callback.NotificarCambio();
                }

                if (nombreUsuarioAmigo != null & eliminacionDeAmistad && clientesConectados.TryGetValue(nombreUsuarioAmigo, out var callbackAmigo))
                {
                    callbackAmigo.NotificarCambio();
                }

                return eliminacionDeAmistad;
            });
        }

        public RespuestaServicio<bool> AceptarSolicitud(int idAmistad, String nombreUsuarioAmigo)
        {
            return GestorErrores.Ejecutar(() =>
            {
                bool aceptacionDeAmistad = GestorAmistad.AceptarSolicitud(idAmistad);

                if (aceptacionDeAmistad)
                {
                    if (clientesConectados.TryGetValue(nombreUsuarioAmigo, out var callback))
                    {
                        callback.NotificarCambio();
                    }
                }

                return aceptacionDeAmistad;
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

        public RespuestaServicio<Logica.Amistad> ObtenerSolicitud()
        {
            return GestorErrores.Ejecutar(() =>
            {
                var solicitud = GestorAmistad.ObtenerSolicitud();
                return solicitud;
            });
        }

        public RespuestaServicio<bool> EnviarSolicitudAmistad(int idUsuarioPrincipal, string nombreUsuarioAmigo)
        {
            return GestorErrores.Ejecutar(() =>
            {
                bool solicitudEnviada = GestorAmistad.EnviarSolicitudAmistad(idUsuarioPrincipal, nombreUsuarioAmigo);

                if (solicitudEnviada && clientesConectados.TryGetValue(nombreUsuarioAmigo, out var callback))
                {
                    callback.NotificarSolicitudAmistad();
                }

                return solicitudEnviada;
            });
        }

        public void ConectarCliente(string nombreUsuario)
        {
            var callback = OperationContext.Current.GetCallbackChannel<IGestionAmigosCallback>();
            clientesConectados.TryAdd(nombreUsuario, callback);
        }

        public void DesconectarCliente(string nombreUsuario)
        {
            clientesConectados.TryRemove(nombreUsuario, out _);
        }

        public RespuestaServicio<bool> UsuarioConectado(string nombreUsuario)
        {
            return GestorErrores.Ejecutar(() =>
            {
                return clientesConectados.ContainsKey(nombreUsuario);
            });
        }

        public void NotificarCambios(string nombreUsuario)
        {
            var clientesFiltrados = clientesConectados.Where(c => c.Key != nombreUsuario);

            foreach (var cliente in clientesFiltrados)
            {
                cliente.Value.NotificarCambio();
            }
        }

        public void NotificarDesconexion(string nombreUsuario)
        {
            foreach (var cliente in clientesConectados.Values)
            {
                cliente.NotificarSalida(nombreUsuario);
            }
        }

        public void NotificarBotonInvitacion(string nombreUsuario)
        {
            foreach (var cliente in clientesConectados.Values)
            {
                cliente.NotificarInvitacionCambio(nombreUsuario);
            }
        }

        public bool TieneInvitacionPendiente(string nombreUsuario)
        {
            return GestorErrores.EjecutarConManejoDeExcepciones(() =>
            {
                if (UsuariosActivos.TryGetValue(nombreUsuario, out CuentaUsuario usuario))
                {
                    return usuario.TieneInvitacionPendiente;
                }
                return false;
            });
        }

        public void ReestablecerInvitacionPendiente(string nombreUsuario)
        {
            GestorErrores.EjecutarConManejoDeExcepciones(() =>
            {
                if (UsuariosActivos.TryGetValue(nombreUsuario, out CuentaUsuario cuentaUsuario))
                {
                    cuentaUsuario.TieneInvitacionPendiente = false;
                }
            });
        }

        public void NotificarInvitacion(string nombreUsuario, string nombreUsuarioInvitacion, string codigoSala)
        {
            if (clientesConectados.TryGetValue(nombreUsuario, out var callback) && 
                UsuariosActivos.TryGetValue(nombreUsuario, out CuentaUsuario cuentaUsuario))
            {
                cuentaUsuario.TieneInvitacionPendiente = true;
                callback.NotificarVentanaInvitacion(nombreUsuarioInvitacion, codigoSala);
            }
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

            return GestorErrores.EjecutarConManejoDeExcepciones(() =>
            {
                if (salas.TryGetValue(codigoSala, out Sala sala))
                {
                    lock (sala.BloqueoSala)
                    {
                        var partida = new Partida();
                        sala.PartidaSala = partida;
                        sala.Disponible = false;
                    }
                    return true;
                }
                return false;
            }, false);
        }

        public void UnirJugadorAPartida(string nombreUsuario, string codigoSala)
        {
            GestorErrores.EjecutarConManejoDeExcepciones(() =>
            {
                if (salas.TryGetValue(codigoSala, out Sala sala) && sala.PartidaSala != null)
                {
                    lock (sala.PartidaSala.BloqueoPartida)
                    {
                        var jugador = sala.Jugadores.Find(j => j.Usuario == nombreUsuario);
                        if (jugador != null)
                        {
                            jugador.ContextoOperacion = OperationContext.Current;
                            sala.PartidaSala.JugadoresEnPartida.Add(jugador);
                            sala.Jugadores.Remove(jugador);
                        }
                    }
                }
            });
        }

        public bool AbandonarPartida(string nombreUsuario, string codigoSala)
        {
            if (!salas.TryGetValue(codigoSala, out Sala sala))
            {
                return false;
            }

            return GestorErrores.EjecutarConManejoDeExcepciones(() =>
            {
                lock (sala.PartidaSala.BloqueoPartida)
                {
                    Jugador jugador = sala.PartidaSala.JugadoresEnPartida.Find(j => j.Usuario.Equals(nombreUsuario));
                    if (jugador == null)
                    {
                        return false;
                    }

                    bool esAnfitrion = jugador.EsAnfitrion;
                    sala.PartidaSala.JugadoresEnPartida.Remove(jugador);

                    if (sala.PartidaSala.JugadoresEnPartida.Count == 0 && sala.Jugadores.Count == 0)
                    {
                        salas.TryRemove(codigoSala, out _);
                    }
                    else
                    {
                        if (esAnfitrion)
                        {
                            AsignarNuevoAnfitrionDesdePartida(sala, jugador.Usuario);
                        }

                        NotificarActualizacionDeJugadoresEnPartida(codigoSala);

                        if (sala.PartidaSala.JugadoresEnPartida.Count == 1)
                        {
                            NotificarFinDePartida(sala);
                        }
                    }

                    return true;
                }
            }, false);
        }

        public void RegresarASala(string nombreUsuario, string codigoSala)
        {
            if (salas.TryGetValue(codigoSala, out Sala sala))
            {
                GestorErrores.EjecutarConManejoDeExcepciones(() =>
                {
                    lock (sala.PartidaSala.BloqueoPartida)
                    {
                        Jugador jugador = sala.PartidaSala.JugadoresEnPartida.Find(j => j.Usuario.Equals(nombreUsuario));
                        if (jugador == null)
                        {
                            return;
                        }

                        sala.PartidaSala.JugadoresEnPartida.Remove(jugador);

                        if (sala.PartidaSala.JugadoresEnPartida.Count == 0)
                        {
                            sala.PartidaSala = null;
                        }
                    }
                });
            }
        }

        private static void AsignarNuevoAnfitrionDesdePartida(Sala sala, string usuarioActual)
        {
            lock (sala.BloqueoSala)
            {
                GestorErrores.EjecutarConManejoDeExcepciones(() =>
                {
                    var nuevoAnfitrion = sala.PartidaSala.JugadoresEnPartida.Find(u => u.Usuario != usuarioActual);
                    if (nuevoAnfitrion != null)
                    {
                        nuevoAnfitrion.EsAnfitrion = true;
                        NotificarUsuarioPartida(nuevoAnfitrion, callback => callback.ConvertirEnAnfitrionDesdePartida());
                    }
                    else
                    {
                        nuevoAnfitrion = sala.Jugadores.Find(u => u.Usuario != usuarioActual);

                        if (nuevoAnfitrion != null)
                        {
                            nuevoAnfitrion.EsAnfitrion = true;
                            NotificarUsuarioSala(nuevoAnfitrion, callback => callback.ConvertirEnAnfitrion(nuevoAnfitrion.Usuario));
                        }
                    }
                });
            }
        }

        public void NotificarInicioPartida(string codigoSala)
        {
            if (salas.TryGetValue(codigoSala, out Sala sala))
            {
                GestorErrores.EjecutarConManejoDeExcepciones(() =>
                {
                    Parallel.ForEach(sala.PartidaSala.JugadoresEnPartida, jugador =>
                    {
                        NotificarUsuarioPartida(jugador, callback => callback.IniciarPartida());
                    });
                });
            }
        }

        public void NotificarDistribucionCartas(string codigoSala)
        {
            if (salas.TryGetValue(codigoSala, out Sala sala))
            {
                lock (sala.BloqueoSala)
                {
                    GestorErrores.EjecutarConManejoDeExcepciones(() =>
                    {
                        var cartas = sala.PartidaSala.Cartas;

                        foreach (var jugador in sala.PartidaSala.JugadoresEnPartida.ToList())
                        {
                            var carta = cartas.Dequeue();
                            AsignarCartaJugador(jugador, carta);
                        }

                        sala.PartidaSala.CartaCentral = cartas.Dequeue();

                        AsignarCartaCentral(sala, sala.PartidaSala.CartaCentral, sala.PartidaSala.Cartas.Count);
                    });
                }
            }
        }

        private static void AsignarCartaCentral(Sala sala, Carta cartaCentral, int cartasRestantes)
        {
            sala.PartidaSala.CartaCentral = cartaCentral;
            Parallel.ForEach(sala.PartidaSala.JugadoresEnPartida, jugador =>
            {
                jugador.CartaBloqueada = false;
                NotificarUsuarioPartida(jugador, callback =>
                    callback.AsignarCartaCentral(cartaCentral, cartasRestantes + 1));
                NotificarUsuarioPartida(jugador, callback => callback.DesbloquearCarta());
            });
        }

        private static void AsignarCartaJugador(Jugador jugador, Carta carta)
        {
            NotificarUsuarioPartida(jugador, callback => callback.AsignarCarta(carta));
        }

        public void ValidarCarta(string nombreUsuario, string rutaIcono, string codigoSala)
        {
            if (!salas.TryGetValue(codigoSala, out Sala sala))
            {
                return;
            }

            lock (sala.BloqueoSala)
            {
                GestorErrores.EjecutarConManejoDeExcepciones(() =>
                {
                    Jugador jugador = sala.PartidaSala.JugadoresEnPartida.Find(j => j.Usuario == nombreUsuario);

                    if (jugador == null)
                    {
                        return;
                    }

                    Carta cartaCentral = sala.PartidaSala.CartaCentral;
                    if (cartaCentral.Iconos.Exists(i => i.Ruta == rutaIcono))
                    {
                        ProcesarCartaValida(sala, jugador, cartaCentral);
                    }
                    else
                    {
                        jugador.CartaBloqueada = true;
                        NotificarUsuarioPartida(jugador, callback => callback.BloquearCarta());

                        if (TodosTienenCartaBloqueada(sala.PartidaSala.JugadoresEnPartida))
                        {
                            AsignarNuevaCartaCentralOSalaFinalizada(sala);
                        }
                    }
                });
            }
        }

        private void ProcesarCartaValida(Sala sala, Jugador jugador, Carta cartaCentral)
        {
            jugador.PuntosEnPartida++;
            NotificarActualizacionDePuntosEnPartida(jugador.Usuario, jugador.PuntosEnPartida, sala);
            AsignarCartaJugador(jugador, cartaCentral);

            AsignarNuevaCartaCentralOSalaFinalizada(sala);
        }

        private void AsignarNuevaCartaCentralOSalaFinalizada(Sala sala)
        {
            if (sala.PartidaSala.Cartas.Any())
            {
                AsignarCartaCentral(sala, sala.PartidaSala.Cartas.Dequeue(), sala.PartidaSala.Cartas.Count);
            }
            else
            {
                NotificarFinDePartida(sala);
            }
        }

        private static bool TodosTienenCartaBloqueada(List<Jugador> jugadores)
        {
            return jugadores.TrueForAll(jugador => jugador.CartaBloqueada);
        }

        private static void NotificarActualizacionDePuntosEnPartida(string nombreUsuario, int puntosEnPartida, Sala sala)
        {
            GestorErrores.EjecutarConManejoDeExcepciones(() =>
            {
                Parallel.ForEach(sala.PartidaSala.JugadoresEnPartida, jugador =>
                {
                    NotificarUsuarioPartida(jugador, callback =>
                        callback.ActualizarPuntosEnPartida(nombreUsuario, puntosEnPartida));
                });
            });
        }

        private static void NotificarFinDePartida(Sala sala)
        {
            GestorErrores.EjecutarConManejoDeExcepciones(() =>
            {
                Parallel.ForEach(sala.PartidaSala.JugadoresEnPartida, jugador =>
                {
                    NotificarUsuarioPartida(jugador, callback => callback.FinalizarPartida());
                });
            });
        }

        public RespuestaServicio<bool> GuardarPuntosJugador(string nombreJugador, int puntosGanados)
        {
            return GestorErrores.Ejecutar(() =>
            {
                bool exito = RegistroUsuario.RegistrarPuntosGanados(nombreJugador, puntosGanados);

                var cuentaUsuario = UsuariosActivos.Values.FirstOrDefault(c => c.Usuario == nombreJugador);
                if (exito && cuentaUsuario != null)
                {
                    int? puntaje = RegistroUsuario.ObtenerPuntosUsuario(nombreJugador);

                    if (puntaje.HasValue)
                    {
                        cuentaUsuario.Puntaje = puntaje.Value;
                    }

                    UsuariosActivos.AddOrUpdate(cuentaUsuario.Usuario, cuentaUsuario, (key, oldValue) => cuentaUsuario);
                }

                return exito;
            });
        }
    

        public void NotificarActualizacionDeJugadoresEnPartida(string codigoSala)
        {
            if (salas.TryGetValue(codigoSala, out Sala sala))
            {
                lock (sala.BloqueoSala)
                {
                    GestorErrores.EjecutarConManejoDeExcepciones(() =>
                    {
                        Parallel.ForEach(sala.PartidaSala.JugadoresEnPartida, jugador =>
                        {
                            NotificarUsuarioPartida(jugador, callback => 
                                callback.ActualizarJugadoresEnPartida(sala.PartidaSala.JugadoresEnPartida));
                        });
                    });
                }
            }
        }

        private static void NotificarUsuarioPartida(Jugador jugador, Action<IPartidaCallback> accion)
        {
            GestorErrores.EjecutarConManejoDeExcepciones(() =>
            {
                if (jugador.ContextoOperacion != null)
                {
                    var callback = jugador.ContextoOperacion.GetCallbackChannel<IPartidaCallback>();
                    if (((ICommunicationObject)callback).State == CommunicationState.Opened)
                    {
                        accion(callback);
                    }
                }
            });
        }
    }

    public partial class ServicioImplementacion : IGestionCorreos
    {
        public RespuestaServicio<bool> EnviarCodigo(string correo, string codigo)
        {
            return GestorErrores.Ejecutar(() =>
            {
                return GestorCorreo.EnviarCorreo(correo, codigo);
            });
        }
    }
}
