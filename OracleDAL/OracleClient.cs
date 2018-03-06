using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace OracleDAL
{
    public class OracleClient
    {
        public static string ConnectionString { set; get; }
        private static OracleConnection conn = null;
     
        private static bool Open()
        {
            conn = new OracleConnection(ConnectionString);
            try
            {
                conn.Open();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void Close()
        {
            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
                conn = null;
            }
        }

        public static int CreateUpdateDelete(string sql)
        {
            try
            {
                Open();
                OracleCommand cmd = new OracleCommand(sql, conn);
                return cmd.ExecuteNonQuery();                
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

        public static IList<T> QueryForList<T>(string sql)
        {
            try
            {
                Open();
                OracleDataReader Dtr = QueryForReader(sql);
                return Dtr2List<T>(Dtr);
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

        public static object QueryForObj<T>(string sql)
        {
            try
            {
                Open();
                OracleDataReader Dtr = QueryForReader(sql);
                return Dtr2Obj<T>(Dtr);                
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

        private static OracleDataReader QueryForReader(string sql)
        {
            try
            {
                OracleCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                return cmd.ExecuteReader();                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        /// <summary>
        /// Transforma um DataReader em uma Lista Genérica
        /// </summary>
        /// <typeparam name="T">Tipo do Objeto (Classe)</typeparam>
        /// <param name="rdr">DataReader a ser transformado</param>
        /// <returns>Objeto IList (Lista Genérica)</returns>
        private static IList<T> Dtr2List<T>(OracleDataReader rdr)
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
                        string typeFullName = obj.GetProperty(rdr.GetName(i)).PropertyType.FullName;
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
        private static object Dtr2Obj<T>(OracleDataReader rdr)
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
                        string typeFullName = obj.GetProperty(rdr.GetName(i)).PropertyType.FullName;
                        tempValue = GetDBNullValue(typeFullName);
                    }
                    else
                    {
                        tempValue = rdr.GetValue(i);
                    }
                    obj.GetProperty(rdr.GetName(i)).SetValue(t, tempValue, null);
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
        private static object GetDBNullValue(string typeFullName)
        {
            typeFullName = typeFullName.ToLower();

            if (typeFullName == OracleDbType.Varchar2.ToString().ToLower())           
                return String.Empty;
            
            if (typeFullName == OracleDbType.Int32.ToString().ToLower())            
                return 0;
            
            if (typeFullName == OracleDbType.Date.ToString().ToLower())            
                return Convert.ToDateTime("");
            
            if (typeFullName == OracleDbType.Boolean.ToString().ToLower())            
                return false;
            
            if (typeFullName == OracleDbType.Int16.ToString().ToLower())            
                return 0;
            
            return null;
        }        

        public static int ExcuteProc(string ProcName)
        {
            return ExcuteSQL(ProcName, null, CommandType.StoredProcedure);
        }

        public static int ExcuteProc(string ProcName, OracleParameter[] pars)
        {
            return ExcuteSQL(ProcName, pars, CommandType.StoredProcedure);
        }

        public static int ExcuteSQL(string strSQL)
        {
            return ExcuteSQL(strSQL, null);
        }

        public static int ExcuteSQL(string strSQL, OracleParameter[] paras)
        {
            return ExcuteSQL(strSQL, paras, CommandType.Text);
        }

        /// <summary>
        /// Executa uma instrução SQL
        /// </summary>    
        /// <param name="strSQL">Instrução SQL</param>    
        /// <param name="paras">Lista de parâmetros. Sem parametros passar null</param>    
        /// <param name="cmdType">Tipo de comando</param>    
        /// <returns>Retorna o número de linhas afetadas</returns>    
        public static int ExcuteSQL(string strSQL, OracleParameter[] paras, CommandType cmdType)
        {
            try
            {                
                Open();
                OracleCommand cmd = new OracleCommand(strSQL, conn);
                cmd.CommandType = cmdType;
                if (paras != null)                
                    cmd.Parameters.AddRange(paras);
                
                return cmd.ExecuteNonQuery();
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
    }
}