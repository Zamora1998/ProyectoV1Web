using DataClient;
using ProyectoV1.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Transactions;
using System.Web.Http;

namespace ProyectoV1.Controllers
{
    public class PeliculasFController : ApiController
    {
        private PeliculasProgra11Entities Pelis = new PeliculasProgra11Entities();

        public PeliculasFController()
        {
            Pelis.Configuration.LazyLoadingEnabled = true;
            Pelis.Configuration.ProxyCreationEnabled = true;
        }

        [HttpGet]
        [Route("api/Peliculas/GetPeliculaPorId/{id}")]
        public IHttpActionResult GetPeliculaPorId(int id)
        {
            var pelicula = Pelis.Peliculas
                .Where(p => p.PeliculaID == id)
                .ToList() // Cargar la película y los comentarios en memoria
                .Select(p => new PeliculaDto
                {
                    PeliculaID = p.PeliculaID,
                    Nombre = p.Nombre,
                    Resena = p.Resena,
                    CalificacionGenerQal = p.CalificacionGenerQal,
                    FechaLanzamiento = p.FechaLanzamiento,
                    PosterID = p.PosterID,
                    Involucrados = p.RolesEnPelicula.Select(r => new InvolucradoDto
                    {
                        ActorStaffID = (int)r.ActorStaffID,
                        Nombre = r.ActoresStaff.Nombre,
                        PaginaWeb = r.ActoresStaff.PaginaWeb,
                        Facebook = r.ActoresStaff.Facebook,
                        Instagram = r.ActoresStaff.Instagram,
                        Twitter = r.ActoresStaff.Twitter
                    }).ToList(),
                    Comentarios = ConstruirArbolComentarios(null, p.Comentarios.ToList()), // Cargar comentarios en memoria
                    Calificaciones = p.CalificacionesExpertos.Select(cal => new CalificacionDto
                    {
                        PeliculaID = cal.PeliculaID,
                        ExpertoID = cal.ExpertoID,
                        Calificacion = (decimal)cal.Calificacion
                    }).ToList()
                })
                .SingleOrDefault();
            if (pelicula == null)
            {
                return NotFound();
            }
            return Ok(pelicula);
        }

        [HttpGet]
        [Route("api/PeliculasF/GetPeliculasRecientes")]
        public IHttpActionResult GetPeliculasRecientes()
        {
            var peliculasRecientes = Pelis.Peliculas
                .OrderByDescending(p => p.FechaLanzamiento)
                .Take(5)
                .ToList() // Cargar las películas en memoria
                .Select(p => new PeliculaDto
                {
                    PeliculaID = p.PeliculaID,
                    Nombre = p.Nombre,
                    Resena = p.Resena,
                    CalificacionGenerQal = p.CalificacionGenerQal,
                    FechaLanzamiento = p.FechaLanzamiento,
                    PosterID = p.PosterID,
                    Involucrados = p.RolesEnPelicula.Select(r => new InvolucradoDto
                    {
                        ActorStaffID = (int)r.ActorStaffID,
                        Nombre = r.ActoresStaff.Nombre,
                        PaginaWeb = r.ActoresStaff.PaginaWeb,
                        Facebook = r.ActoresStaff.Facebook,
                        Instagram = r.ActoresStaff.Instagram,
                        Twitter = r.ActoresStaff.Twitter
                    }).ToList(),
                    Comentarios = ConstruirArbolComentarios(null, p.Comentarios.ToList()), // Cargar comentarios en memoria
                    Calificaciones = p.CalificacionesExpertos.Select(cal => new CalificacionDto
                    {
                        PeliculaID = cal.PeliculaID,
                        ExpertoID = cal.ExpertoID,
                        Calificacion = (decimal)cal.Calificacion
                    }).ToList()
                })
                .ToList();
            if (peliculasRecientes == null || peliculasRecientes.Count == 0)
            {
                return NotFound();
            }
            return Ok(peliculasRecientes);
        }

        private List<ComentarioDto> ConstruirArbolComentarios(int? comentarioPadreID, List<Comentarios> comentarios)
        {
            var comentariosArbol = new List<ComentarioDto>();

            var comentariosHijos = comentarios.Where(c => c.RespuestaAComentarioID == comentarioPadreID).ToList();
            foreach (var comentario in comentariosHijos)
            {
                var comentarioDto = new ComentarioDto
                {
                    ComentarioID = comentario.ComentarioID,
                    PeliculaID = comentario.PeliculaID.Value,
                    UsuarioID = comentario.UsuarioID.Value,
                    Comentario = comentario.Comentario,
                    FechaComentario = comentario.FechaComentario.Value,
                    RespuestaAComentarioID = comentario.RespuestaAComentarioID,
                    ComentariosHijos = ConstruirArbolComentarios(comentario.ComentarioID, comentarios)
                };
                comentariosArbol.Add(comentarioDto);
            }
            return comentariosArbol;
        }

