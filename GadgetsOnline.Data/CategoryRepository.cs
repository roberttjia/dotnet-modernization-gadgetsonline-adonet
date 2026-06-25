using System.Collections.Generic;
using GadgetsOnline.Models;
using Microsoft.Data.SqlClient;

namespace GadgetsOnline.Data
{
    /// <summary>
    /// Category data access via stored procedures.
    /// </summary>
    public class CategoryRepository
    {
        private readonly Database _database;

        public CategoryRepository(Database database)
        {
            _database = database;
        }

        public List<Category> GetAll()
        {
            var categories = new List<Category>();

            using var connection = _database.CreateOpenConnection();
            using var command = Database.CreateStoredProcCommand("dbo.Categories_GetAll", connection);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                categories.Add(new Category
                {
                    CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Description"))
                });
            }

            return categories;
        }
    }
}
