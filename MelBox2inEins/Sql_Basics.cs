﻿using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBox2
{
    public partial class MelBoxSql
    {

        private readonly string DataSource = "Data Source=" + DbPath;
        internal static string DbPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "DB", "MelBox2.db");

        #region Methods

        public bool SqlNonQuery(string query, Dictionary<string, object> args = null)
        {
            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = query;

                    if (args != null && args.Count > 0)
                    {
                        foreach (string key in args.Keys)
                        {
                            command.Parameters.AddWithValue(key, args[key]);
                        }
                    }

                    return 0 != command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("SqlNonQuery(): " + query + "\r\n" + ex.GetType() + "\r\n" + ex.Message);
            }
        }

        /// <summary>
        /// BAUSTELLE
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="query"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public DataTable SqlSelectDataTable(string tableName, string query, Dictionary<string, object> args = null)
        {


            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    command.CommandText = query;

                    if (args != null && args.Count > 0)
                    {
                        foreach (string key in args.Keys)
                        {
                            command.Parameters.AddWithValue(key, args[key]);
                        }
                    }

                    try
                    {
                        using (var reader = command.ExecuteReader())
                        {

                            DataTable shiftTable = new DataTable
                            {
                                TableName = tableName
                            };

                            //Mit Schema einlesen
                            shiftTable.Load(reader);
                            return shiftTable;
                        }
                    }
                    catch
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            DataTable shiftTable = new DataTable
                            {
                                TableName = tableName
                            };

                            //zu Fuß einlesen
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                //Spalten einrichten
                                //shiftTable.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
                                shiftTable.Columns.Add(reader.GetName(i), typeof(string));
                            }
                            int n = 0;

                            while (reader.Read())
                            {
                                List<object> row = new List<object>();

                                Console.Write("\r\nZeile {0}:", n++);

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string colType = shiftTable.Columns[i].DataType.Name;

                                    if (reader.IsDBNull(i))
                                    {
                                        row.Add(string.Empty);
                                        Console.Write("'NULL'\t");
                                    }
                                    else
                                    {
                                        string r = reader.GetFieldValue<string>(i);
                                        row.Add(r);
                                        Console.Write(r + "\t");
                                    }
                                }

                                shiftTable.Rows.Add(row.ToArray());
                            }
                            return shiftTable;
                        }
                    }
                
                }
            }
            catch (Exception ex)
            {
                throw new Exception("SqlSelectDataTable(): " + query + "\r\n" + ex.GetType() + "\r\n" + ex.Message);
            }

          //  return shiftTable;
        }

        /// <summary>
        /// Liest den ersten Eintrag der ersten Spalte des Abfrageergebnisses als Integer 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="args"></param>
        /// <returns>Abfrageergebnis; Bei Fehler oder leerem Ergebnis 0</returns>
        public int SqlSelectInteger(string query, Dictionary<string, object> args = null)
        {
            int result = 0;
            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    command.CommandText = query;

                    if (args != null && args.Count > 0)
                    {
                        foreach (string key in args.Keys)
                        {
                            command.Parameters.AddWithValue(key, args[key]);
                        }
                    }


                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Lese Eintrag
                            if (int.TryParse(reader.GetString(0), out result))
                            {
                                return result;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("SqlSelectInteger(): " + query + "\r\n" + ex.GetType() + "\r\n" + ex.Message);
            }

            return result;
        }

        public List<ulong> SqlSelectPhoneNumbers(string query, Dictionary<string, object> args = null)
        {
            try
            {
                List<ulong> list = new List<ulong>();

                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    command.CommandText = query;

                    if (args != null &&  args.Count > 0)
                    {
                        foreach (string key in args.Keys)
                        {
                            command.Parameters.AddWithValue(key, args[key]);
                        }
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Lese Eintrag
                            if (ulong.TryParse(reader.GetString(0), out ulong result))
                            {
                                list.Add(result);
                            }
                        }
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("SqlSelectPhoneNumbers(): " + query + "\r\n" + ex.GetType() + "\r\n" + ex.Message);
            }                       
        }

        public System.Net.Mail.MailAddressCollection SqlSelectEmailAddresses(string query, Dictionary<string, object> args = null)
        {
            try
            {
                System.Net.Mail.MailAddressCollection mailTo = new System.Net.Mail.MailAddressCollection();

                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    command.CommandText = query;

                    if (args != null && args.Count > 0)
                    {
                        foreach (string key in args.Keys)
                        {
                            command.Parameters.AddWithValue(key, args[key]);
                        }
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Lese Eintrag
                            while (reader.Read())
                            {
                                //Lese Eintrag
                                string name = reader.GetString(0);
                                string mail = reader.GetString(1);

                                if (IsEmail(mail))
                                {
                                    mailTo.Add(new System.Net.Mail.MailAddress(mail, name));
                                }
                            }
                        }
                    }
                    
                }

                return mailTo;
            }
            catch (Exception ex)
            {
                throw new Exception("SqlSelectEmailAddresses(): " + query + "\r\n" + ex.GetType() + "\r\n" + ex.Message);
            }
        }


        #endregion

    }
}