        [HttpPost]
        [Route("api/PeliculasF/RegistrarPelicula")]
        public IHttpActionResult RegistrarPelicula(RegistrarPelicula peliculaNueva)
        {
            int nuevoPosterID = 0;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var posterExistente = Pelis.Posters.FirstOrDefault(p => p.NombreArchivo == peliculaNueva.NombreArchivo);
                if (posterExistente == null)
                {
                    // La película no existe, crear una nueva instancia
                    var nuevoPoster = new Posters
                    {
                        NombreArchivo = peliculaNueva.NombreArchivo,
                        RutaArchivo = peliculaNueva.RutaArchivo
                    };
                    Pelis.Posters.Add(nuevoPoster);
                    nuevoPosterID = nuevoPoster.PosterID;
                }
                else
                {
                    var response = Request.CreateResponse(HttpStatusCode.Conflict, "El poster ya existe en la base de datos.");
                    return ResponseMessage(response);
                }

                // Verificar si la película ya existe en la base de datos
                var peliculaExistente = Pelis.Peliculas.FirstOrDefault(p => p.Nombre == peliculaNueva.Nombre);
                if (peliculaExistente == null)
                {
                    // La película no existe, crear una nueva instancia
                    var nuevaPelicula = new Peliculas
                    {
                        Nombre = peliculaNueva.Nombre,
                        Resena = peliculaNueva.Resena,
                        CalificacionGenerQal = (int?)peliculaNueva.CalificacionGenerQal,
                        FechaLanzamiento = peliculaNueva.FechaLanzamiento,
                        PosterID = nuevoPosterID
                    };
                    // Agregar la nueva película a la base de datos
                    Pelis.Peliculas.Add(nuevaPelicula);
                }
                else
                {
                    var response = Request.CreateResponse(HttpStatusCode.Conflict, "La película ya existe en la base de datos.");
                    return ResponseMessage(response);
                }

                if (peliculaNueva.Involucrados != null)
                {
                    // Agregar involucrados a la película
                    foreach (var involucradoDTO in peliculaNueva.Involucrados)
                    {
                        // Verificar si el actor ya existe en la base de datos
                        var actorExistente = Pelis.ActoresStaff.FirstOrDefault(a => a.ActorStaffID == involucradoDTO.ActorStaffID);
                        if (actorExistente == null)
                        {
                            // El actor no existe, crear una nueva instancia
                            var involucrado = new ActoresStaff
                            {
                                Nombre = involucradoDTO.Nombre,
                                PaginaWeb = involucradoDTO.PaginaWeb,
                                Facebook = involucradoDTO.Facebook,
                                Instagram = involucradoDTO.Instagram,
                                Twitter = involucradoDTO.Twitter
                            };
                            Pelis.ActoresStaff.Add(involucrado);
                        }
                        else
                        {
                            // Verificar si el rol en la película ya existe en la base de datos
                            var rolExistente = Pelis.RolesEnPelicula.FirstOrDefault(r => r.RolID == involucradoDTO.RolID && r.ActorStaffID == involucradoDTO.ActorStaffID);
                            if (rolExistente == null)
                            {
                                // El rol en la película no existe, crear una nueva instancia
                                var rolEnPelicula = new RolesEnPelicula
                                {
                                    RolID = involucradoDTO.RolID,
                                    ActorStaffID = involucradoDTO.ActorStaffID,
                                    IDPelic = involucradoDTO.IDPelic,
                                    OrdenAparicion = involucradoDTO.OrdenAparicion
                                };
                                Pelis.RolesEnPelicula.Add(rolEnPelicula);
                            }
                            else
                            {
                                var response = Request.CreateResponse(HttpStatusCode.Conflict, "El actor ya esta agregado en esta pelicula");
                                return ResponseMessage(response);
                            }
                        }
                    }
                }
                else
                {
                    return BadRequest("La lista de involucrados está vacía o es nula.");
                }
                Pelis.SaveChanges();
                return Created(Request.RequestUri, peliculaNueva);
            }
            catch (Exception ex)
            {
                // Return a meaningful error response to the client
                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("api/PeliculasF/EditarPelicula")]
        public IHttpActionResult EditarPelicula(EditarPelicula datosEditados)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var peliculaExistente = Pelis.Peliculas.FirstOrDefault(p => p.PeliculaID == datosEditados.PeliculaID);
            if (peliculaExistente == null)
            {
                return NotFound(); // Devolver un código 404 si la película no se encuentra
            }

            peliculaExistente.Nombre = datosEditados.Nombre;
            peliculaExistente.Resena = datosEditados.Resena;
            peliculaExistente.CalificacionGenerQal = (int?)datosEditados.CalificacionGenerQal;
            peliculaExistente.FechaLanzamiento = datosEditados.FechaLanzamiento;
            peliculaExistente.PosterID = datosEditados.PosterID;
            Pelis.SaveChanges();
            return Ok(datosEditados);
        }

        [HttpDelete]
        [Route("api/PeliculasF/EliminarPeliculaPorCodigo")]
        public IHttpActionResult EliminarPeliculaPorCodigo(int peliculaID, int? returnData)
        {
            var peliculaAEliminar = Pelis.Peliculas.FirstOrDefault(p => p.PeliculaID == peliculaID);

            if (peliculaAEliminar == null)
            {
                return NotFound();
            }
            try
            {
                // Eliminar los datos relacionados en RolesEnPelicula
                var rolesRelacionados = Pelis.RolesEnPelicula.Where(r => r.IDPelic == peliculaAEliminar.PeliculaID).ToList();
                Pelis.RolesEnPelicula.RemoveRange(rolesRelacionados);
                // Eliminar la película de la base de datos
                Pelis.Peliculas.Remove(peliculaAEliminar);
                Pelis.SaveChanges();
                if (returnData == 1)
                {
                    return Ok(peliculaID);
                }
                else
                {
                    return StatusCode(HttpStatusCode.NoContent);
                }
            }
            catch (DbUpdateException ex)
            {
                return InternalServerError(ex); // Devolver un error interno del servidor junto con el mensaje de error
            }
        }
    }
}