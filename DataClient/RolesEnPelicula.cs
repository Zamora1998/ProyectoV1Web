//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataClient
{
    using System;
    using System.Collections.Generic;
    
    public partial class RolesEnPelicula
    {
        public int RolEnPeliculaID { get; set; }
        public Nullable<int> RolID { get; set; }
        public Nullable<int> ActorStaffID { get; set; }
        public Nullable<int> IDPelic { get; set; }
        public Nullable<int> OrdenAparicion { get; set; }
    
        public virtual ActoresStaff ActoresStaff { get; set; }
        public virtual Peliculas Peliculas { get; set; }
        public virtual Rol Rol { get; set; }
    }
}
