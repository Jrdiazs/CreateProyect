using CreateProject.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CreateProject.Data
{
    public interface IRepositoryProject : IConnections
    {
        string GetPrimaryKey(string tableName, string schema);

        List<PropertiesView> GetProperties(string tableName);

        List<TablesView> GetTables();
    }

    public class RepositoryProject : Connections, IRepositoryProject
    {
        public RepositoryProject()
        { }

        public string GetPrimaryKey(string tableName, string schema)
        {
            try
            {
                var sqlQuery = new StringBuilder();
                sqlQuery.AppendLine(@"SELECT COLUMN_NAME PrimaryKey");
                sqlQuery.AppendLine(@"FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE");
                sqlQuery.AppendLine(@"WHERE OBJECTPROPERTY(");
                sqlQuery.AppendLine(@"OBJECT_ID(CONSTRAINT_SCHEMA + '.' + QUOTENAME(CONSTRAINT_NAME)),");
                sqlQuery.AppendLine(@"'IsPrimaryKey'");
                sqlQuery.AppendLine(@") = 1");
                sqlQuery.AppendLine(@"AND TABLE_NAME = @tableName");
                sqlQuery.AppendLine(@"AND TABLE_SCHEMA = @schema;");

                return DataBase.QueryFirst<string>(sql: sqlQuery.ToString(), param: new { tableName = tableName, schema = schema }, commandType: CommandType.Text) ?? string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<PropertiesView> GetProperties(string tableName)
        {
            try
            {
                var sqlQuery = new StringBuilder();
                sqlQuery.AppendLine(@"SELECT c.COLUMN_NAME ColumnName,");
                sqlQuery.AppendLine(@"c.ORDINAL_POSITION OrderColumn,");
                sqlQuery.AppendLine(@"c.IS_NULLABLE IsNullable,");
                sqlQuery.AppendLine(@"c.DATA_TYPE DataType,");
                sqlQuery.AppendLine(@"c.CHARACTER_MAXIMUM_LENGTH MaximumLength");
                sqlQuery.AppendLine(@"FROM INFORMATION_SCHEMA.COLUMNS c");
                sqlQuery.AppendLine(@"WHERE c.TABLE_NAME = @tableName");

                var query = DataBase.Query<PropertiesView>(sqlQuery.ToString(), param: new { tableName = tableName }, commandType: CommandType.Text).ToList();

                return query ?? new List<PropertiesView>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<TablesView> GetTables()
        {
            try
            {
                var sqlQuery = new StringBuilder();
                sqlQuery.AppendLine(@"SELECT");
                sqlQuery.AppendLine(@"c.TABLE_NAME TableName,c.*");
                sqlQuery.AppendLine(@"FROM");
                sqlQuery.AppendLine(@"INFORMATION_SCHEMA.TABLES c WHERE c.TABLE_NAME NOT IN('sysdiagrams','__RefactorLog') AND c.TABLE_TYPE <> 'View';");

                var query = DataBase.Query<TablesView>(sqlQuery.ToString(), commandType: CommandType.Text).ToList();
                return query ?? new List<TablesView>();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}