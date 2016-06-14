using Newtonsoft.Json.Linq;
using NLog;
using SolidQ.ABI.Extensibility.Compiler;
using System;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.OleDb;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace SolidQ.ABI.Plugin.Common.MetadataCompiler
{
    [Export(typeof(IMetadataCompilerPlugin))]
    public class ColumnsMetadataCompiler : IMetadataCompilerPlugin
    {
        private static Logger _logger;

        private static string ColumnsMetadataCommandFormat = @"
            SELECT
                t.COLUMN_NAME,
	            COLUMN_DATA_TYPE = CASE
                    WHEN t.DATA_TYPE IN('varchar', 'char', 'nvarchar', 'nchar', 'binary', 'varbinary')
                        THEN LOWER(t.DATA_TYPE) + '(' + CAST(t.CHARACTER_MAXIMUM_LENGTH AS VARCHAR(10)) + ')'
                    WHEN t.DATA_TYPE IN('time', 'datetime2', 'datetimeoffset')
                        THEN LOWER(t.DATA_TYPE) + '(' + CAST(t.[DATETIME_PRECISION] AS VARCHAR(10)) + ')'		
		            when t.DATA_TYPE IN ('decimal', 'numeric')
                        THEN LOWER(t.DATA_TYPE) + '(' + CAST(t.NUMERIC_PRECISION AS VARCHAR(10)) + ',' + CAST(t.NUMERIC_SCALE AS VARCHAR(10)) + ')'
		            ELSE LOWER(t.DATA_TYPE)
                END,
	            COLUMN_ALLOW_NULL = CASE
		            WHEN t.IS_NULLABLE = 'NO'
		                THEN 'NOT NULL'
                    ELSE 'NULL'
	            END
            FROM INFORMATION_SCHEMA.COLUMNS AS t
            WHERE t.TABLE_SCHEMA = '{0}' AND t.TABLE_NAME = '{1}'
            ORDER BY t.ORDINAL_POSITION";

        #region Properties

        public string Name
        {
            get
            {
                return "ColumnsMetadataCompiler";
            }
        }

        public string Author
        {
            get
            {
                return "SolidQ";
            }
        }

        public string Version
        {
            get
            {
                return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            }
        }

        public string Description
        {
            get 
            {
                return "Create JSON column definition reading from INFORMATION_SCHEMA view";
            }
        }

        public string Help
        {
            get
            {
                return
                    "Plugin parameters are: connectionString, tableSchema, tableName, [connectionType]" + Environment.NewLine +
                    "connectionType is opional and supports OLEDB and ODBC. If omitted OLEDB is assumed.";
            }
        }

        #endregion

        public void Initialize(LogFactory log)
        {
            _logger = log.GetCurrentClassLogger();
            _logger.Debug("Initialize");
        }

        public void Shutdown()
        {
            _logger.Debug("Shutdown");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">A string array that contains plugin parameters [connectionString, tableSchema, tableName, connectionType]</param>
        /// <returns>A string representing JObject</returns>
        public dynamic Compile(params string[] args)
        {
            _logger.Debug("Compile");

            #region Argument exceptions

            if (args == null)
                throw new ArgumentNullException("args");

            if (args.Length < 3 || args.Length > 4)
                throw new ArgumentException("Invalid parameters number");

            if (args.Any((a) => a == null))
                throw new ArgumentException("Some parameter is null");

            string connectionType = "OLEDB";
            if(args.Length == 4)
            {
                connectionType = args[3].ToUpper();
                if (connectionType != "OLEDB" && connectionType != "ODBC")
                    throw new ArgumentException("connectionType parameter can OLEDB or ODBC only");
            }

            #endregion

            using (var table = GetColumnsMetadata(connectionString: args[0], tableSchema: args[1], tableName: args[2], connectionType: connectionType))
                return FormatColumnsMetadata(table);
        }

        private DataTable GetColumnsMetadata(string connectionString, string tableSchema, string tableName, string connectionType)
        {
            var columnsMetadataCommand = string.Format(ColumnsMetadataCommandFormat, tableSchema, tableName);

            using (IDbConnection connection = CreateConnectionObject(connectionString, connectionType))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = columnsMetadataCommand;

                    using (var reader = command.ExecuteReader())
                    {
                        var table = new DataTable();
                        table.Load(reader);
                        return table;
                    }
                }
            }
        }

        private IDbConnection CreateConnectionObject(string connectionString, string connectionType)
        {
            IDbConnection connection = null;

            _logger.Debug("Driver: {0}", connectionType);

            if (connectionType == "OLEDB")
                connection = new OleDbConnection(connectionString);
            else
                connection = new OdbcConnection(connectionString);

            return connection;
        }

        private dynamic FormatColumnsMetadata(DataTable table)
        {
            Debug.Assert(table.Columns["COLUMN_NAME"] != null);
            Debug.Assert(table.Columns["COLUMN_DATA_TYPE"] != null);

            foreach (DataRow row in table.Rows)
                _logger.Debug("Retrieved column [{0}] [{1}]", row.ItemArray);


            var columns = new JArray() as dynamic;

            foreach (DataRow row in table.Rows)
            {
                dynamic column = new JObject();
                column.Name = row["COLUMN_NAME"];
                column.DataType = row["COLUMN_DATA_TYPE"];
                columns.Add(column);
            }

            return columns;
        }
    }
}
