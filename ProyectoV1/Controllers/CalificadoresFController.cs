using DataClient;
using ProyectoV1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace ProyectoV1.Controllers
{
    public class CalificadoresFController : ApiController
    {
        private PeliculasProgra11Entities Califi = new PeliculasProgra11Entities();

        public CalificadoresFController()
        {
            Califi.Configuration.LazyLoadingEnabled = true;
            Califi.Configuration.ProxyCreationEnabled = true;
        } 

        [HttpPost]
        [Route("api/Calificadores/RegistrarCalificador")]
        public IHttpActionResult RegistrarCalificador(RegistrarCalificador calificadorNuevo)
        {
            try
            {
                if (Califi.ExpertosEnCalificaciones.Any(u => u.codigoexperto == calificadorNuevo.codigoexperto))
                {
                    return Conflict(); // Devuelve código 409 (Conflict) si el calificador ya existe
                }

                var nuevoCalificador = new ExpertosEnCalificaciones
                {
                    Nombre = calificadorNuevo.Nombre,
                    codigoexperto = calificadorNuevo.codigoexperto
                };

                Califi.ExpertosEnCalificaciones.Add(nuevoCalificador);
                Califi.SaveChanges();

                var requestUri = Request.RequestUri;
                var newResourceUrl = new Uri(requestUri, $"api/Calificadores/RegistrarCalificador/{nuevoCalificador.codigoexperto}");

                // Devuelve código 201 (Created) junto con el objeto en JSON
                return Content(HttpStatusCode.Created, nuevoCalificador, new JsonMediaTypeFormatter(), "application/json");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex); // Devuelve código 500 (Internal Server Error) en caso de error
            }
        } 

        [HttpPut]
        [Route("api/Calificadores/EditarCalificador/{id}")]
        public IHttpActionResult EditarCalificador(int id, RegistrarCalificador calificadorEditado, int? noReturn)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var calificadorExistente = Califi.ExpertosEnCalificaciones.FirstOrDefault(c => c.ExpertoID == id);

                if (calificadorExistente == null)
                {
                    return NotFound();
                }
                calificadorExistente.Nombre = calificadorEditado.Nombre;
                calificadorExistente.codigoexperto = calificadorEditado.codigoexperto;
                Califi.SaveChanges();

                if (noReturn == 1)
                {
                    return StatusCode(HttpStatusCode.NoContent); 
                }
                else
                {
                    return Ok(calificadorExistente); 
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        } 

        [HttpPut]
        [Route("api/Calificadores/EditarCalificadorPorCodigo")]
        public IHttpActionResult EditarCalificadorPorCodigo(string codigoexperto, [FromBody] RegistrarCalificador calificadorEditado, int? noReturn)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var calificadorExistente = Califi.ExpertosEnCalificaciones.FirstOrDefault(c => c.codigoexperto == codigoexperto);

                if (calificadorExistente == null)
                {
                    return NotFound();
                }
                calificadorExistente.Nombre = calificadorEditado.Nombre;
                calificadorExistente.codigoexperto = calificadorEditado.codigoexperto;
                Califi.SaveChanges();

                if (noReturn == 1)
                {
                    return StatusCode(HttpStatusCode.NoContent);
                }
                else
                {
                    return Ok(calificadorEditado); 
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        } 

        [HttpDelete]
        [Route("api/Calificadores/EliminarCalificadorPorCodigo")]
        public IHttpActionResult EliminarCalificadorPorCodigo(string codigoexperto)
        {
            var calificadorExistente = Califi.ExpertosEnCalificaciones.FirstOrDefault(c => c.codigoexperto == codigoexperto);

            if (calificadorExistente == null)
            {
                return NotFound();
            }
            try
            {
                Califi.ExpertosEnCalificaciones.Remove(calificadorExistente);
                Califi.SaveChanges();

                return Ok("Calificador eliminado con éxito.");
            }
            catch (Exception)
            {
                return Conflict();
            }
        }

        [HttpGet]
        [Route("api/Calificadores/ObtenerCalificadores")]
        public IHttpActionResult ObtenerCalificadores(string nombre = null)
        {
            try
            {
                var query = Califi.ExpertosEnCalificaciones.AsQueryable();

                if (!string.IsNullOrEmpty(nombre))
                {
                    query = query.Where(c => c.Nombre.Contains(nombre));
                }

                var calificadores = query.Select(c => new
                {
                    c.ExpertoID,
                    c.Nombre,
                    c.codigoexperto
                }).ToList();

                if (calificadores.Count == 0 && !string.IsNullOrEmpty(nombre))
                {
                    return NotFound();
                }

                return Ok(calificadores);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        } 
    }
}
