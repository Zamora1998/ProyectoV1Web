using DataClient;
using ProyectoV1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ProyectoV1.Controllers
{
    public class RolesPeliculaFController : ApiController
    {
        private PeliculasProgra11Entities RolesPelicula = new PeliculasProgra11Entities();

        public RolesPeliculaFController()
        {
            RolesPelicula.Configuration.LazyLoadingEnabled = true;
            RolesPelicula.Configuration.ProxyCreationEnabled = true;
        } 

        [HttpPost]
        [Route("api/Roles/RegistrarTipoDeRol")]
        public IHttpActionResult RegistrarTipoDeRol(RegistrarRolesPelicula registroTipoDeRol)
        {
            try
            {
                var rolExistente = RolesPelicula.Rol
                    .FirstOrDefault(r => r.Nombre == registroTipoDeRol.Nombre);

                if (rolExistente != null)
                {
                    return Conflict(); 
                }

                var nuevoTipoDeRol = new Rol
                {
                    Nombre = registroTipoDeRol.Nombre
                };

                RolesPelicula.Rol.Add(nuevoTipoDeRol);
                RolesPelicula.SaveChanges();
                var requestUri = Request.RequestUri;
                var newResourceUrl = new Uri(requestUri, $"api/Roles/RegistrarTipoDeRol/{nuevoTipoDeRol.RolID}");

                return Created(newResourceUrl, nuevoTipoDeRol); 
            }
            catch (Exception)
            {
                return NotFound();
            }
        } 

        [HttpPut]
        [Route("api/Roles/EditarTipoDeInvolucrado/{id}")]
        public IHttpActionResult EditarTipoDeInvolucrado(int id, EditarRolesPelicula tipoDeInvolucradoEditado, int? noReturn)
        {
            var tipoDeInvolucradoExistente = RolesPelicula.Rol.Find(id);

            if (tipoDeInvolucradoExistente == null)
            {
                return NotFound(); 
            }

            var tipoDeInvolucradoConNuevoNombre = RolesPelicula.Rol
                .FirstOrDefault(r => r.Nombre == tipoDeInvolucradoEditado.Nombre && r.RolID != id);

            if (tipoDeInvolucradoConNuevoNombre != null)
            {
                return Conflict(); 
            }

            tipoDeInvolucradoExistente.Nombre = tipoDeInvolucradoEditado.Nombre;
            RolesPelicula.SaveChanges();

            if (noReturn == 1)
            {
                return StatusCode(HttpStatusCode.NoContent); 
            }
            else
            {
                return Ok(tipoDeInvolucradoExistente); 
            }
        }
         
        [HttpPut]
        [Route("api/Roles/EditarTipoDeInvolucradoNombre/{nombre}")]
        public IHttpActionResult EditarTipoDeInvolucradoNombre(string nombre, EditarRolesPelicula tipoDeInvolucradoEditado, int? noReturn)
        {
            try
            {
                var tipoDeInvolucradoExistente = RolesPelicula.Rol.FirstOrDefault(r => r.Nombre == nombre);

                if (tipoDeInvolucradoExistente == null)
                {
                    return NotFound(); 
                }

                var tipoDeInvolucradoConNuevoNombre = RolesPelicula.Rol
                    .FirstOrDefault(r => r.Nombre == tipoDeInvolucradoEditado.Nombre && r.Nombre != nombre);

                if (tipoDeInvolucradoConNuevoNombre != null)
                {
                    return Conflict(); 
                }

                tipoDeInvolucradoExistente.Nombre = tipoDeInvolucradoEditado.Nombre;
                RolesPelicula.SaveChanges();

                if (noReturn == 1)
                {
                    return StatusCode(HttpStatusCode.NoContent); 
                }
                else
                {
                    return Ok(tipoDeInvolucradoExistente); 
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex); 
            }
        } 

        [HttpDelete]
        [Route("api/Roles/EliminarTipoDeInvolucrado/{id}")]
        public IHttpActionResult EliminarTipoDeInvolucrado(int id, int? noReturn)
        {
            var tipoDeInvolucradoExistente = RolesPelicula.Rol.Find(id);

            if (tipoDeInvolucradoExistente == null)
            {
                return NotFound(); 
            }

            try
            {
                RolesPelicula.Rol.Remove(tipoDeInvolucradoExistente);
                RolesPelicula.SaveChanges();
                
                if (noReturn == 1)
                {
                    return StatusCode(HttpStatusCode.NoContent); 
                }
                else
                {
                    
                    return Ok(tipoDeInvolucradoExistente);
                }
            }
            catch (Exception)
            {
                return Conflict();
            }
        } 

        [HttpGet]
        [Route("api/Roles/ObtenerTiposDeRol")]
        public IHttpActionResult ObtenerTiposDeRol(string nombre = null)
        {
            try
            {
                IQueryable<Rol> roles;

                if (!string.IsNullOrEmpty(nombre))
                {
                    roles = RolesPelicula.Rol.Where(r => r.Nombre.Contains(nombre));

                    
                    if (!roles.Any())
                    {
                        return NotFound();
                    }
                }
                else
                {
                    roles = RolesPelicula.Rol.AsQueryable();
                }

                var rolesDTO = roles
                    .Select(r => new
                    {
                        RolID = r.RolID,
                        Nombre = r.Nombre
                    })
                    .ToList();

                return Ok(rolesDTO);
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        } 
    }
}
