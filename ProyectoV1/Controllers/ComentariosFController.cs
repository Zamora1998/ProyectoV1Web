using DataClient;
using ProyectoV1.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ProyectoV1.Controllers
{
    public class ComentariosFController : ApiController
    {
        private PeliculasProgra11Entities Coment = new PeliculasProgra11Entities();

        public ComentariosFController()
        {
            Coment.Configuration.LazyLoadingEnabled = true;
            Coment.Configuration.ProxyCreationEnabled = true;
        }

        [HttpPost]
        [Route("api/Comentarios/AgregarComentario")]
        public IHttpActionResult AgregarComentario(AgregarComentario comentario)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Verificar si el usuario existe
                var usuarioExistente = Coment.Usuarios.FirstOrDefault(u => u.UsuarioID == comentario.UsuarioID);
                if (usuarioExistente == null)
                {
                    return (IHttpActionResult)Request.CreateResponse(HttpStatusCode.NotFound, "El usuario especificado no existe.");
                }

                if (comentario.RespuestaAComentarioID == 0)
                {
                    comentario.RespuestaAComentarioID = null;
                }
                else 
                {
                    var comentarioExistente = Coment.Comentarios.FirstOrDefault(c => c.ComentarioID == comentario.RespuestaAComentarioID);
                    if (comentarioExistente == null)
                    {
                        return (IHttpActionResult)Request.CreateResponse(HttpStatusCode.NotFound, "Esta intentando responder a un comentario que no existe.");
                    }
                } 

                // Crear una nueva instancia de Comentario utilizando los datos proporcionados
                var nuevoComentario = new Comentarios
                {
                    PeliculaID = comentario.PeliculaID,
                    UsuarioID = comentario.UsuarioID,
                    Comentario = comentario.Comentario,
                    FechaComentario = comentario.FechaComentario,
                    RespuestaAComentarioID = comentario.RespuestaAComentarioID
                };
                // Agregar el comentario a la base de datos
                Coment.Comentarios.Add(nuevoComentario);
                Coment.SaveChanges();
                return Created(Request.RequestUri, comentario);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpDelete]
        [Route("api/Comentarios/EliminarComentario")]
        public IHttpActionResult EliminarComentario(int comentarioID, int? returnData)
        {
            var comentarioAEliminar = Coment.Comentarios.FirstOrDefault(c => c.ComentarioID == comentarioID);

            if (comentarioAEliminar == null)
            {
                return NotFound();
            }
            // Eliminar el comentario de la base de datos
            Coment.Comentarios.Remove(comentarioAEliminar);
            Coment.SaveChanges();
            if (returnData == 1)
            { 
                return Ok(comentarioID);
            }
            else
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
        }

        [HttpPut]
        [Route("api/Comentarios/EditarComentario")]
        public IHttpActionResult EditarComentario(EditarComentario datosEditados)
        {
            // Buscar el comentario por su identificador
            var comentarioExistente = Coment.Comentarios.FirstOrDefault(c => c.ComentarioID == datosEditados.ComentarioID);

            // Verificar si el comentario NO existe
            if (comentarioExistente == null)
            {
                return NotFound(); // Devolver un código 404 si el comentario no se encuentra
            }

            // Actualizar el campo de comentario con el nuevo valor
            comentarioExistente.PeliculaID = datosEditados.PeliculaID;
            comentarioExistente.Comentario = datosEditados.Comentario;
            comentarioExistente.FechaComentario = datosEditados.FechaComentario;
             
            // Guardar los cambios en la base de datos
            Coment.SaveChanges(); 
            return Ok(datosEditados);
        } 
    }
}