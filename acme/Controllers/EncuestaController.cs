using acme.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Text;

namespace acme.Controllers
{
    [ApiController]
    [Route("acme/[controller]")]
    public class EncuestaController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        public EncuestaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpPost]
        [Authorize]
        public IActionResult AgregarEncuesta([FromBody] Encuesta encuesta)
        {
            string sqlDataSource = _configuration.GetConnectionString("conexiondb");

            using (SqlConnection conexion = new SqlConnection(sqlDataSource))
            {
                string idEncuesta = "";
                SqlCommand cmd = new SqlCommand("agregarEncuesta", conexion);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nombre", encuesta.nombre);
                cmd.Parameters.AddWithValue("@descripcion", encuesta.descripcion);
                cmd.Parameters.AddWithValue("@link", "");
                conexion.Open();

                try
                {
                    using (SqlDataReader sqlDR = cmd.ExecuteReader())
                    {
                        // Insertamos la encuesta y obtenemos el identificador
                        while (sqlDR.Read())
                        {
                            idEncuesta = sqlDR["id_encuesta"].ToString();
                        }
                        conexion.Close();
                    }
                    
                    // Actualizamos la encuesta con el link para acceder a la encuesta
                    SqlCommand sqlCommandUpdate = new SqlCommand("modificarEncuesta", conexion);
                    sqlCommandUpdate.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommandUpdate.Parameters.AddWithValue("@id_encuesta", idEncuesta);
                    sqlCommandUpdate.Parameters.AddWithValue("@nombre", encuesta.nombre);
                    sqlCommandUpdate.Parameters.AddWithValue("@descripcion", encuesta.descripcion);
                    sqlCommandUpdate.Parameters.AddWithValue("@link", $"https://localhost:7135/acme/Encuesta?id_encuesta={idEncuesta}");
                    conexion.Open();
                    sqlCommandUpdate.ExecuteNonQuery();
                    conexion.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    conexion.Close();
                    return StatusCode(500, $"Ocurrió un error en el servidor!");
                }

                List<Campo> listaCampos = new List<Campo>();
                listaCampos = encuesta.campos;

                foreach (var campo in listaCampos)
                {
                    SqlCommand newCMD = new SqlCommand("agregarCampo", conexion);
                    newCMD.CommandType = System.Data.CommandType.StoredProcedure;
                    newCMD.Parameters.AddWithValue("@nombre", campo.nombre);
                    newCMD.Parameters.AddWithValue("@titulo", campo.titulo);
                    newCMD.Parameters.AddWithValue("@requerido", campo.requerido);
                    newCMD.Parameters.AddWithValue("@tipo", campo.tipo);
                    newCMD.Parameters.AddWithValue("@id_encuesta", idEncuesta);
                    conexion.Open();
                    newCMD.ExecuteNonQuery();
                    conexion.Close();
                }
                return Ok($"Encuesta agreada! Guarde el siguiente link https://localhost:7135/acme/Encuesta?id_encuesta={idEncuesta}");
            }
        }

        
        [HttpGet]
        [Authorize]
        public string ObtenerEncuesta([FromQuery(Name = "id_encuesta")] string id_encuesta)
        {
            string sqlDataSource = _configuration.GetConnectionString("conexiondb");
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<!DOCTYPE html>");
            stringBuilder.AppendLine("<html>\n<body>");
            stringBuilder.AppendLine("<form action=\"https://localhost:7135/acme/Encuesta/AgregarRespuesta?id_encuesta" + id_encuesta +"\" method =\"POST\" > ");

            using (SqlConnection conexion = new SqlConnection(sqlDataSource))
            {
                // Buscar la encuesta
                SqlCommand sqlCommandSearch = new SqlCommand("buscarEncuesta", conexion);
                sqlCommandSearch.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommandSearch.Parameters.AddWithValue("@id_encuesta", id_encuesta);

                conexion.Open();
                string existe = "";
                using (SqlDataReader sqlDR = sqlCommandSearch.ExecuteReader())
                {
                    // Insertamos la encuesta y obtenemos el identificador
                    while (sqlDR.Read())
                    {
                        existe = sqlDR["id_encuesta"].ToString();
                    }
                }
                conexion.Close();

                // Si no existe retornarmos error
                if (existe == "")
                {
                    return "La encuesta no existe!";
                }

                // Si existe devolvemos el formulario
                SqlCommand cmd = new SqlCommand("obtenerEncuesta", conexion);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_encuesta", id_encuesta);
                try
                {
                    conexion.Open();
                    using (SqlDataReader sqlDR = cmd.ExecuteReader())
                    {
                        // Insertamos la encuesta y obtenemos el identificador
                        while (sqlDR.Read())
                        {

                            Campo temp = new Campo();
                            {
                                stringBuilder.AppendLine("<label for=\"" + sqlDR["nombre"].ToString() + "\">" + sqlDR["titulo"].ToString() + ":" + "</label><br>");
                                stringBuilder.AppendLine("<input type=\"" + sqlDR["tipo"].ToString() + "\" id =\"" + sqlDR["nombre"].ToString() + "\" name =\"" + sqlDR["nombre"].ToString() + "\"><br>");

                            };
                        }
                        stringBuilder.AppendLine("<input type=\"submit\" value =\"Guardar\" > ");
                        stringBuilder.AppendLine("</form> ");
                        stringBuilder.AppendLine("</body>\n</html>");
                    }
                    conexion.Close();
                    string resultado = stringBuilder.ToString();
                    return resultado;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    conexion.Close();
                    return $"Ocurrió un error en el servidor!";
                }
            }
        }

        [HttpPut]
        [Authorize]
        public IActionResult ActualizarEncuesta([FromQuery(Name = "id_encuesta")] string id_encuesta, [FromBody] Encuesta encuesta)
        {
            string sqlDataSource = _configuration.GetConnectionString("conexiondb");

            using (SqlConnection conexion = new SqlConnection(sqlDataSource))
            {
                // Buscar la encuesta
                SqlCommand sqlCommandSearch = new SqlCommand("buscarEncuesta", conexion);
                sqlCommandSearch.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommandSearch.Parameters.AddWithValue("@id_encuesta", id_encuesta);

                conexion.Open();
                string existe = "";
                using (SqlDataReader sqlDR = sqlCommandSearch.ExecuteReader())
                {
                    // Insertamos la encuesta y obtenemos el identificador
                    while (sqlDR.Read())
                    {
                        existe = sqlDR["id_encuesta"].ToString();
                    }
                }
                conexion.Close();

                // Si no existe retornarmos error
                if (existe == "")
                {
                    return StatusCode(404, $"La encuesta no existe!");
                }

                try
                {                   
                    // Actualizamos la encuesta con el link para acceder a la encuesta
                    SqlCommand sqlCommandUpdate = new SqlCommand("modificarEncuesta", conexion);
                    sqlCommandUpdate.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommandUpdate.Parameters.AddWithValue("@id_encuesta", id_encuesta);
                    sqlCommandUpdate.Parameters.AddWithValue("@nombre", encuesta.nombre);
                    sqlCommandUpdate.Parameters.AddWithValue("@descripcion", encuesta.descripcion);
                    sqlCommandUpdate.Parameters.AddWithValue("@link", $"https://localhost:7135/acme/Encuesta?id_encuesta={id_encuesta}");
                    conexion.Open();
                    sqlCommandUpdate.ExecuteNonQuery();
                    conexion.Close();

                    List<Campo> listaCampos = new List<Campo>();
                    listaCampos = encuesta.campos;

                    foreach (var campo in listaCampos)
                    {
                        SqlCommand newCMD = new SqlCommand("agregarCampo", conexion);
                        newCMD.CommandType = System.Data.CommandType.StoredProcedure;
                        newCMD.Parameters.AddWithValue("@nombre", campo.nombre);
                        newCMD.Parameters.AddWithValue("@titulo", campo.titulo);
                        newCMD.Parameters.AddWithValue("@requerido", campo.requerido);
                        newCMD.Parameters.AddWithValue("@tipo", campo.tipo);
                        newCMD.Parameters.AddWithValue("@id_encuesta", id_encuesta);
                        conexion.Open();
                        newCMD.ExecuteNonQuery();
                        conexion.Close();
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    conexion.Close();
                    return StatusCode(500, $"Ocurrió un error en el servidor!");
                }

                return Ok($"Encuesta actualizada!");
            }
        }

        [HttpDelete]
        [Authorize]
        public IActionResult EliminarEncuesta([FromQuery(Name = "id_encuesta")] string id_encuesta)
        {
            string sqlDataSource = _configuration.GetConnectionString("conexiondb");

            using (SqlConnection conexion = new SqlConnection(sqlDataSource))
            {
                // Buscar la encuesta
                SqlCommand sqlCommandSearch = new SqlCommand("buscarEncuesta", conexion);
                sqlCommandSearch.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommandSearch.Parameters.AddWithValue("@id_encuesta", id_encuesta);

                conexion.Open();
                string existe = "";
                using (SqlDataReader sqlDR = sqlCommandSearch.ExecuteReader())
                {
                    // Insertamos la encuesta y obtenemos el identificador
                    while (sqlDR.Read())
                    {
                        existe = sqlDR["id_encuesta"].ToString();
                    }
                }
                conexion.Close();

                // Si no existe retornarmos error
                if (existe == "")
                {
                    return StatusCode(404, $"La encuesta no existe!");
                }

                // Si existe se elimina
                try
                {
                    SqlCommand cmd = new SqlCommand("eliminarEncuesta", conexion);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_encuesta", id_encuesta);
                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    conexion.Close();

                    return Ok($"Encuesta eliminada!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    conexion.Close();
                    return StatusCode(500, $"Ocurrió un error en el servidor!");
                }

            }
        }


        [HttpPost]
        [Route("AgregarRespuesta")]
        [AllowAnonymous]
        public IActionResult AgregarRespuesta([FromQuery(Name = "id_encuesta")] string id_encuesta, [FromBody] List<Respuesta> listaRespuestas)
        {
            string sqlDataSource = _configuration.GetConnectionString("conexiondb");

            using (SqlConnection conexion = new SqlConnection(sqlDataSource))
            {
                try
                {
                    foreach (var respuesta in listaRespuestas)
                    {
                        // Buscar la encuesta
                        SqlCommand sqlCommandSearch = new SqlCommand("obtenerIDCampo", conexion);
                        sqlCommandSearch.CommandType = System.Data.CommandType.StoredProcedure;
                        sqlCommandSearch.Parameters.AddWithValue("@id_encuesta", id_encuesta);
                        sqlCommandSearch.Parameters.AddWithValue("@nombre", respuesta.nombre);
                        string id = "";
                        conexion.Open();
                        using (SqlDataReader sqlDR = sqlCommandSearch.ExecuteReader())
                        {
                            // Insertamos la encuesta y obtenemos el identificador
                            while (sqlDR.Read())
                            {
                                id = sqlDR["id_campo"].ToString();
                            }
                        }

                        SqlCommand cmd = new SqlCommand("agregarRespuesta", conexion);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@respuesta", respuesta.respuesta);
                        cmd.Parameters.AddWithValue("@id_campo", id);

                        
                        cmd.ExecuteNonQuery();
                        conexion.Close();
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    conexion.Close();
                    return StatusCode(500, $"Ocurrió un error en el servidor!");
                }

                return Ok($"Respuesta agreada!");
            }
        }

        [HttpGet]
        [Route("ObtenerRespuestas")]
        [Authorize]
        public List<Respuesta> ObtenerRespuestas([FromQuery(Name = "id_encuesta")] string id_encuesta)
        {
            string sqlDataSource = _configuration.GetConnectionString("conexiondb");
            List<Respuesta> listaRespuestas = new List<Respuesta>();
            using (SqlConnection conexion = new SqlConnection(sqlDataSource))
            {
                // Buscar la encuesta
                SqlCommand sqlCommandSearch = new SqlCommand("buscarEncuesta", conexion);
                sqlCommandSearch.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommandSearch.Parameters.AddWithValue("@id_encuesta", id_encuesta);

                conexion.Open();
                string existe = "";
                using (SqlDataReader sqlDR = sqlCommandSearch.ExecuteReader())
                {
                    // Insertamos la encuesta y obtenemos el identificador
                    while (sqlDR.Read())
                    {
                        existe = sqlDR["id_encuesta"].ToString();
                    }
                }
                conexion.Close();

                // Si no existe retornarmos error
                if (existe == "")
                {
                    return listaRespuestas;
                }

                // Si existe devolvemos el formulario
                SqlCommand cmd = new SqlCommand("obtenerRespuestas", conexion);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_encuesta", id_encuesta);
                try
                {
                    conexion.Open();
                    using (SqlDataReader sqlDR = cmd.ExecuteReader())
                    {
                        // Guardamos las respuestas en una lista
                        while (sqlDR.Read())
                        {

                            Respuesta r = new Respuesta();
                            {                                
                                r.respuesta = sqlDR["respuesta"].ToString();
                                r.nombre = sqlDR["nombre"].ToString();
                                listaRespuestas.Add(r);
                            };
                        }

                    }
                    conexion.Close();
                    return listaRespuestas;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    conexion.Close();
                    return listaRespuestas;
                }
            }
        }

    }


}
