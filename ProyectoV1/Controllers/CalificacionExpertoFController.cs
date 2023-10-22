using DataClient;
using ProyectoV1.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ProyectoV1.Controllers
{
    public class CalificacionExpertoFController : ApiController
    {
        private PeliculasProgra11Entities CaliE = new PeliculasProgra11Entities();

        public CalificacionExpertoFController()
        {
            CaliE.Configuration.LazyLoadingEnabled = true;
            CaliE.Configuration.ProxyCreationEnabled = true;
        }

        [HttpPost]
        [Route("api/Calificaciones/AgregarCalificacionExperto")]
        public IHttpActionResult AgregarCalificacionExperto(AgregarCalificacionExperto calificacion)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Verificar si la película existe
                var peliculaExistente = CaliE.Peliculas.FirstOrDefault(p => p.PeliculaID == calificacion.PeliculaID);
                if (peliculaExistente == null)
                {
                    return (IHttpActionResult)Request.CreateResponse(HttpStatusCode.NotFound, "La pelicula especificada no existe.");
                }

                // Verificar si el experto existe
                var expertoExistente = CaliE.ExpertosEnCalificaciones.FirstOrDefault(e => e.ExpertoID == calificacion.ExpertoID);
                if (expertoExistente == null)
                {
                    return (IHttpActionResult)Request.CreateResponse(HttpStatusCode.NotFound, "El experto especificado no existe.");
                }

                // Crear una nueva instancia de CalificacionesExpertos utilizando los datos proporcionados
                var nuevaCalificacion = new CalificacionesExpertos
                {
                    PeliculaID = calificacion.PeliculaID,
                    ExpertoID = calificacion.ExpertoID,
                    Calificacion = calificacion.Calificacion
                };

                // Agregar la calificación a la base de datos
                CaliE.CalificacionesExpertos.Add(nuevaCalificacion);
                CaliE.SaveChanges();

                return Created(Request.RequestUri, calificacion);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("api/Calificaciones/EditarCalificacionExperto")]
        public IHttpActionResult EditarCalificacionExperto(AgregarCalificacionExperto datosEditados)
        {
            // Buscar la calificación de experto por su PeliculaID y ExpertoID
            var calificacionExistente = CaliE.CalificacionesExpertos.FirstOrDefault(c => c.PeliculaID == datosEditados.PeliculaID && c.ExpertoID == datosEditados.ExpertoID);

            // Verificar si la calificación de experto existe
            if (calificacionExistente == null)
            {
                return NotFound(); // Devolver un código 404 si la calificación de experto no se encuentra
            } 
            // Actualizar el campo de calificación con el nuevo valor
            calificacionExistente.Calificacion = datosEditados.Calificacion; 
            // Guardar los cambios en la base de datos
            CaliE.SaveChanges(); 
            return Ok(datosEditados);
        }

        [HttpDelete]
        [Route("api/Calificaciones/EliminarCalificacionExperto")]
        public IHttpActionResult EliminarCalificacionExperto(int peliculaID, int expertoID, int? returnData)
        {
            var calificacionAEliminar = CaliE.CalificacionesExpertos.FirstOrDefault(c => c.PeliculaID == peliculaID && c.ExpertoID == expertoID);

            if (calificacionAEliminar == null)
            {
                return NotFound();
            }

            // Eliminar la calificación de experto de la base de datos
            CaliE.CalificacionesExpertos.Remove(calificacionAEliminar);
            CaliE.SaveChanges();
            if (returnData == 1)
            { 
                return Ok(expertoID);
            }
            else
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
        }
    }
}
