using System.Collections.Generic;
using GadgetsOnline.Models;
using Microsoft.Data.SqlClient;

namespace GadgetsOnline.Data
{
    /// <summary>
    /// Product data access via stored procedures.
    /// </summary>
    public class ProductRepository
    {
        private readonly Database _database;

        public ProductRepository(Database database)
        {
            _database = database;
        }

        public List<Product> GetBestSellers(int count)
        {
            var products = new List<Product>();

            using var connection = _database.CreateOpenConnection();
            using var command = Database.CreateStoredProcCommand("dbo.Products_GetBestSellers", connection);
            command.Parameters.AddWithValue("@Count", count);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                products.Add(MapProduct(reader));
            }

            return products;
        }

        public List<Product> GetByCategoryName(string categoryName)
        {
            var products = new List<Product>();

            using var connection = _database.CreateOpenConnection();
            using var command = Database.CreateStoredProcCommand("dbo.Products_GetByCategoryName", connection);
            command.Parameters.AddWithValue("@CategoryName", categoryName);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                products.Add(MapProduct(reader));
            }

            return products;
        }

        public Product GetById(int id)
        {
            using var connection = _database.CreateOpenConnection();
            using var command = Database.CreateStoredProcCommand("dbo.Products_GetById", connection);
            command.Parameters.AddWithValue("@ProductId", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var product = MapProduct(reader);
                // The proc also returns the joined category columns so we can
                // hydrate the navigation property the views rely on.
                product.Category = new Category
                {
                    CategoryId = reader.GetInt32(reader.GetOrdinal("Category_CategoryId")),
                    Name = reader.GetString(reader.GetOrdinal("Category_Name")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Category_Description"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Category_Description"))
                };
                return product;
            }

            return null;
        }

        public List<Product> GetFrequentlyBoughtTogether(int productId, int count)
        {
            var products = new List<Product>();

            using var connection = _database.CreateOpenConnection();
            using var command = Database.CreateStoredProcCommand("dbo.Products_GetFrequentlyBoughtTogether", connection);
            command.Parameters.AddWithValue("@ProductId", productId);
            command.Parameters.AddWithValue("@Count", count);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                products.Add(MapProduct(reader));
            }

            return products;
        }

        private static Product MapProduct(SqlDataReader reader)
        {
            return new Product
            {
                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                ProductArtUrl = reader.IsDBNull(reader.GetOrdinal("ProductArtUrl"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("ProductArtUrl"))
            };
        }
    }
}
