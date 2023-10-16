using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace SistemaGestion
{
    public class VentaController : ApiController
    {
        private string connectionString = "Martinbaigorria(aca iria mi conexion a la base)"; 
        public IEnumerable<Venta> GetVentas()
        {
            List<Venta> ventas = new List<Venta>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Ventas";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Venta venta = new Venta
                        {
                            Id = (int)reader["Id"],
                        };
                        ventas.Add(venta);
                    }
                }
            }

            return ventas;
        }

        public Venta GetVenta(int id)
        {
            Venta venta = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Ventas WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            venta = new Venta
                            {
                                Id = (int)reader["Id"],
                            };
                        }
                    }
                }
            }

            return venta;
        }

        public HttpResponseMessage PostVenta(Venta venta)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Ventas (Fecha, Monto, ...) VALUES (@Fecha, @Monto, ...)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Fecha", venta.Fecha);
                    command.Parameters.AddWithValue("@Monto", venta.Monto);
                    command.ExecuteNonQuery();
                }
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        public HttpResponseMessage PutVenta(int id, Venta venta)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Ventas SET Fecha = @Fecha, Monto = @Monto, ... WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Fecha", venta.Fecha);
                    command.Parameters.AddWithValue("@Monto", venta.Monto);
                    command.ExecuteNonQuery();
                }
            }

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        public HttpResponseMessage DeleteVenta(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Ventas WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        public void CargarVenta(List<ProductoVendido> productosVendidos, int idUsuario)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string insertVentaQuery = "INSERT INTO Ventas (IdUsuario, Fecha) VALUES (@IdUsuario, @Fecha); SELECT SCOPE_IDENTITY();";
                decimal ventaId;
                using (SqlCommand command = new SqlCommand(insertVentaQuery, connection))
                {
                    command.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    command.Parameters.AddWithValue("@Fecha", DateTime.Now);
                    ventaId = Convert.ToDecimal(command.ExecuteScalar());
                }

                foreach (var productoVendido in productosVendidos)
                {
                    string insertProductoVendidoQuery = "INSERT INTO ProductosVendidos (VentaId, ProductoId, Cantidad) VALUES (@VentaId, @ProductoId, @Cantidad)";
                    using (SqlCommand command = new SqlCommand(insertProductoVendidoQuery, connection))
                    {
                        command.Parameters.AddWithValue("@VentaId", ventaId);
                        command.Parameters.AddWithValue("@ProductoId", productoVendido.ProductoId);
                        command.Parameters.AddWithValue("@Cantidad", productoVendido.Cantidad);
                        command.ExecuteNonQuery();
                    }

                    string updateStockQuery = "UPDATE Productos SET Stock = Stock - @Cantidad WHERE Id = @ProductoId";
                    using (SqlCommand command = new SqlCommand(updateStockQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ProductoId", productoVendido.ProductoId);
                        command.Parameters.AddWithValue("@Cantidad", productoVendido.Cantidad);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

    }

}
