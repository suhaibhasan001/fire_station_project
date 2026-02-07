using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;

namespace Fire_Station_project
{
    public partial class Alarmsystem : System.Web.UI.Page
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
                        query = @"SELECT als.SN, st.StationName, als.Area, als.SystemSpecs, als.CallPoints, 
                                         als.Sounder, als.Detectors, als.InstallationYear, als.LastMaintenance, 
                                         als.LastTestDate, als.Remarks
                                  FROM AlarmSystems als
                                  INNER JOIN Stations st ON als.StationID = st.StationID
                                  ORDER BY als.SN";
                    }
                    else
                    {
                        query = @"SELECT als.SN, st.StationName, als.Area, als.SystemSpecs, als.CallPoints, 
                                         als.Sounder, als.Detectors, als.InstallationYear, als.LastMaintenance, 
                                         als.LastTestDate, als.Remarks
                                  FROM AlarmSystems als
                                  INNER JOIN Stations st ON als.StationID = st.StationID
                                  WHERE als.StationID = @StationID
                                  ORDER BY als.SN";
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
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Error loading alarm system records: " + ex.Message.Replace("'", "\\'") + "');", true);
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
                string area = txtArea.Text.Trim();
                string specs = txtSystemSpecs.Text.Trim();
                int callPoints = string.IsNullOrEmpty(txtCallPoints.Text) ? 0 : int.Parse(txtCallPoints.Text);
                int sounder = string.IsNullOrEmpty(txtSounder.Text) ? 0 : int.Parse(txtSounder.Text);
                int detectors = string.IsNullOrEmpty(txtDetectors.Text) ? 0 : int.Parse(txtDetectors.Text);
                int installYear = string.IsNullOrEmpty(txtInstallationYear.Text) ? 0 : int.Parse(txtInstallationYear.Text);
                string lastMaint = txtLastMaintenance.Text.Trim();
                string lastTest = txtLastTestDate.Text.Trim();
                string remarks = txtRemarks.Text.Trim();

                int? stationID = Session["StationID"] as int?;

                if (string.IsNullOrEmpty(area) || stationID == null)
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Please fill required fields: Area');", true);
                    return;
                }

                int nextSN = GetNextSequentialSN();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SET IDENTITY_INSERT AlarmSystems ON;
                                     INSERT INTO AlarmSystems (SN, Area, SystemSpecs, CallPoints, Sounder, Detectors,
                                                               InstallationYear, LastMaintenance, LastTestDate, Remarks, StationID)
                                     VALUES (@SN, @Area, @SystemSpecs, @CallPoints, @Sounder, @Detectors,
                                             @InstallationYear, @LastMaintenance, @LastTestDate, @Remarks, @StationID);
                                     SET IDENTITY_INSERT AlarmSystems OFF;";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@SN", nextSN);
                    cmd.Parameters.AddWithValue("@Area", area);
                    cmd.Parameters.AddWithValue("@SystemSpecs", string.IsNullOrEmpty(specs) ? (object)DBNull.Value : specs);
                    cmd.Parameters.AddWithValue("@CallPoints", callPoints);
                    cmd.Parameters.AddWithValue("@Sounder", sounder);
                    cmd.Parameters.AddWithValue("@Detectors", detectors);
                    cmd.Parameters.AddWithValue("@InstallationYear", installYear == 0 ? (object)DBNull.Value : installYear);
                    cmd.Parameters.AddWithValue("@LastMaintenance", string.IsNullOrEmpty(lastMaint) ? (object)DBNull.Value : DateTime.Parse(lastMaint));
                    cmd.Parameters.AddWithValue("@LastTestDate", string.IsNullOrEmpty(lastTest) ? (object)DBNull.Value : DateTime.Parse(lastTest));
                    cmd.Parameters.AddWithValue("@Remarks", string.IsNullOrEmpty(remarks) ? (object)DBNull.Value : remarks);
                    cmd.Parameters.AddWithValue("@StationID", stationID);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Alarm system record saved successfully!');", true);
                pnlForm.Visible = false;
                ClearForm();
                BindGrid();
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Error: " + ex.Message.Replace("'", "\\'") + "');", true);
            }
        }

        private int GetNextSequentialSN()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT SN FROM AlarmSystems ORDER BY SN";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                List<int> used = new List<int>();
                while (reader.Read()) used.Add((int)reader["SN"]);
                reader.Close();

                int next = 1;
                while (used.Contains(next)) next++;
                return next;
            }
        }

        private void ClearForm()
        {
            txtArea.Text = txtSystemSpecs.Text = txtCallPoints.Text = txtSounder.Text =
            txtDetectors.Text = txtInstallationYear.Text = txtLastMaintenance.Text =
            txtLastTestDate.Text = txtRemarks.Text = "";

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
                int sn = Convert.ToInt32(GridView1.DataKeys[e.RowIndex].Value);
                GridViewRow row = GridView1.Rows[e.RowIndex];

                string area = ((TextBox)row.Cells[2].Controls[0]).Text;
                string specs = ((TextBox)row.Cells[3].Controls[0]).Text;
                string callPointsStr = ((TextBox)row.Cells[4].Controls[0]).Text;
                string sounderStr = ((TextBox)row.Cells[5].Controls[0]).Text;
                string detectorsStr = ((TextBox)row.Cells[6].Controls[0]).Text;
                string installYearStr = ((TextBox)row.Cells[7].Controls[0]).Text;
                string lastMaint = ((TextBox)row.Cells[8].Controls[0]).Text;
                string lastTest = ((TextBox)row.Cells[9].Controls[0]).Text;
                string remarks = ((TextBox)row.Cells[10].Controls[0]).Text;

                int callPoints = string.IsNullOrEmpty(callPointsStr) ? 0 : int.Parse(callPointsStr);
                int sounder = string.IsNullOrEmpty(sounderStr) ? 0 : int.Parse(sounderStr);
                int detectors = string.IsNullOrEmpty(detectorsStr) ? 0 : int.Parse(detectorsStr);
                int installYear = string.IsNullOrEmpty(installYearStr) ? 0 : int.Parse(installYearStr);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"UPDATE AlarmSystems SET Area=@Area, SystemSpecs=@SystemSpecs, CallPoints=@CallPoints,
                                     Sounder=@Sounder, Detectors=@Detectors, InstallationYear=@InstallationYear,
                                     LastMaintenance=@LastMaintenance, LastTestDate=@LastTestDate, Remarks=@Remarks WHERE SN=@SN";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Area", area);
                    cmd.Parameters.AddWithValue("@SystemSpecs", string.IsNullOrEmpty(specs) ? (object)DBNull.Value : specs);
                    cmd.Parameters.AddWithValue("@CallPoints", callPoints);
                    cmd.Parameters.AddWithValue("@Sounder", sounder);
                    cmd.Parameters.AddWithValue("@Detectors", detectors);
                    cmd.Parameters.AddWithValue("@InstallationYear", installYear == 0 ? (object)DBNull.Value : installYear);
                    cmd.Parameters.AddWithValue("@LastMaintenance", string.IsNullOrEmpty(lastMaint) ? (object)DBNull.Value : DateTime.Parse(lastMaint));
                    cmd.Parameters.AddWithValue("@LastTestDate", string.IsNullOrEmpty(lastTest) ? (object)DBNull.Value : DateTime.Parse(lastTest));
                    cmd.Parameters.AddWithValue("@Remarks", string.IsNullOrEmpty(remarks) ? (object)DBNull.Value : remarks);
                    cmd.Parameters.AddWithValue("@SN", sn);

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
                int sn = Convert.ToInt32(GridView1.DataKeys[e.RowIndex].Value);
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("DELETE FROM AlarmSystems WHERE SN=@SN", conn);
                    cmd.Parameters.AddWithValue("@SN", sn);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    ReorganizeSNValues();
                }
                GridView1.EditIndex = -1;
                BindGrid();
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Record deleted and SN reorganized!');", true);
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Delete error: " + ex.Message.Replace("'", "\\'") + "');", true);
            }
        }

        private void ReorganizeSNValues()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string selectQuery = "SELECT SN FROM AlarmSystems ORDER BY SN";
                SqlCommand selectCmd = new SqlCommand(selectQuery, conn);
                List<int> currentSNs = new List<int>();
                SqlDataReader reader = selectCmd.ExecuteReader();
                while (reader.Read()) currentSNs.Add((int)reader["SN"]);
                reader.Close();

                for (int i = 0; i < currentSNs.Count; i++)
                {
                    int current = currentSNs[i];
                    int newSN = i + 1;
                    if (current != newSN)
                    {
                        SqlCommand on = new SqlCommand("SET IDENTITY_INSERT AlarmSystems ON", conn);
                        on.ExecuteNonQuery();

                        SqlCommand update = new SqlCommand("UPDATE AlarmSystems SET SN = @New WHERE SN = @Old", conn);
                        update.Parameters.AddWithValue("@New", newSN);
                        update.Parameters.AddWithValue("@Old", current);
                        update.ExecuteNonQuery();

                        SqlCommand off = new SqlCommand("SET IDENTITY_INSERT AlarmSystems OFF", conn);
                        off.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}