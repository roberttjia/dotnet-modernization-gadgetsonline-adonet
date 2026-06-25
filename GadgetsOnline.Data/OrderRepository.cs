using System.Data;
using GadgetsOnline.Models;
using Microsoft.Data.SqlClient;

namespace GadgetsOnline.Data
{
    /// <summary>
    /// Order and order-detail data access via stored procedures.
    /// </summary>
    public class OrderRepository
    {
        private readonly Database _database;

        public OrderRepository(Database database)
        {
            _database = database;
        }

        /// <summary>
        /// Inserts the order and returns the generated OrderId.
        /// </summary>
        public int CreateOrder(Order order)
        {
            using var connection = _database.CreateOpenConnection();
            using var command = Database.CreateStoredProcCommand("dbo.Orders_Create", connection);
            command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
            command.Parameters.AddWithValue("@Username", (object)order.Username ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@FirstName", order.FirstName);
            command.Parameters.AddWithValue("@LastName", order.LastName);
            command.Parameters.AddWithValue("@Address", order.Address);
            command.Parameters.AddWithValue("@City", order.City);
            command.Parameters.AddWithValue("@State", order.State);
            command.Parameters.AddWithValue("@PostalCode", order.PostalCode);
            command.Parameters.AddWithValue("@Country", order.Country);
            command.Parameters.AddWithValue("@Phone", order.Phone);
            command.Parameters.AddWithValue("@Email", order.Email);
            command.Parameters.AddWithValue("@Total", order.Total);

            var orderIdParam = new SqlParameter("@OrderId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(orderIdParam);

            command.ExecuteNonQuery();

            int orderId = (int)orderIdParam.Value;
            order.OrderId = orderId;
            return orderId;
        }

        /// <summary>
        /// Places an order atomically from the given cart via a single
        /// transactional stored procedure: computes the total, inserts the
        /// order header and line items, and empties the cart. Returns the new
        /// OrderId and sets it on the supplied order.
        /// </summary>
        public int PlaceOrder(string cartId, Order order)
        {
            using var connection = _database.CreateOpenConnection();
            using var command = Database.CreateStoredProcCommand("dbo.Checkout_PlaceOrder", connection);
            command.Parameters.AddWithValue("@CartId", cartId);
            command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
            command.Parameters.AddWithValue("@Username", (object)order.Username ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@FirstName", order.FirstName);
            command.Parameters.AddWithValue("@LastName", order.LastName);
            command.Parameters.AddWithValue("@Address", order.Address);
            command.Parameters.AddWithValue("@City", order.City);
            command.Parameters.AddWithValue("@State", order.State);
            command.Parameters.AddWithValue("@PostalCode", order.PostalCode);
            command.Parameters.AddWithValue("@Country", order.Country);
            command.Parameters.AddWithValue("@Phone", order.Phone);
            command.Parameters.AddWithValue("@Email", order.Email);

            var orderIdParam = new SqlParameter("@OrderId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(orderIdParam);

            command.ExecuteNonQuery();

            int orderId = (int)orderIdParam.Value;
            order.OrderId = orderId;
            return orderId;
        }

        public void UpdateTotal(int orderId, decimal total)
        {
            using var connection = _database.CreateOpenConnection();
            using var command = Database.CreateStoredProcCommand("dbo.Orders_UpdateTotal", connection);
            command.Parameters.AddWithValue("@OrderId", orderId);
            command.Parameters.AddWithValue("@Total", total);
            command.ExecuteNonQuery();
        }

        public void AddOrderDetail(OrderDetail detail)
        {
            using var connection = _database.CreateOpenConnection();
            using var command = Database.CreateStoredProcCommand("dbo.OrderDetails_Add", connection);
            command.Parameters.AddWithValue("@OrderId", detail.OrderId);
            command.Parameters.AddWithValue("@ProductId", detail.ProductId);
            command.Parameters.AddWithValue("@Quantity", detail.Quantity);
            command.Parameters.AddWithValue("@UnitPrice", detail.UnitPrice);
            command.ExecuteNonQuery();
        }
    }
}
