using System.Collections.Generic;
using System.Data;
using GadgetsOnline.Models;
using Microsoft.Data.SqlClient;

namespace GadgetsOnline.Data
{
    /// <summary>
    /// Shopping cart data access via stored procedures.
    /// </summary>
    public class CartRepository
    {
        private readonly Database _database;

        public CartRepository(Database database)
        {
            _database = database;
        }

        public List<Cart> GetCartItems(string cartId)
        {
            var items = new List<Cart>();

            using var connection = _database.CreateOpenConnection();
            using var command = Database.CreateStoredProcCommand("dbo.Carts_GetByCartId", connection);
            command.Parameters.AddWithValue("@CartId", cartId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var cart = new Cart
                {
                    RecordId = reader.GetInt32(reader.GetOrdinal("RecordId")),
                    CartId = reader.GetString(reader.GetOrdinal("CartId")),
                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                    Count = reader.GetInt32(reader.GetOrdinal("Count")),
                    DateCreated = reader.GetDateTime(reader.GetOrdinal("DateCreated")),
                    // Hydrate Cart.Product (used by the cart view).
                    Product = new Product
                    {
                        ProductId = reader.GetInt32(reader.GetOrdinal("Product_ProductId")),
                        CategoryId = reader.GetInt32(reader.GetOrdinal("Product_CategoryId")),
                        Name = reader.GetString(reader.GetOrdinal("Product_Name")),
                        Price = reader.GetDecimal(reader.GetOrdinal("Product_Price")),
                        ProductArtUrl = reader.IsDBNull(reader.GetOrdinal("Product_ProductArtUrl"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("Product_ProductArtUrl"))
                    }
                };
                items.Add(cart);
            }

            return items;
        }

        public void AddToCart(string cartId, int productId)
        {
            using var connection = _database.CreateOpenConnection();
            using var command = Database.CreateStoredProcCommand("dbo.Carts_AddItem", connection);
            command.Parameters.AddWithValue("@CartId", cartId);
            command.Parameters.AddWithValue("@ProductId", productId);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Decrements an item's quantity (or removes it) and returns the
        /// remaining count for that item.
        /// </summary>
        public int RemoveFromCart(string cartId, int productId)
        {
            using var connection = _database.CreateOpenConnection();
            using var command = Database.CreateStoredProcCommand("dbo.Carts_RemoveItem", connection);
            command.Parameters.AddWithValue("@CartId", cartId);
            command.Parameters.AddWithValue("@ProductId", productId);

            var remaining = new SqlParameter("@RemainingCount", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(remaining);

            command.ExecuteNonQuery();

            return remaining.Value == System.DBNull.Value ? 0 : (int)remaining.Value;
        }

        public int GetCount(string cartId)
        {
            using var connection = _database.CreateOpenConnection();
            using var command = Database.CreateStoredProcCommand("dbo.Carts_GetCount", connection);
            command.Parameters.AddWithValue("@CartId", cartId);

            var result = command.ExecuteScalar();
            return result == null || result == System.DBNull.Value ? 0 : (int)result;
        }

        public decimal GetTotal(string cartId)
        {
            using var connection = _database.CreateOpenConnection();
            using var command = Database.CreateStoredProcCommand("dbo.Carts_GetTotal", connection);
            command.Parameters.AddWithValue("@CartId", cartId);

            var result = command.ExecuteScalar();
            return result == null || result == System.DBNull.Value ? decimal.Zero : (decimal)result;
        }

        public void EmptyCart(string cartId)
        {
            using var connection = _database.CreateOpenConnection();
            using var command = Database.CreateStoredProcCommand("dbo.Carts_EmptyCart", connection);
            command.Parameters.AddWithValue("@CartId", cartId);
            command.ExecuteNonQuery();
        }
    }
}
