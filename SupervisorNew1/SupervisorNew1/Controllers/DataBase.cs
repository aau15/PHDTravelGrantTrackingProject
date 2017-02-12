using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Data.SqlClient;
namespace SupervisorNew1.Controllers
{
    // database adapter class that handles the communication layer with the database
    public class DataBase : IDisposable
    {
        private MySqlConnection con_mysql;//create connection target
        private static string strConnectString = ConfigurationManager.ConnectionStrings["DefaultConnect"].ToString();

        //open database connection
        private void Open()
        {
            if (con_mysql == null)//check null
            {
                con_mysql = new MySqlConnection(strConnectString);//create database connection target
            }
            if (con_mysql.State == ConnectionState.Closed)
                con_mysql.Open();
        }

        //Dispose
        public void Dispose()
        {
            if (con_mysql != null)
            {
                con_mysql.Dispose();
                con_mysql = null;
            }
        }

        //close database connection
        public void Close()
        {
            if (con_mysql != null)
                con_mysql.Close();
        }

        //execute query with no return
        public int RunProc(string procName)
        {
            try
            {
                this.Open();
                MySqlCommand cmd = new MySqlCommand(procName, con_mysql);
                cmd.ExecuteNonQuery();
                this.Close();
                return 1;
            }
            catch (Exception)
            {
                return -1;
            }

        }

        //execute query with return
        public DataSet RunProcReturn(string procName, string tbName)
        {
            DataSet ds = new DataSet();
            try
            {
                MySqlDataAdapter dap = CreateDataAdapterMysql(procName, null);
                dap.Fill(ds, tbName);
                this.Close();
                return ds;
            }
            catch (Exception)
            {
                return null;
            }
        }

        //create the data adapter for mysql
        private MySqlDataAdapter CreateDataAdapterMysql(string procName, MySqlParameter[] prams)
        {
            this.Open();
            MySqlDataAdapter dap = new MySqlDataAdapter(procName, con_mysql);
            dap.SelectCommand.CommandType = CommandType.Text;
            if (prams != null)
            {
                foreach (MySqlParameter parameter in prams)
                    dap.SelectCommand.Parameters.Add(parameter);
            }
            dap.SelectCommand.Parameters.Add(new MySqlParameter("ReturnValue", MySqlDbType.Int32, 4,
                ParameterDirection.ReturnValue, false, 0, 0, string.Empty, DataRowVersion.Default, null));
            return dap;
        }




    }
}