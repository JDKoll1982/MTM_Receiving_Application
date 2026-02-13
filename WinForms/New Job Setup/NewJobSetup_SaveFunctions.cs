using System;
using System.Data;
using System.Printing;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using MTM_Waitlist_Application_2._0.Core.Database_Classes;

namespace MTM_Waitlist_Application_2._0.WinForms.New_Job_Setup
{
    public class NewJobSetupSaveFunctions
    {
        private readonly string _connectionString = SqlCommands.GetConnectionString(null, "mtm_waitlist", null, null);

        public void CheckDunnageSaveData(int id, string partid, string op, string dunnage, string skid,
            string cardboard, string boxes, string other1, string other2, string other3, string other4, string other5,
            string parttype)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string selectCommand = "SELECT * FROM waitlist_jobdunnage WHERE PartID = @partid AND Operation = @op";
                MySqlCommand cmd = new MySqlCommand(selectCommand, conn);
                cmd.Parameters.AddWithValue("@partid", partid);
                cmd.Parameters.AddWithValue("@op", op);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (NeedsUpdate(reader, dunnage, skid, cardboard, boxes, other1, other2, other3, other4, other5, parttype))
                        {
                            reader.Close(); // Close the reader before executing another command

                            string updateCommand = @"
                                UPDATE waitlist_jobdunnage 
                                SET Dunnage = @dunnage, Skid = @skid, Cardboard = @cardboard, Boxes = @boxes, 
                                    Other1 = @other1, Other2 = @other2, Other3 = @other3, Other4 = @other4, Other5 = @other5, 
                                    Parttype = @parttype 
                                WHERE PartID = @partid AND Operation = @op";

                            ExecuteNonQuery(conn, updateCommand, partid, op, dunnage, skid, cardboard, boxes, other1, other2, other3, other4, other5, parttype);
                        }
                    }
                    else
                    {
                        reader.Close(); // Close the reader before executing another command

                        string insertCommand = @"
                            INSERT INTO waitlist_jobdunnage (PartID, Operation, Dunnage, Skid, Cardboard, Boxes, 
                                Other1, Other2, Other3, Other4, Other5, Parttype) 
                            VALUES (@partid, @op, @dunnage, @skid, @cardboard, @boxes, @other1, @other2, @other3, @other4, @other5, @parttype)";

                        ExecuteNonQuery(conn, insertCommand, partid, op, dunnage, skid, cardboard, boxes, other1, other2, other3, other4, other5, parttype);
                    }
                }
            }
        }

        private bool NeedsUpdate(MySqlDataReader reader, string dunnage, string skid, string cardboard, string boxes, string other1, string other2, string other3, string other4, string other5, string parttype)
        {
            return reader["Dunnage"].ToString() != dunnage ||
                   reader["Skid"].ToString() != skid ||
                   reader["Cardboard"].ToString() != cardboard ||
                   reader["Boxes"].ToString() != boxes ||
                   reader["Other1"].ToString() != other1 ||
                   reader["Other2"].ToString() != other2 ||
                   reader["Other3"].ToString() != other3 ||
                   reader["Other4"].ToString() != other4 ||
                   reader["Other5"].ToString() != other5 ||
                   reader["PartType"].ToString() != parttype;
        }

        private void ExecuteNonQuery(MySqlConnection conn, string commandText, string partid, string op, string dunnage, string skid, string cardboard, string boxes, string other1, string other2, string other3, string other4, string other5, string parttype)
        {
            using (MySqlCommand cmd = new MySqlCommand(commandText, conn))
            {
                cmd.Parameters.AddWithValue("@partid", partid);
                cmd.Parameters.AddWithValue("@op", op);
                cmd.Parameters.AddWithValue("@dunnage", dunnage);
                cmd.Parameters.AddWithValue("@skid", skid);
                cmd.Parameters.AddWithValue("@cardboard", cardboard);
                cmd.Parameters.AddWithValue("@boxes", boxes);
                cmd.Parameters.AddWithValue("@other1", other1);
                cmd.Parameters.AddWithValue("@other2", other2);
                cmd.Parameters.AddWithValue("@other3", other3);
                cmd.Parameters.AddWithValue("@other4", other4);
                cmd.Parameters.AddWithValue("@other5", other5);
                cmd.Parameters.AddWithValue("@parttype", parttype);

                cmd.ExecuteNonQuery();
            }

            conn.Close();
        }

        public void SaveSetupNotes(string partid, string op, string notes)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string selectCommand = "SELECT * FROM waitlist_setupnotes WHERE PartID = @partid AND Operation = @op";
                MySqlCommand cmd = new MySqlCommand(selectCommand, conn);
                cmd.Parameters.AddWithValue("@partid", partid);
                cmd.Parameters.AddWithValue("@op", op);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (reader["SetupNotes"].ToString() != notes)
                        {
                            reader.Close(); // Close the reader before executing another command

                            string updateCommand = "UPDATE waitlist_setupnotes SET SetupNotes = @notes WHERE PartID = @partid AND Operation = @op";
                            cmd.CommandText = updateCommand;
                            cmd.Parameters.AddWithValue("@notes", notes);
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            conn.Close();
                        }
                    }
                    else
                    {
                        reader.Close(); // Close the reader before executing another command

                        string insertCommand = "INSERT INTO waitlist_setupnotes (PartID, Operation, SetupNotes) VALUES (@partid, @op, @notes)";
                        cmd.CommandText = insertCommand;
                        cmd.Parameters.AddWithValue("@notes", notes);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        conn.Close();
                    }
                }
            }
        }
    }
}