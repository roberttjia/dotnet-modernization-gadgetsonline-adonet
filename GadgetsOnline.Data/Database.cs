using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace GadgetsOnline.Data
{
    /// <summary>
    /// Central ADO.NET helper. Owns the connection string, hands out open
    /// connections, and bootstraps the database from the SQL scripts in the
    /// Database folder (schema -> stored procedures -> seed).
    /// </summary>
    public class Database
    {
        private readonly string _connectionString;

        public Database(string connectionString)
        {
            _connectionString = connectionString
                ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public string ConnectionString => _connectionString;

        /// <summary>
        /// Combines a base connection string (which holds non-secret parts such
        /// as Server, Initial Catalog and User ID) with a password supplied
        /// separately (e.g. from user secrets or an environment variable). This
        /// keeps the password out of appsettings.json while leaving the database
        /// name and other details visible there. If the base string already
        /// contains a password, the supplied one (when non-empty) overrides it.
        /// </summary>
        public static string BuildConnectionString(string baseConnectionString, string password)
        {
            var builder = new SqlConnectionStringBuilder(baseConnectionString);
            if (!string.IsNullOrEmpty(password))
            {
                builder.Password = password;
            }
            return builder.ConnectionString;
        }

        /// <summary>
        /// Opens a new SqlConnection. Callers are responsible for disposing it
        /// (typically via a using statement).
        /// </summary>
        public SqlConnection CreateOpenConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Creates a command bound to the given connection for a stored procedure.
        /// </summary>
        public static SqlCommand CreateStoredProcCommand(string procName, SqlConnection connection)
        {
            var command = new SqlCommand(procName, connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            return command;
        }

        /// <summary>
        /// Ensures the target database exists, then runs schema, stored
        /// procedure, and seed scripts. Safe to run on every startup because
        /// each script is written to be idempotent.
        /// </summary>
        public void Initialize(string scriptsDirectory)
        {
            EnsureDatabaseExists();

            RunScriptFile(Path.Combine(scriptsDirectory, "schema.sql"));
            RunScriptFile(Path.Combine(scriptsDirectory, "stored_procedures.sql"));
            RunScriptFile(Path.Combine(scriptsDirectory, "seed.sql"));
        }

        /// <summary>
        /// Connects to the server's master database and creates the target
        /// catalog if it is not already present.
        /// </summary>
        private void EnsureDatabaseExists()
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            string targetDatabase = builder.InitialCatalog;

            if (string.IsNullOrWhiteSpace(targetDatabase))
            {
                // Nothing to create; assume the connection already targets a DB.
                return;
            }

            builder.InitialCatalog = "master";

            using var connection = new SqlConnection(builder.ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText =
                "IF DB_ID(@dbName) IS NULL EXEC('CREATE DATABASE [' + @dbName + ']');";
            command.Parameters.AddWithValue("@dbName", targetDatabase);
            command.ExecuteNonQuery();
        }

        private void RunScriptFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    $"Required SQL script was not found: {path}");
            }

            string script = File.ReadAllText(path);

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            foreach (var batch in SplitSqlBatches(script))
            {
                using var command = connection.CreateCommand();
                command.CommandText = batch;
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Splits a T-SQL script into batches on lines containing only "GO",
        /// which is a SQLCMD batch separator and not valid T-SQL on its own.
        /// </summary>
        private static IEnumerable<string> SplitSqlBatches(string script)
        {
            // Split on a line that contains only GO (case-insensitive),
            // optionally surrounded by whitespace.
            var batches = Regex.Split(
                script,
                @"^\s*GO\s*$",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);

            foreach (var batch in batches)
            {
                if (!string.IsNullOrWhiteSpace(batch))
                {
                    yield return batch;
                }
            }
        }
    }
}
