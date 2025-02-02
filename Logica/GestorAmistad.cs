﻿using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    public static class GestorAmistad
    {
        public static bool EnviarSolicitudAmistad(int idUsuarioPrincipal, String nombreUsuarioAmigo)
        {
            bool resultado = false;
            using (var contexto = new DobbleBDEntidades())
            {
                var usuarioAmigo = contexto.Cuenta.FirstOrDefault(c => c.nombreUsuario == nombreUsuarioAmigo);

                if (usuarioAmigo != null)
                {
                    var nuevaAmistad = new DataAccess.Amistad
                    {
                        estadoSolicitud = false,
                        UsuarioPrincipalId = idUsuarioPrincipal,
                        UsuarioAmigoId = usuarioAmigo.idCuenta
                    };

                    contexto.Amistad.Add(nuevaAmistad);
                    resultado = contexto.SaveChanges() > 0;
                }
            }
            return resultado;
        }

        public static bool AmistadYaExiste(int idUsuarioPrincipal, string nombreUsuarioAmigo)
        {
            using (var contexto = new DobbleBDEntidades())
            {
                var usuarioAmigo = contexto.Cuenta.FirstOrDefault(c => c.nombreUsuario == nombreUsuarioAmigo);

                if (usuarioAmigo != null)
                {
                    int idUsuarioAmigo = usuarioAmigo.idCuenta;

                    var amistadExistente = contexto.Amistad.Any(a =>
                        (a.UsuarioPrincipalId == idUsuarioPrincipal && a.UsuarioAmigoId == idUsuarioAmigo) ||
                        (a.UsuarioPrincipalId == idUsuarioAmigo && a.UsuarioAmigoId == idUsuarioPrincipal)
                    );

                    return amistadExistente;
                }

                return false;
            }
        }

        public static List<Logica.Amistad> ObtenerSolicitudesPendientes(int idUsuarioAmigo)
        {
            using (var contexto = new DobbleBDEntidades())
            {
                var solicitudesPendientes = contexto.Amistad
                    .Where(a => a.UsuarioAmigoId == idUsuarioAmigo && a.estadoSolicitud == false)
                    .ToList();

                // Mapeo manual de DataAccess.Amistad a Logica.Amistad
                var solicitudesPendientesLogica = solicitudesPendientes.Select(a => new Logica.Amistad
                {
                    IdAmistad = a.idAmistad,
                    EstadoSolicitud = a.estadoSolicitud ?? false,
                    UsuarioPrincipalId = a.UsuarioPrincipalId,
                    UsuarioAmigoId = a.UsuarioAmigoId
                    // Agrega otras propiedades si es necesario
                }).ToList();

                return solicitudesPendientesLogica;
            }
        }

        public static CuentaUsuario ObtenerUsuario(int idUsuario)
        {
            CuentaUsuario cuentaUsuario = null;
            using (var contexto = new DobbleBDEntidades())
            {
                var consulta = (from cuenta in contexto.Cuenta
                                join usuario in contexto.Usuario
                                on cuenta.Usuario.idCuenta
                                equals usuario.idCuenta
                                where cuenta.idCuenta == idUsuario
                                select new CuentaUsuario
                                {
                                    IdCuentaUsuario = cuenta.idCuenta,
                                    Usuario = cuenta.nombreUsuario,
                                    Correo = cuenta.correo,
                                    Contraseña = cuenta.contraseña,
                                    Foto = usuario.foto,
                                    Puntaje = usuario.puntaje.Value,
                                }).Take(1);

                cuentaUsuario = consulta.FirstOrDefault();
            }

            return cuentaUsuario;
        }

        public static CuentaUsuario ObtenerUsuarioPorNombre(String nombreUsuario)
        {
            CuentaUsuario cuentaUsuario = null;
            using (var contexto = new DobbleBDEntidades())
            {
                var consulta = (from cuenta in contexto.Cuenta
                                join usuario in contexto.Usuario
                                on cuenta.Usuario.idCuenta
                                equals usuario.idCuenta
                                where cuenta.nombreUsuario == nombreUsuario
                                select new CuentaUsuario
                                {
                                    IdCuentaUsuario = cuenta.idCuenta,
                                    Usuario = cuenta.nombreUsuario,
                                    Correo = cuenta.correo,
                                    Contraseña = cuenta.contraseña,
                                    Foto = usuario.foto,
                                    Puntaje = usuario.puntaje.Value,
                                }).Take(1);

                cuentaUsuario = consulta.FirstOrDefault();
            }

            return cuentaUsuario;
        }

        public static bool AceptarSolicitud(int idAmistad)
        {
            using (var contexto = new DobbleBDEntidades())
            {
                var amistad = contexto.Amistad.FirstOrDefault(u => u.idAmistad == idAmistad);

                if (amistad == null)
                {
                    return false;
                }

                amistad.estadoSolicitud = true;

                return contexto.SaveChanges() > 0;
            }
        }

        public static bool EliminarAmistad(int idAmistad)
        {
            using (var contexto = new DobbleBDEntidades())
            {
                var amistadAEliminar = contexto.Amistad.FirstOrDefault(u => u.idAmistad == idAmistad);

                if (amistadAEliminar == null)
                {
                    return false;
                }

                contexto.Amistad.Remove(amistadAEliminar);

                return contexto.SaveChanges() > 0;
            }
        }

        public static List<Logica.Amistad> ObtenerAmistades(int idUsuario)
        {
            using (var contexto = new DobbleBDEntidades())
            {
                var amistades = contexto.Amistad
                    .Where(a => (a.estadoSolicitud == true && a.UsuarioPrincipalId == idUsuario) ||
                                (a.estadoSolicitud == true && a.UsuarioAmigoId == idUsuario))
                    .ToList();

                var amistadesLogica = amistades.Select(a => new Logica.Amistad
                {
                    IdAmistad = a.idAmistad,
                    EstadoSolicitud = a.estadoSolicitud ?? true,
                    UsuarioPrincipalId = a.UsuarioPrincipalId,
                    UsuarioAmigoId = a.UsuarioAmigoId

                }).ToList();

                return amistadesLogica;
            }
        }

        public static Logica.Amistad ObtenerAmistad(int idAmistad)
        {
            using (var contexto = new DobbleBDEntidades())
            {
                var amistad = contexto.Amistad
                    .FirstOrDefault(a => a.idAmistad == idAmistad);

                if (amistad == null)
                {
                    return null; 
                }

                var amistadLogica = new Logica.Amistad
                {
                    IdAmistad = amistad.idAmistad,
                    EstadoSolicitud = amistad.estadoSolicitud ?? true,
                    UsuarioPrincipalId = amistad.UsuarioPrincipalId,
                    UsuarioAmigoId = amistad.UsuarioAmigoId
                };

                return amistadLogica;
            }
        }

        public static Logica.Amistad ObtenerSolicitud()
        {
            using (var contexto = new DobbleBDEntidades())
            {
                var amistad = contexto.Amistad
                    .OrderByDescending(a => a.idAmistad)
                    .FirstOrDefault();

                if (amistad == null)
                {
                    return null; 
                }

                var amistadLogica = new Logica.Amistad
                {
                    IdAmistad = amistad.idAmistad,
                    EstadoSolicitud = amistad.estadoSolicitud ?? false,
                    UsuarioPrincipalId = amistad.UsuarioPrincipalId,
                    UsuarioAmigoId = amistad.UsuarioAmigoId
                };

                return amistadLogica;
            }
        }
    }
}