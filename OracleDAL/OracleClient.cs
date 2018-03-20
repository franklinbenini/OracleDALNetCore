using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace OracleDAL
{
    public class OracleClient : IDisposable
    {
        private string connString = null;
        private OracleConnection conn = null;
        private Dictionary<string, object> parametros = new Dictionary<string, object>();

        public OracleClient(string ConnectionString)
        {
            this.connString = ConnectionString;
        }

        private bool Open()
        {            
            this.conn = new OracleConnection(connString);
                        
            try
            {
                this.conn.Open();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Close()
        {
            if (this.conn != null)
            {
                this.parametros.Clear();
                this.conn.Close();
                this.conn.Dispose();
                this.conn = null;
            }
        }
        
        public void AddParametro(string nome, object valor)
        {       
            this.parametros.Add(nome, valor);
        }

        private void AdicionaParametrosNoComando(OracleCommand comando)
        {
            OracleParameter _dbParametro;
            foreach (var param in parametros)
            {
                _dbParametro = comando.CreateParameter();
                _dbParametro.ParameterName = param.Key;
                _dbParametro.Value = param.Value;
                comando.Parameters.Add(_dbParametro);
            }
            _dbParametro = null;
            parametros.Clear();
        }

        public int CreateUpdateDelete(string sql)
        {
            try
            {
                Open();
                using (OracleCommand cmd = new OracleCommand(sql, conn))
                {                    
                    if (parametros != null)
                    {
                        AdicionaParametrosNoComando(cmd);
                    }
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {                
                Close();
            }
        }

        public IList<T> QueryForList<T>(string sql)
        {
            try
            {
                Open();
                using (OracleDataReader Dtr = QueryForReader(sql))
                {
                    return Dtr2List<T>(Dtr);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close();
            }
        }

        public object QueryForObj<T>(string sql)
        {
            try
            {
                Open();
                using (OracleDataReader Dtr = QueryForReader(sql))
                {
                    return Dtr2Obj<T>(Dtr);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close();
            }
        }

        private OracleDataReader QueryForReader(string sql)
        {
            try
            {
                using (OracleCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    return cmd.ExecuteReader();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IList<T> ProcForList<T>(string nomeProcedure)
        {
            return ProcForList<T>(nomeProcedure, "P_RETORNO");
        }

        public IList<T> ProcForList<T>(string nomeProcedure, string nomeParRetorno)
        {
            try
            {
                Open();
                using (OracleDataReader Dtr = ProcForReader(nomeProcedure, nomeParRetorno))
                {
                    return Dtr2List<T>(Dtr);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close();
            }
        }

        public object ProcForObj<T>(string nomeProcedure)
        {
            return ProcForObj<T>(nomeProcedure, "P_RETORNO");
        }

        public object ProcForObj<T>(string nomeProcedure, string nomeParRetorno)
        {
            try
            {
                Open();
                using (OracleDataReader Dtr = ProcForReader(nomeProcedure, nomeParRetorno))
                {
                    return Dtr2Obj<T>(Dtr);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close();
            }
        }
                
        private OracleDataReader ProcForReader(string nomeProcedure, string nomeParRetorno)
        {               
            using (OracleCommand cmd = new OracleCommand(nomeProcedure, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (parametros != null)
                {
                    AdicionaParametrosNoComando(cmd);
                }
                cmd.Parameters.Add(new OracleParameter(nomeParRetorno, OracleDbType.RefCursor)).Direction = ParameterDirection.Output;                        
                cmd.CommandText = nomeProcedure;
                return cmd.ExecuteReader();
            }            
        }

        public int ProcNonQuery(string nomeProcedure)
        {
            try
            {
                Open();
                using (OracleCommand cmd = new OracleCommand(nomeProcedure, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (parametros != null)
                    {
                        AdicionaParametrosNoComando(cmd);
                    }
                    cmd.CommandText = nomeProcedure;
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close();
            }
        }

        /// <summary>
        /// Transforma um DataReader em uma Lista Genérica
        /// </summary>
        /// <typeparam name="T">Tipo do Objeto (Classe)</typeparam>
        /// <param name="rdr">DataReader a ser transformado</param>
        /// <returns>Objeto List (Lista Genérica)</returns>
        private IList<T> Dtr2List<T>(OracleDataReader rdr)
        {
            IList<T> list = new List<T>();

            while (rdr.Read())
            {
                T t = System.Activator.CreateInstance<T>();
                Type obj = t.GetType();
                
                for (int i = 0; i < rdr.FieldCount; i++)
                {
                    object tempValue = null;
                    if (rdr.IsDBNull(i))
                    {
                        string typeFullName = obj.GetProperty(rdr.GetName(i).ToLower()).PropertyType.FullName;
                        tempValue = GetDBNullValue(typeFullName);
                    }
                    else
                    {
                        tempValue = rdr.GetValue(i);
                    }

                    obj.GetProperty(rdr.GetName(i).ToLower()).SetValue(t, tempValue, null);
                }

                list.Add(t);
            }
            return list;
        }
        
        /// <summary>
        /// Transforma um DataReader em um Objeto
        /// </summary>
        /// <typeparam name="T">Tipo do Objeto (Classe)</typeparam>
        /// <param name="rdr">DataReader a ser transformado</param>
        /// <returns>Objeto</returns>
        private object Dtr2Obj<T>(OracleDataReader rdr)
        {
            T t = System.Activator.CreateInstance<T>();
            Type obj = t.GetType();

            if (rdr.Read())
            {
                for (int i = 0; i < rdr.FieldCount; i++)
                {
                    object tempValue = null;
                    if (rdr.IsDBNull(i))
                    {
                        string typeFullName = obj.GetProperty(rdr.GetName(i).ToLower()).PropertyType.FullName;
                        tempValue = GetDBNullValue(typeFullName);
                    }
                    else
                    {
                        tempValue = rdr.GetValue(i);
                    }
                    
                    
                    obj.GetProperty(rdr.GetName(i).ToLower()).SetValue(t, tempValue, null);
                }
                return t;
            }
            else
                return null;
        }               

        /// <summary>    
        /// Retorna um valor padrão para DBNull de acordo com o tipo de dados
        /// </summary>    
        /// <param name="typeFullName">O nome completo do tipo de dados. Ex.: System.Int32</param>    
        /// <returns>O valor padrão do tipo de dados</returns>
        private object GetDBNullValue(string typeFullName)
        {
            typeFullName = typeFullName.ToLower();

            // Mais comuns
            if (typeFullName == OracleDbType.Varchar2.ToString().ToLower())           
                return String.Empty;

            if (typeFullName == OracleDbType.Int64.ToString().ToLower())
                return 0;

            if (typeFullName == OracleDbType.Date.ToString().ToLower())
                return Convert.ToDateTime("");

            if (typeFullName == OracleDbType.Char.ToString().ToLower())
                return String.Empty; // O C# entende o CHAR do Oracle como String


            // Menos comuns
            if (typeFullName == OracleDbType.Double.ToString().ToLower())
                return 0;
            
            if (typeFullName == OracleDbType.Int32.ToString().ToLower())
                return 0;

            if (typeFullName == OracleDbType.Decimal.ToString().ToLower())
                return 0;

            if (typeFullName == OracleDbType.Boolean.ToString().ToLower())
                return false;

            if (typeFullName == OracleDbType.Long.ToString().ToLower())
                return String.Empty;
            
            if (typeFullName == OracleDbType.Int16.ToString().ToLower())
                return 0;
            
            return null;
        }

        public void Dispose()
        {
            this.Close();            
        }

        //public static int ExecuteProc(string ProcName)
        //{
        //    return ExecuteSQL(ProcName, null, CommandType.StoredProcedure);
        //}

        //public static int ExecuteProc(string ProcName, OracleParameterCollection pars)
        //{
        //    return ExecuteSQL(ProcName, pars, CommandType.StoredProcedure);
        //}

        //public static int ExecuteSQL(string strSQL)
        //{
        //    return ExecuteSQL(strSQL, null);
        //}

        //public static int ExecuteSQL(string strSQL, OracleParameterCollection paras)
        //{
        //    return ExecuteSQL(strSQL, paras, CommandType.Text);
        //}

        ///// <summary>
        ///// Executa uma instrução SQL
        ///// </summary>    
        ///// <param name="strSQL">Instrução SQL</param>    
        ///// <param name="paras">Lista de parâmetros. Sem parametros passar null</param>    
        ///// <param name="cmdType">Tipo de comando</param>    
        ///// <returns>Retorna o número de linhas afetadas</returns>    
        //public static int ExecuteSQL(string strSQL, OracleParameterCollection paras, CommandType cmdType)
        //{               
        //    try
        //    {                
        //        Open();
        //        using (OracleCommand cmd = new OracleCommand(strSQL, conn))
        //        {
        //            cmd.CommandType = cmdType;
        //            if (paras != null)
        //                cmd.Parameters.Add(paras);

        //            return cmd.ExecuteNonQuery();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        Close();
        //    }
        //}
    }
}