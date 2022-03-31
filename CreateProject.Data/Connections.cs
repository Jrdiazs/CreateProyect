using CreateProject.Tools.String;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Data.SqlClient;

namespace CreateProject.Data
{
    // <summary>
    /// Clase de conexion
    /// </summary>
    public abstract class Connections : IConnections, IDisposable
    {
        #region [Propiedades]

        /// <summary>
        /// Obtiene la conexion actual de base de datos
        /// </summary>
        public IDbConnection DataBase { get; private set; }

        /// <summary>
        /// Obtiene que tipo de conexion se esta utilizando
        /// </summary>
        public EnumConnectionType TypeConnections { get; private set; }

        #endregion [Propiedades]

        #region [Constructor]

        /// <summary>
        /// Crea un objeto de conexion a bcm por default
        /// </summary>
        public Connections()
        {
            EvaluateConnections();
        }

        /// <summary>
        /// Verifica que tipo de conexion debe crear
        /// </summary>
        private void EvaluateConnections()
        {
            try
            {
                ///Crea la conexion por default
                string typeConnection = "TypeDatabase".ReadAppConfig("0");
                if ("0".Equals(typeConnection) || string.Empty.Equals(typeConnection))
                {
                    TypeConnections = EnumConnectionType.SQL;
                    DataBase = GetConnectionSql("DefaultConnection");
                }
                else if (!typeConnection.IsEmpty())
                {
                    switch (typeConnection.ToUpper())
                    {
                        case "0":
                        case "SQL":
                            TypeConnections = EnumConnectionType.SQL;
                            DataBase = GetConnectionSql("DefaultConnection");
                            break;

                        case "1":
                        case "ORACLE":
                            TypeConnections = EnumConnectionType.Oracle;
                            DataBase = GetOracleConnection("OracleConnection");
                            break;

                        case "2":
                        case "MYSQL":
                            TypeConnections = EnumConnectionType.Mysql;
                            DataBase = GetConnectionMySql("MysqlConnection");
                            break;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Crea una conexion a partir de otra
        /// </summary>
        /// <param name="anotherConnection">otra conexion</param>
        public Connections(IDbConnection anotherConnection)
        {
            if (anotherConnection is SqlConnection)
                TypeConnections = EnumConnectionType.SQL;
            else if (anotherConnection is OracleConnection)
                TypeConnections = EnumConnectionType.Oracle;
            else if (anotherConnection is MySqlConnection)
                TypeConnections = EnumConnectionType.Mysql;

            EqualConnection(anotherConnection);
        }

        /// <summary>
        /// Puede crear una conexion sql u  oracle segun el EnumConnectionType y el nombre de la llave de conexion
        /// </summary>
        /// <param name="typeConnection">type connection sql u oracle</param>
        /// <param name="keyConnection">llave de coneccion en el web config</param>
        public Connections(EnumConnectionType typeConnection, string keyConnection)
        {
            TypeConnections = typeConnection;
            switch (typeConnection)
            {
                default:
                case EnumConnectionType.SQL:
                    DataBase = GetConnectionSql(keyConnection);
                    break;

                case EnumConnectionType.Oracle:
                    DataBase = GetOracleConnection(keyConnection);
                    break;

                case EnumConnectionType.Mysql:
                    DataBase = GetConnectionMySql(keyConnection);
                    break;
            }
        }

        #endregion [Constructor]

        #region [Metodos]

        /// <summary>
        /// Crea una conexion a sql server segun el nombre de la llave en el web config
        /// </summary>
        /// <param name="keyConnection">nombre de la llave de conexion en el web config</param>
        /// <returns>SqlConnection</returns>
        public SqlConnection GetConnectionSql(string keyConnection)
        {
            try
            {
                string connection = keyConnection.ReadConnections();
                return new SqlConnection(connection);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Crea una conexion de base de datos con mysql
        /// </summary>
        /// <param name="keyConnection">nombre de la llave de conexion en el web config</param>
        /// <returns>MySqlConnection</returns>
        public MySqlConnection GetConnectionMySql(string keyConnection)
        {
            try
            {
                string connection = keyConnection.ReadConnections();
                return new MySqlConnection(connection);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Crea una nueva conexion a oracle segun la llave de conexion en el web config
        /// </summary>
        /// <param name="keyConnection">nombr de la llave de conexion en el web config</param>
        /// <returns>OracleConnection</returns>
        public OracleConnection GetOracleConnection(string keyConnection)
        {
            try
            {
                string connection = keyConnection.ReadConnections();
                return new OracleConnection(connection);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Cierra la conexion actual
        /// </summary>
        public void Close()
        {
            try
            {
                if (DataBase != null && DataBase.State != ConnectionState.Closed)
                    DataBase.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Abre la conexion actual
        /// </summary>
        public void Open()
        {
            try
            {
                if (DataBase != null && DataBase.State != ConnectionState.Open)
                    DataBase.Open();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Iguala otra conexion a la actual para realizar transacciones
        /// </summary>
        /// <param name="anotherConnection">IDbConnection</param>
        public void EqualConnection(IDbConnection anotherConnection)
        {
            try
            {
                DataBase = anotherConnection;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion [Metodos]

        #region [IDisposable]

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Close();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DataConnection() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        #endregion [IDisposable]
    }

    // <summary>
    /// Clase de conexion
    /// </summary>
    public interface IConnections : IDisposable
    {
        /// <summary>
        /// Iguala otra conexion a la actual para realizar transacciones
        /// </summary>
        /// <param name="anotherConnection">IDbConnection</param>
        void EqualConnection(IDbConnection anotherConnection);

        /// <summary>
        /// Obtiene la conexion actual de base de datos
        /// </summary>
        IDbConnection DataBase { get; }

        /// <summary>
        /// Obtiene que tipo de conexion se esta utilizando
        /// </summary>
        EnumConnectionType TypeConnections { get; }

        /// <summary>
        /// Cierra la conexion actual
        /// </summary>
        void Close();

        /// <summary>
        /// Crea una llave de conexion a sql server segun el nombre de la llave en el web config
        /// </summary>
        /// <param name="stringConnection">nombre de la llave de conexion en el web config</param>
        /// <returns>SqlConnection</returns>
        SqlConnection GetConnectionSql(string stringConnection);

        /// <summary>
        /// Crea una nueva conexion a oracle segun la llave de conexion en el web config
        /// </summary>
        /// <param name="stringConnection">nombr de la llave de conexion en el web config</param>
        /// <returns>OracleConnection</returns>
        OracleConnection GetOracleConnection(string stringConnection);

        /// <summary>
        /// Crea una conexion de base de datos con mysql
        /// </summary>
        /// <param name="keyConnection">nombre de la llave de conexion en el web config</param>
        /// <returns>MySqlConnection</returns>
        MySqlConnection GetConnectionMySql(string keyConnection);

        /// <summary>
        /// Abre la conexion actual
        /// </summary>
        void Open();
    }
}