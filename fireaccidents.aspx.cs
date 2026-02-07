using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;

namespace Fire_Station_project
{
    public partial class fireaccidents : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["FireProtectionDBConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["StationName"] != null)
                {
                    txtStation.Text = Session["StationName"].ToString();
                    txtStation.ReadOnly = true;
                    txtStation.CssClass += " readonly-field";
                    BindGrid();
                }
                else
                {
                    Response.Redirect("Login.aspx");
                }
            }
        }

        private void BindGrid()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    int? currentStationID = Session["StationID"] as int?;
                    string role = Session["Role"] as string;
                    string query;

                    if (role == "Admin")
                    {
                        query = @"SELECT fa.SNO, st.StationName, fa.AccidentDate, fa.Location, fa.Cause, 
                                         fa.Injuries, fa.Fatalities, fa.DamageDescription, fa.ActionsTaken, fa.Remarks
                                  FROM FireAccidents fa
                                  INNER JOIN Stations st ON fa.StationID = st.StationID
                                  ORDER BY fa.SNO";
                    }
                    else
                    {
                        query = @"SELECT fa.SNO, st.StationName, fa.AccidentDate, fa.Location, fa.Cause, 
                                         fa.Injuries, fa.Fatalities, fa.DamageDescription, fa.ActionsTaken, fa.Remarks
                                  FROM FireAccidents fa
                                  INNER JOIN Stations st ON fa.StationID = st.StationID
                                  WHERE fa.StationID = @StationID
                                  ORDER BY fa.SNO";
                    }

                    SqlCommand cmd = new SqlCommand(query, conn);
                    if (role != "Admin" && currentStationID.HasValue)
                        cmd.Parameters.AddWithValue("@StationID", currentStationID.Value);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    GridView1.DataSource = dt;
                    GridView1.DataBind();
                }
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Error loading fire accident records: " + ex.Message.Replace("'", "\\'") + "');", true);
            }
        }

        protected void btnAddDetails_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = true;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
            ClearForm();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string accidentDate = txtAccidentDate.Text.Trim();
                string location = txtLocation.Text.Trim();
                string cause = txtCause.Text.Trim();
                string injuries = txtInjuries.Text.Trim();
                string fatalities = txtFatalities.Text.Trim();
                string damage = txtDamage.Text.Trim();
                string actions = txtActions.Text.Trim();
                string remarks = txtRemarks.Text.Trim();

                int? stationID = Session["StationID"] as int?;

                if (string.IsNullOrEmpty(accidentDate) || string.IsNullOrEmpty(location) || stationID == null)
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Please fill required fields: Accident Date, Location');", true);
                    return;
                }

                int nextSNO = GetNextSequentialSNO();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SET IDENTITY_INSERT FireAccidents ON;
                                     INSERT INTO FireAccidents (SNO, AccidentDate, Location, Cause, Injuries, Fatalities,
                                                               DamageDescription, ActionsTaken, Remarks, StationID)
                                     VALUES (@SNO, @AccidentDate, @Location, @Cause, @Injuries, @Fatalities,
                                             @DamageDescription, @ActionsTaken, @Remarks, @StationID);
                                     SET IDENTITY_INSERT FireAccidents OFF;";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@SNO", nextSNO);
                    cmd.Parameters.AddWithValue("@AccidentDate", DateTime.Parse(accidentDate));
                    cmd.Parameters.AddWithValue("@Location", location);
                    cmd.Parameters.AddWithValue("@Cause", string.IsNullOrEmpty(cause) ? (object)DBNull.Value : cause);
                    cmd.Parameters.AddWithValue("@Injuries", string.IsNullOrEmpty(injuries) ? (object)DBNull.Value : injuries);
                    cmd.Parameters.AddWithValue("@Fatalities", string.IsNullOrEmpty(fatalities) ? (object)DBNull.Value : fatalities);
                    cmd.Parameters.AddWithValue("@DamageDescription", string.IsNullOrEmpty(damage) ? (object)DBNull.Value : damage);
                    cmd.Parameters.AddWithValue("@ActionsTaken", string.IsNullOrEmpty(actions) ? (object)DBNull.Value : actions);
                    cmd.Parameters.AddWithValue("@Remarks", string.IsNullOrEmpty(remarks) ? (object)DBNull.Value : remarks);
                    cmd.Parameters.AddWithValue("@StationID", stationID);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Fire accident record saved successfully!');", true);
                pnlForm.Visible = false;
                ClearForm();
                BindGrid();
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Error: " + ex.Message.Replace("'", "\\'") + "');", true);
            }
        }

        private int GetNextSequentialSNO()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT SNO FROM FireAccidents ORDER BY SNO";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                List<int> used = new List<int>();
                while (reader.Read()) used.Add((int)reader["SNO"]);
                reader.Close();

                int next = 1;
                while (used.Contains(next)) next++;
                return next;
            }
        }

        private void ClearForm()
        {
            txtAccidentDate.Text = txtLocation.Text = txtCause.Text = txtInjuries.Text =
            txtFatalities.Text = txtDamage.Text = txtActions.Text = txtRemarks.Text = "";

            if (Session["StationName"] != null)
                txtStation.Text = Session["StationName"].ToString();
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Label lblSerial = (Label)e.Row.FindControl("lblSerial");
                if (lblSerial != null)
                    lblSerial.Text = (e.Row.RowIndex + 1).ToString();
            }
        }

        protected void GridView1_RowEditing(object sender, GridViewEditEventArgs e)
        {
            GridView1.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        protected void GridView1_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            GridView1.EditIndex = -1;
            BindGrid();
        }

        protected void GridView1_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int sno = Convert.ToInt32(GridView1.DataKeys[e.RowIndex].Value);
                GridViewRow row = GridView1.Rows[e.RowIndex];

                string accidentDate = ((TextBox)row.Cells[2].Controls[0]).Text;
                string location = ((TextBox)row.Cells[3].Controls[0]).Text;
                string cause = ((TextBox)row.Cells[4].Controls[0]).Text;
                string injuries = ((TextBox)row.Cells[5].Controls[0]).Text;
                string fatalities = ((TextBox)row.Cells[6].Controls[0]).Text;
                string damage = ((TextBox)row.Cells[7].Controls[0]).Text;
                string actions = ((TextBox)row.Cells[8].Controls[0]).Text;
                string remarks = ((TextBox)row.Cells[9].Controls[0]).Text;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"UPDATE FireAccidents SET AccidentDate=@AccidentDate, Location=@Location, Cause=@Cause,
                                     Injuries=@Injuries, Fatalities=@Fatalities, DamageDescription=@DamageDescription,
                                     ActionsTaken=@ActionsTaken, Remarks=@Remarks WHERE SNO=@SNO";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@AccidentDate", DateTime.Parse(accidentDate));
                    cmd.Parameters.AddWithValue("@Location", location);
                    cmd.Parameters.AddWithValue("@Cause", string.IsNullOrEmpty(cause) ? (object)DBNull.Value : cause);
                    cmd.Parameters.AddWithValue("@Injuries", string.IsNullOrEmpty(injuries) ? (object)DBNull.Value : injuries);
                    cmd.Parameters.AddWithValue("@Fatalities", string.IsNullOrEmpty(fatalities) ? (object)DBNull.Value : fatalities);
                    cmd.Parameters.AddWithValue("@DamageDescription", string.IsNullOrEmpty(damage) ? (object)DBNull.Value : damage);
                    cmd.Parameters.AddWithValue("@ActionsTaken", string.IsNullOrEmpty(actions) ? (object)DBNull.Value : actions);
                    cmd.Parameters.AddWithValue("@Remarks", string.IsNullOrEmpty(remarks) ? (object)DBNull.Value : remarks);
                    cmd.Parameters.AddWithValue("@SNO", sno);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                GridView1.EditIndex = -1;
                BindGrid();
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Record updated successfully!');", true);
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Update error: " + ex.Message.Replace("'", "\\'") + "');", true);
            }
        }

        protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                int sno = Convert.ToInt32(GridView1.DataKeys[e.RowIndex].Value);
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("DELETE FROM FireAccidents WHERE SNO=@SNO", conn);
                    cmd.Parameters.AddWithValue("@SNO", sno);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    ReorganizeSNOValues();
                }
                GridView1.EditIndex = -1;
                BindGrid();
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Record deleted and SNO reorganized!');", true);
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Delete error: " + ex.Message.Replace("'", "\\'") + "');", true);
            }
        }

        private void ReorganizeSNOValues()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string selectQuery = "SELECT SNO FROM FireAccidents ORDER BY SNO";
                SqlCommand selectCmd = new SqlCommand(selectQuery, conn);
                List<int> currentSNOs = new List<int>();
                SqlDataReader reader = selectCmd.ExecuteReader();
                while (reader.Read()) currentSNOs.Add((int)reader["SNO"]);
                reader.Close();

                for (int i = 0; i < currentSNOs.Count; i++)
                {
                    int current = currentSNOs[i];
                    int newSNO = i + 1;
                    if (current != newSNO)
                    {
                        SqlCommand on = new SqlCommand("SET IDENTITY_INSERT FireAccidents ON", conn);
                        on.ExecuteNonQuery();

                        SqlCommand update = new SqlCommand("UPDATE FireAccidents SET SNO = @New WHERE SNO = @Old", conn);
                        update.Parameters.AddWithValue("@New", newSNO);
                        update.Parameters.AddWithValue("@Old", current);
                        update.ExecuteNonQuery();

                        SqlCommand off = new SqlCommand("SET IDENTITY_INSERT FireAccidents OFF", conn);
                        off.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}