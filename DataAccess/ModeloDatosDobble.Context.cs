﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataAccess
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DobbleBDEntities : DbContext
    {
        public DobbleBDEntities()
            : base("name=DobbleBDEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Cuenta_Usuario> Cuenta_Usuario { get; set; }
        public virtual DbSet<Estado_SolicitudAmistad> Estado_SolicitudAmistad { get; set; }
        public virtual DbSet<Relaciones_Amistad> Relaciones_Amistad { get; set; }
        public virtual DbSet<Usuario> Usuario { get; set; }
    }
}
