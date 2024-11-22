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
                cuentaUsuario.ContextoOperacion = OperationContext.Current;
                cuentaUsuario.Estado = true;

                UsuariosActivos.AddOrUpdate(nombreUsuario, cuentaUsuario, (key, existingVal) => cuentaUsuario);
            }

            return respuestaServicio;
        }

        public void CerrarSesionJugador(string nombreUsuario, string mensaje)
        {
            foreach (var sala in salas.Values)
            {
                if (sala.Jugadores.Any(u => u.Usuario == nombreUsuario))
                {
                    AbandonarSala(nombreUsuario, sala.CodigoSala, mensaje);
                }
            }

            if (UsuariosActivos.TryRemove(nombreUsuario, out CuentaUsuario cuentaUsuario))
            {
                Console.WriteLine($"El usuario {nombreUsuario} ha sido desconectado");
            }
            else
            {
                Console.WriteLine($"No se encontró al usuario {nombreUsuario}");
            }
        }

        /*public RespuestaServicio<bool> ModificarNombreUsuario(int idUsuario, String nombreUsuario)
        {
            return GestorErrores.Ejecutar(() =>
            {
                bool exito = ModificarUsuario.ModificarNombreUsuario(idUsuario, nombreUsuario);

                var cuentaUsuario = UsuariosActivos.Values.FirstOrDefault(c => c.IdCuentaUsuario == idUsuario);
                if (exito && cuentaUsuario != null)
                {
                    cuentaUsuario.Usuario = nombreUsuario;
                    UsuariosActivos.AddOrUpdate(nombreUsuario, cuentaUsuario, (key, odlValue) => cuentaUsuario);

                    // Itera sobre todos los clientes conectados
                    foreach (var cliente in clientesConectados.Values)
                    {
                        // Llama al método de callback para notificar a cada cliente
                        cliente.NotificarCambio();
                    }
                }

                return exito;
            });
        }*/
        public RespuestaServicio<bool> ModificarNombreUsuario(int idUsuario, string nombreUsuario)
        {
            return GestorErrores.Ejecutar(() =>
            {
                bool exito = ModificarUsuario.ModificarNombreUsuario(idUsuario, nombreUsuario);

                var cuentaUsuario = UsuariosActivos.Values.FirstOrDefault(c => c.IdCuentaUsuario == idUsuario);

                if (exito && cuentaUsuario != null)
                {
                    // Actualizar la información del usuario
                    string nombreUsuarioAntiguo = cuentaUsuario.Usuario;
                    cuentaUsuario.Usuario = nombreUsuario;

                    // Actualizar el diccionario UsuariosActivos
                    UsuariosActivos.TryRemove(nombreUsuarioAntiguo, out _); // Elimina la entrada vieja
                    UsuariosActivos.AddOrUpdate(nombreUsuario, cuentaUsuario, (key, oldValue) => cuentaUsuario);

                    // Actualizar el diccionario clientesConectados
                    if (clientesConectados.TryRemove(nombreUsuarioAntiguo, out var callback))
                    {
                        clientesConectados.TryAdd(nombreUsuario, callback); // Añadir el cliente con el nuevo nombre
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
                bool exito = ModificarUsuario.ModificarFotoUsuario(idCuenta, fotoUsuario);

                var cuentaUsuario = UsuariosActivos.Values.FirstOrDefault(c => c.IdCuentaUsuario == idCuenta);
                if (exito && cuentaUsuario != null)
                {
                    cuentaUsuario.Foto = fotoUsuario;
                    UsuariosActivos.AddOrUpdate(cuentaUsuario.Usuario, cuentaUsuario, (key, oldValue) => cuentaUsuario);

                    // Itera sobre todos los clientes conectados
                    foreach (var cliente in clientesConectados.Values)
                    {
                        // Llama al método de callback para notificar a cada cliente
                        cliente.NotificarCambio();
                    }
                }

                return exito;
            });
        }

        public RespuestaServicio<bool> ValidarContraseña(int idUsuario, String contraseñaIngresada)
        {
            return GestorErrores.Ejecutar(() =>
            {
                return ModificarUsuario.ValidarContraseña(idUsuario, contraseñaIngresada);
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
                    Jugador cuentaUsuario = sala.Jugadores.FirstOrDefault(c => c.Usuario == nombreUsuario);
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

            if (resultado == false && salas.ContainsKey(codigoSala))
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
                Jugador usuarioAExpulsar = sala.Jugadores.FirstOrDefault(u => u.Usuario == nombreUsuario);
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

        private void AsignarNuevoAnfitrion(Sala sala, string usuarioActual)
        {
            lock(sala.BloqueoSala)
            {
                GestorErrores.EjecutarConManejoDeExcepciones(() =>
                {
                    var cuentaActual = sala.Jugadores.FirstOrDefault(u => u.Usuario == usuarioActual && u.EsAnfitrion);
                    if (cuentaActual != null)
                    {
                        cuentaActual.EsAnfitrion = false;

                        var nuevoAnfitrion = sala.Jugadores.FirstOrDefault(u => u.Usuario != usuarioActual);
                        if (nuevoAnfitrion != null)
                        {
                            nuevoAnfitrion.EsAnfitrion = true;
                            NotificarUsuarioSala(nuevoAnfitrion, callback => callback.ConvertirEnAnfitrion());
                        }
                    }
                });
            }
        }

        public bool CrearNuevaSala(string nombreAnfitrion, string codigoSala)
        {
            if (string.IsNullOrEmpty(nombreAnfitrion) || string.IsNullOrEmpty(codigoSala))
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
                    var usuarioEmisor = sala.Jugadores.FirstOrDefault(u => u.Usuario.Equals(nombreUsuario));
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

        private void EnviarNotificacionUsuarios(Sala sala)
        {
            var usuariosParaNotificar = sala.Jugadores.ToList();

            foreach (var jugador in usuariosParaNotificar)
            {
                NotificarUsuarioSala(jugador, callback => callback.ActualizarUsuariosConectados(sala.Jugadores));
            }
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

        private void NotificarUsuarioSala(Jugador jugador, Action<ISalaCallback> accion)
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
        // Diccionario para almacenar los callbacks de los clientes conectados
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

                if(nombreUsuario != null)
                {
                    if (eliminacionDeAmistad)
                    {
                        if (clientesConectados.TryGetValue(nombreUsuario, out var callback))
                        {
                            callback.NotificarCambio();
                        }
                    }
                }

                if (nombreUsuarioAmigo != null)
                {
                    if (eliminacionDeAmistad)
                    {
                        if (clientesConectados.TryGetValue(nombreUsuarioAmigo, out var callback))
                        {
                            callback.NotificarCambio();
                        }
                    }
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
                    // Verifica si el destinatario está conectado y tiene un callback disponible
                    if (clientesConectados.TryGetValue(nombreUsuarioAmigo, out var callback))
                    {
                        // Llama al método de callback para notificar al destinatario
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
                // Guarda la solicitud de amistad en la base de datos
                bool solicitudEnviada = GestorAmistad.EnviarSolicitudAmistad(idUsuarioPrincipal, nombreUsuarioAmigo);

                if (solicitudEnviada)
                {
                    // Verifica si el destinatario está conectado y tiene un callback disponible
                    if (clientesConectados.TryGetValue(nombreUsuarioAmigo, out var callback))
                    {
                        // Llama al método de callback para notificar al destinatario
                        callback.NotificarSolicitudAmistad();
                    }
                }

                return solicitudEnviada;
            });
        }

        // Método para conectar un cliente y registrar su callback
        public void ConectarCliente(string nombreUsuario)
        {
            var callback = OperationContext.Current.GetCallbackChannel<IGestionAmigosCallback>();
            clientesConectados.TryAdd(nombreUsuario, callback);
        }


        // Método para desconectar un cliente y eliminar su callback
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


        public void NotificarCambios()
        {
            foreach (var cliente in clientesConectados.Values)
            {
                cliente.NotificarCambio();
            }
        }

        public void NotificarDesconexion(string nombreUsuario)
        {
            foreach (var cliente in clientesConectados.Values)
            {
                cliente.NotificarSalida(nombreUsuario);
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
                if (salas.TryGetValue(codigoSala, out Sala sala))
                {
                    if (sala.PartidaSala != null)
                    {
                        lock (sala.PartidaSala.BloqueoPartida)
                        {
                            var jugador = sala.Jugadores.FirstOrDefault(j => j.Usuario == nombreUsuario);
                            if (jugador != null)
                            {
                                jugador.ContextoOperacion = OperationContext.Current;
                                sala.PartidaSala.JugadoresEnPartida.Add(jugador);
                                sala.Jugadores.Remove(jugador);
                            }
                        }
                    }
                }
            }); 
        }

        public bool AbandonarPartida(string nombreUsuario, string codigoSala)
        {
            if (salas.TryGetValue(codigoSala, out Sala sala))
            {
                return GestorErrores.EjecutarConManejoDeExcepciones(() =>
                {
                    lock (sala.PartidaSala.BloqueoPartida)
                    {
                        Jugador jugador = sala.PartidaSala.JugadoresEnPartida.FirstOrDefault(c => c.Usuario.Equals(nombreUsuario));
                        if (jugador == null) return false;

                        sala.PartidaSala.JugadoresEnPartida.Remove(jugador);
                        sala.Jugadores.Remove(jugador);

                        if (sala.PartidaSala.JugadoresEnPartida.Count == 0)
                        {
                            salas.TryRemove(codigoSala, out _);
                        }
                        else
                        {
                            NotificarActualizacionDeJugadoresEnPartida(codigoSala);
                        }

                        return true;
                    }
                }, false);
            }

            return false;
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
                        sala.PartidaSala.CartaCentral = cartas.Dequeue();

                        AsignarCartaCentral(sala, sala.PartidaSala.CartaCentral);
                        
                        foreach (var jugador in sala.PartidaSala.JugadoresEnPartida.ToList())
                        {
                            var carta = cartas.Dequeue();
                            AsignarCartaJugador(jugador, carta);
                        }
                    });
                }
            }
        }

        private void AsignarCartaCentral(Sala sala, Carta cartaCentral)
        {
            sala.PartidaSala.CartaCentral = cartaCentral;   
            Parallel.ForEach(sala.PartidaSala.JugadoresEnPartida, jugador =>
            {
                NotificarUsuarioPartida(jugador, callback =>
                callback.AsignarCartaCentral(cartaCentral));
            });
        }

        private void AsignarCartaJugador(Jugador jugador, Carta carta)
        {
            NotificarUsuarioPartida(jugador, callback => callback.AsignarCarta(carta));
        }

        public void ValidarCarta(string nombreUsuario, string rutaIcono, string codigoSala)
        {
            if (salas.TryGetValue(codigoSala, out Sala sala))
            {
                lock (sala.BloqueoSala)
                {
                    GestorErrores.EjecutarConManejoDeExcepciones(() =>
                    {
                        Carta cartaCentral = sala.PartidaSala.CartaCentral;
                        Jugador jugador = sala.PartidaSala.JugadoresEnPartida.FirstOrDefault(j => j.Usuario == nombreUsuario);

                        if (cartaCentral.Iconos.Any(i => i.Ruta == rutaIcono))
                        {
                            jugador.PuntosEnPartida++;
                            NotificarActualizacionDePuntosEnPartida(jugador.Usuario, jugador.PuntosEnPartida, sala);
                            AsignarCartaJugador(jugador, cartaCentral);
                            
                            if (sala.PartidaSala.Cartas.Any())
                            {
                                AsignarCartaCentral(sala, sala.PartidaSala.Cartas.Dequeue());
                            }
                            else
                            {
                                NotificarFinDePartida(sala);
                            }
                        }
                    });
                }
            }
        }

        private void NotificarActualizacionDePuntosEnPartida(string nombreUsuario, int puntosEnPartida, Sala sala)
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

        private void NotificarFinDePartida(Sala sala)
        {
            GestorErrores.EjecutarConManejoDeExcepciones(() =>
            {
                Parallel.ForEach(sala.PartidaSala.JugadoresEnPartida, jugador =>
                {
                    NotificarUsuarioPartida(jugador, callback => callback.FinalizarPartida());
                });
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

        private void NotificarUsuarioPartida(Jugador jugador, Action<IPartidaCallback> accion)
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
