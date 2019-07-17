using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVReader
{
    public class SQLite: ISQLLite
    {
        private SQLiteConnection con;
        private SQLiteCommand cmd;
        private SQLiteDataAdapter adapter;

        public SQLite()
        {
            string conn = @"Data Source=C:\Learn\Demo\CSVReader\CSVReader\DBCSVReader.db; Version=3;";

            con = new SQLiteConnection(conn);
                //System.Configuration.ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString);



        }
        public int Execute(string sql_statement)
        {
            con.Open();
            cmd = con.CreateCommand();
            cmd.CommandText = sql_statement;
            int row_updated;
            try
            {
                row_updated = cmd.ExecuteNonQuery();
            }
            catch
            {
                con.Close();
                return 0;
            }
            con.Close();
            return row_updated;
        }
        public DataTable GetDataTable(string tablename)
        {
            DataTable DT = new DataTable();
            con.Open();
            cmd = con.CreateCommand();
            cmd.CommandText = string.Format("SELECT * FROM {0}", tablename);
            adapter = new SQLiteDataAdapter(cmd);
            adapter.AcceptChangesDuringFill = false;
            adapter.Fill(DT);
            con.Close();
            DT.TableName = tablename;
            return DT;
        }
        public int SaveDataTable(DataTable DT)
        {
            try
            {
                int updatedRows = 0;
                con.Open();
                cmd = con.CreateCommand();
                cmd.CommandText = string.Format("SELECT * FROM {0}", DT.TableName);
                adapter = new SQLiteDataAdapter(cmd);
                SQLiteCommandBuilder builder = new SQLiteCommandBuilder(adapter);
                updatedRows =  adapter.Update(DT);
                con.Close();
                return updatedRows;
            }
            catch (Exception Ex)
            {
                throw;
            }
        }
    }


    public interface ISQLLite
    {
        int Execute(string sql_statement);
         int SaveDataTable(DataTable DT);
        DataTable GetDataTable(string tablename);
    }

}

