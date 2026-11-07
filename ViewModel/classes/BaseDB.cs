using System;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace ViewModel.classes
{
    public class BaseDB
    {
        private static readonly object db_lock = new object();
        private string DBName;
        private string database_path;
        private string connection_string;

        public BaseDB()
        {
            this.DBName = "chess.accdb";
            this.database_path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\files\database\" + this.DBName));
            this.connection_string = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={this.database_path};Persist Security Info=False;";
        }

        protected DataTable GetTable(string sql, params object[] args)
        {
            lock (db_lock)
            {
                try
                {
                    using (OleDbConnection con = new OleDbConnection(this.connection_string))
                    using (OleDbCommand cmd = new OleDbCommand(sql, con))
                    {
                        AddArgs(cmd, args);

                        DataTable table = new DataTable();
                        new OleDbDataAdapter(cmd).Fill(table);
                        return table;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        protected object GetScalar(string sql, params object[] args)
        {
            lock (db_lock)
            {
                try
                {
                    using (OleDbConnection con = new OleDbConnection(this.connection_string))
                    using (OleDbCommand cmd = new OleDbCommand(sql, con))
                    {
                        AddArgs(cmd, args);
                        con.Open();

                        object result = cmd.ExecuteScalar();
                        if (result == DBNull.Value)
                            return null;
                        return result;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        protected int ExecuteNonQuery(string sql, params object[] args)
        {
            lock (db_lock)
            {
                try
                {
                    using (OleDbConnection con = new OleDbConnection(this.connection_string))
                    using (OleDbCommand cmd = new OleDbCommand(sql, con))
                    {
                        AddArgs(cmd, args);
                        con.Open();
                        return cmd.ExecuteNonQuery();
                    }
                }
                catch
                {
                    return -1;
                }
            }
        }

        protected static int ReadInt(DataRow row, string column)
        {
            object value = row[column];
            if (value == null || value == DBNull.Value)
                return 0;
            return Convert.ToInt32(value);
        }

        private void AddArgs(OleDbCommand cmd, object[] args)
        {
            if (args == null)
                return;

            for (int i = 0; i < args.Length; i++)
            {
                cmd.Parameters.AddWithValue("?", args[i]);
            }
        }
    }
}
