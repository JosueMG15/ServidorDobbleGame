using Logica;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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

        public void CerrarSesionJugador(string nombreUsuario)
        {
            if (UsuariosActivos.TryRemove(nombreUsuario, out CuentaUsuario cuentaUsuario))
            {
                Console.WriteLine($"El usuario {nombreUsuario} ha sido desconectado");
            }
            else
            {
                Console.WriteLine($"No se encontró al usuario {nombreUsuario}");
            }
        }

        public RespuestaServicio<bool> ModificarNombreUsuario(int idUsuario, String nombreUsuario)
        {
            return GestorErrores.Ejecutar(() =>
            {
                bool exito = ModificarUsuario.ModificarNombreUsuario(idUsuario, nombreUsuario);

                var cuentaUsuario = UsuariosActivos.Values.FirstOrDefault(c => c.IdCuentaUsuario == idUsuario);
                if (exito && cuentaUsuario != null)
                {
                    cuentaUsuario.Usuario = nombreUsuario;
                    UsuariosActivos.AddOrUpdate(nombreUsuario, cuentaUsuario, (key, odlValue) => cuentaUsuario);
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
    }

    public partial class ServicioImplementacion : IGestionSala
    {
        private readonly ConcurrentDictionary<string, Sala> salas = new ConcurrentDictionary<string, Sala>();

        public bool AbandonarSala(string nombreUsuario, string codigoSala, string mensaje)
        {
            if (salas.TryGetValue(codigoSala, out Sala sala))
            {
                lock (sala.BloqueoSala)
                {
                    CuentaUsuario cuentaUsuario = sala.Usuarios.FirstOrDefault(c => c.Usuario == nombreUsuario);
                    if (cuentaUsuario == null) return false;

                    if (sala.Usuarios.Count > 1)
                    {
                        AsignarNuevoAnfitrion(sala, nombreUsuario);
                    }

                    sala.Usuarios.Remove(cuentaUsuario);

                    if (sala.Usuarios.Count == 0 && salas.TryRemove(codigoSala, out _))
                    {
                        return true;
                    }
                }

                EnviarMensajeConexionSala(nombreUsuario, codigoSala, mensaje);
                NotificarUsuarioConectado(codigoSala);

                return true;
            }

            return false;
        }

        private void AsignarNuevoAnfitrion(Sala sala, string usuarioActual)
        {
            lock(sala.BloqueoSala)
            {
                var cuentaActual = sala.Usuarios.FirstOrDefault(u => u.Usuario == usuarioActual && u.EsAnfitrion);
                if (cuentaActual != null)
                {
                    cuentaActual.EsAnfitrion = false;

                    var nuevoAnfitrion = sala.Usuarios.FirstOrDefault(u => u.Usuario != usuarioActual);
                    if (nuevoAnfitrion != null)
                    {
                        nuevoAnfitrion.EsAnfitrion = true;
                    }
                }
            }
        }

        public bool CrearNuevaSala(string nombreAnfitrion, string codigoSala)
        {
            if (string.IsNullOrEmpty(nombreAnfitrion) || string.IsNullOrEmpty(codigoSala))
            {
                return false;
            }

            try
            {
                var sala = new Sala();
                if (salas.TryAdd(codigoSala, sala))
                {
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
            if (salas.TryGetValue(codigoSala, out Sala sala))
            {
                lock (sala.BloqueoSala)
                {
                    var usuarioEmisor = sala.Usuarios.FirstOrDefault(u => u.Usuario.Equals(nombreUsuario));
                    string respuesta = usuarioEmisor != null ? $"{usuarioEmisor.Usuario}: {mensaje}" : string.Empty;

                    if (string.IsNullOrWhiteSpace(respuesta))
                    {
                        Console.WriteLine("No se pudo enviar un mensaje vacío");
                        return;
                    }

                    foreach (var usuario in sala.Usuarios.ToList())
                    {
                        NotificarUsuario(usuario, callback => callback.MostrarMensajeSala(respuesta));
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

                    foreach (var usuario in sala.Usuarios.ToList())
                    {
                        NotificarUsuario(usuario, callback => callback.MostrarMensajeSala(respuesta));
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

            if (salas.TryGetValue(codigoSala, out Sala sala))
            {
                cuentaUsuario.ContextoOperacion = OperationContext.Current;
                cuentaUsuario.EsAnfitrion = esAnfitrion;
                EnviarMensajeConexionSala(nombreUsuario, codigoSala, mensaje);

                lock (sala.BloqueoSala)
                {
                    if (!sala.Usuarios.Contains(cuentaUsuario))
                    {
                        sala.Usuarios.Add(cuentaUsuario);
                    }
                }

                return true;
            }

            return false;
        }

        public void NotificarUsuarioConectado(string codigoSala)
        {
            if (salas.TryGetValue(codigoSala, out Sala sala))
            {
                lock (sala.BloqueoSala)
                {
                    EnviarNotificacionUsuarios(sala);

                    sala.Usuarios = sala.Usuarios
                        .Where(u => u.ContextoOperacion != null &&
                            ((ICommunicationObject)u.ContextoOperacion.GetCallbackChannel<ISalaCallback>()).State == CommunicationState.Opened).ToList();
                }
            }
        }

        public bool HayEspacioSala(string codigoSala)
        {
            if (salas.TryGetValue(codigoSala, out Sala sala) && sala.Usuarios.Count <= sala.U)
            {

            }
        }

        private void EnviarNotificacionUsuarios(Sala sala)
        {
            var usuariosParaNotificar = sala.Usuarios.ToList(); 

            foreach (var usuario in usuariosParaNotificar)
            {
                NotificarUsuario(usuario, callback => callback.ActualizarUsuariosConectados(sala.Usuarios));
            }
        }


        private void NotificarUsuario(CuentaUsuario usuario, Action<ISalaCallback> accion)
        {
            try
            {
                if (usuario.ContextoOperacion != null)
                {
                    var callback = usuario.ContextoOperacion.GetCallbackChannel<ISalaCallback>();
                    if (((ICommunicationObject)callback).State == CommunicationState.Opened)
                    {
                        accion(callback);
                    }
                }
            }
            catch(CommunicationException ex)
            {
                Console.WriteLine($"Erro de comunicación con {usuario.Usuario}: {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine($"Error de tiempo de espera con {usuario.Usuario}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general con {usuario.Usuario}: {ex}");
            }
        }
    }

    public partial class ServicioImplementacion : IGestionAmigos
    {
        public RespuestaServicio<bool> EnviarSolicitudAmistad(int idUsuarioPrincipal, String nombreUsuarioAmigo)
        {
            return GestorErrores.Ejecutar(() =>
            {
                // Llamamos al método en la capa lógica para crear la solicitud
                bool resultado = GestorAmistad.EnviarSolicitudAmistad(idUsuarioPrincipal, nombreUsuarioAmigo);

                /*if (resultado)
                {
                    // Obtener el canal de callback del cliente actual
                    var callback = OperationContext.Current.GetCallbackChannel<IAmistadCallback>();

                    // Invocar la notificación en el cliente destinatario
                    callback?.NotificarNuevaSolicitudAmistad(nombreUsuarioAmigo, idUsuarioPrincipal);
                }*/

                return resultado;
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

        //CALLBACKS
    }

    public partial class ServicioImplementacion : IGestionPartida
    {
        public bool CrearNuevaPartida(string codigoSala)
        {
            if (string.IsNullOrEmpty(codigoSala))
            {
                return false;
            }

            if (salas.TryGetValue(codigoSala, out Sala sala))
            {
                var partida = new Partida()
                {
                    CuentasEnPartida = sala.Usuarios,
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
            }


            return false;
        }

        public void UnirJugadoresAPartida(string codigoSala)
        {
            if (salas.TryGetValue(codigoSala, out Sala sala))

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
            if (salas.TryGetValue(codigoSala, out Sala sala))
            {
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
            }

            return false;
        }

        public void NotificarActualizacionDeJugadoresEnpartida(string codigoSala)
        {
            if (salas.TryGetValue(codigoSala, out Sala sala))

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
