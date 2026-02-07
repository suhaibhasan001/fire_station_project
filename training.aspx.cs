using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fireportalopenPIA
{
    public partial class training : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["FireProtectionDBConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Check if user is logged in and has station information
                if (Session["StationName"] != null)
                {
                    // Pre-populate station field with logged-in user's station
                    txtStation.Text = Session["StationName"].ToString();

                    // Make station field read-only (locked)
                    txtStation.ReadOnly = true;
                    txtStation.CssClass += " readonly-field";

                    // Load existing training records
                    BindGrid();
                }
                else
                {
                    // If no station in session, redirect back to login
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
                    // Get current user's station ID for filtering
                    int? currentStationID = Session["StationID"] as int?;
                    string role = Session["Role"] as string;

                    string query;
                    if (role == "Admin")
                    {
                        // Admin can see all records with station names
                        query = @"SELECT s.SNO, s.Name, s.PNo_SNo, s.Designation, s.Department, 
                                        s.Section, st.StationName, s.TrainingDate, s.ValidDate, s.Remarks
                                 FROM SafetyTrainings s
                                 INNER JOIN Stations st ON s.StationID = st.StationID
                                 ORDER BY s.SNO";
                    }
                    else
                    {
                        // Other users can only see their station's records
                        query = @"SELECT s.SNO, s.Name, s.PNo_SNo, s.Designation, s.Department, 
                                        s.Section, st.StationName, s.TrainingDate, s.ValidDate, s.Remarks
                                 FROM SafetyTrainings s
                                 INNER JOIN Stations st ON s.StationID = st.StationID
                                 WHERE s.StationID = @StationID
                                 ORDER BY s.SNO";
                    }

                    SqlCommand cmd = new SqlCommand(query, conn);
                    if (role != "Admin" && currentStationID.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@StationID", currentStationID.Value);
                    }

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    GridView1.DataSource = dt;
                    GridView1.DataBind();
                }
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Error loading training records: " + ex.Message.Replace("'", "\\'") + "');", true);
            }
        }

        protected void btnAddDetails_Click(object sender, EventArgs e)
        {
            // Show form panel when Add Details button clicked
            pnlForm.Visible = true;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            // Hide form and clear fields
            pnlForm.Visible = false;
            ClearForm();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Get form data
                string name = txtName.Text.Trim();
                string pno = txtPno.Text.Trim();
                string designation = txtDesignation.Text.Trim();
                string department = txtDepartment.Text.Trim();
                string section = txtSection.Text.Trim();
                string trainingDate = txtTrainingDate.Text.Trim();
                string validDate = txtValidDate.Text.Trim();
                string remarks = txtRemarks.Text.Trim();

                // Get StationID from session
                int? stationID = Session["StationID"] as int?;

                // Basic validation
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(designation) || stationID == null)
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Please fill required fields: Name, Designation');", true);
                    return;
                }

                // Get the next sequential SNO
                int nextSNO = GetNextSequentialSNO();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Use INSERT with explicit SNO value
                    string query = @"SET IDENTITY_INSERT SafetyTrainings ON;
                                   INSERT INTO SafetyTrainings (SNO, Name, PNo_SNo, Designation, Department, Section, StationID, TrainingDate, ValidDate, Remarks)
                                   VALUES (@SNO, @Name, @PNo_SNo, @Designation, @Department, @Section, @StationID, @TrainingDate, @ValidDate, @Remarks);
                                   SET IDENTITY_INSERT SafetyTrainings OFF;";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@SNO", nextSNO);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@PNo_SNo", string.IsNullOrEmpty(pno) ? (object)DBNull.Value : pno);
                    cmd.Parameters.AddWithValue("@Designation", designation);
                    cmd.Parameters.AddWithValue("@Department", string.IsNullOrEmpty(department) ? (object)DBNull.Value : department);
                    cmd.Parameters.AddWithValue("@Section", string.IsNullOrEmpty(section) ? (object)DBNull.Value : section);
                    cmd.Parameters.AddWithValue("@StationID", stationID);
                    cmd.Parameters.AddWithValue("@TrainingDate", string.IsNullOrEmpty(trainingDate) ? (object)DBNull.Value : DateTime.Parse(trainingDate));
                    cmd.Parameters.AddWithValue("@ValidDate", string.IsNullOrEmpty(validDate) ? (object)DBNull.Value : DateTime.Parse(validDate));
                    cmd.Parameters.AddWithValue("@Remarks", string.IsNullOrEmpty(remarks) ? (object)DBNull.Value : remarks);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Training record saved successfully with SNO: " + nextSNO + "');", true);

                // Hide form and clear fields
                pnlForm.Visible = false;
                ClearForm();

                // Refresh data display
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
                // Get all existing SNO values and find the next sequential number
                string query = "SELECT SNO FROM SafetyTrainings ORDER BY SNO";
                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                // Create a list to track used numbers
                System.Collections.Generic.List<int> usedNumbers = new System.Collections.Generic.List<int>();

                while (reader.Read())
                {
                    usedNumbers.Add((int)reader["SNO"]);
                }
                reader.Close();
                conn.Close();

                // Find the next available sequential number starting from 1
                int nextNumber = 1;
                while (usedNumbers.Contains(nextNumber))
                {
                    nextNumber++;
                }

                return nextNumber;
            }
        }

        private void ClearForm()
        {
            // Don't clear station field as it should remain locked
            txtDepartment.Text = "";
            txtName.Text = "";
            txtSection.Text = "";
            txtPno.Text = "";
            txtTrainingDate.Text = "";
            txtDesignation.Text = "";
            txtValidDate.Text = "";
            txtRemarks.Text = "";

            // Re-populate station field after clearing
            if (Session["StationName"] != null)
            {
                txtStation.Text = Session["StationName"].ToString();
                txtStation.ReadOnly = true;
            }
        }

        // ================= GRIDVIEW EDIT / UPDATE / DELETE ==================

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

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Set sequential serial number (starting from 1)
                Label lblSerial = (Label)e.Row.FindControl("lblSerial");
                if (lblSerial != null)
                {
                    lblSerial.Text = (e.Row.RowIndex + 1).ToString();
                }
            }
        }

        protected void GridView1_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int sno = Convert.ToInt32(GridView1.DataKeys[e.RowIndex].Value);

                GridViewRow row = GridView1.Rows[e.RowIndex];
                string name = ((TextBox)row.Cells[1].Controls[0]).Text;
                string pnoSno = ((TextBox)row.Cells[2].Controls[0]).Text;
                string designation = ((TextBox)row.Cells[3].Controls[0]).Text;
                string department = ((TextBox)row.Cells[4].Controls[0]).Text;
                string section = ((TextBox)row.Cells[5].Controls[0]).Text;
                string trainingDate = ((TextBox)row.Cells[7].Controls[0]).Text;
                string validDate = ((TextBox)row.Cells[8].Controls[0]).Text;
                string remarks = ((TextBox)row.Cells[9].Controls[0]).Text;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"UPDATE SafetyTrainings 
                                     SET Name=@Name, PNo_SNo=@PNo_SNo, Designation=@Designation, 
                                         Department=@Department, Section=@Section, 
                                         TrainingDate=@TrainingDate, ValidDate=@ValidDate, Remarks=@Remarks
                                     WHERE SNO=@SNO";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@PNo_SNo", string.IsNullOrEmpty(pnoSno) ? (object)DBNull.Value : pnoSno);
                    cmd.Parameters.AddWithValue("@Designation", designation);
                    cmd.Parameters.AddWithValue("@Department", string.IsNullOrEmpty(department) ? (object)DBNull.Value : department);
                    cmd.Parameters.AddWithValue("@Section", string.IsNullOrEmpty(section) ? (object)DBNull.Value : section);
                    cmd.Parameters.AddWithValue("@TrainingDate", string.IsNullOrEmpty(trainingDate) ? (object)DBNull.Value : DateTime.Parse(trainingDate));
                    cmd.Parameters.AddWithValue("@ValidDate", string.IsNullOrEmpty(validDate) ? (object)DBNull.Value : DateTime.Parse(validDate));
                    cmd.Parameters.AddWithValue("@Remarks", string.IsNullOrEmpty(remarks) ? (object)DBNull.Value : remarks);
                    cmd.Parameters.AddWithValue("@SNO", sno);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Record updated successfully!');", true);

                GridView1.EditIndex = -1;
                BindGrid();
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Error updating record: " + ex.Message.Replace("'", "\\'") + "');", true);
            }
        }

        protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                int sno = Convert.ToInt32(GridView1.DataKeys[e.RowIndex].Value);

                // Delete the record from database
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM SafetyTrainings WHERE SNO=@SNO";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@SNO", sno);

                    conn.Open();
                    int result = cmd.ExecuteNonQuery();
                    conn.Close();

                    if (result > 0)
                    {
                        // After successful deletion, reorganize SNO values
                        ReorganizeSNOValues();

                        // Show success message and refresh grid
                        ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Record deleted successfully! SNO numbers reorganized.');", true);

                        // Reset edit index and refresh grid
                        GridView1.EditIndex = -1;
                        BindGrid();
                    }
                    else
                    {
                        ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Failed to delete record.');", true);
                    }
                }
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Error deleting record: " + ex.Message.Replace("'", "\\'") + "');", true);
            }
        }

        private void ReorganizeSNOValues()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Get all records ordered by current SNO
                string selectQuery = "SELECT SNO FROM SafetyTrainings ORDER BY SNO";
                SqlCommand selectCmd = new SqlCommand(selectQuery, conn);

                System.Collections.Generic.List<int> currentSNOs = new System.Collections.Generic.List<int>();
                SqlDataReader reader = selectCmd.ExecuteReader();

                while (reader.Read())
                {
                    currentSNOs.Add((int)reader["SNO"]);
                }
                reader.Close();

                // Now update each record to have sequential SNO (1, 2, 3...)
                for (int i = 0; i < currentSNOs.Count; i++)
                {
                    int currentSNO = currentSNOs[i];
                    int newSNO = i + 1;

                    if (currentSNO != newSNO)
                    {
                        // Turn on IDENTITY_INSERT to allow manual SNO update
                        string identityOnQuery = "SET IDENTITY_INSERT SafetyTrainings ON";
                        SqlCommand identityOnCmd = new SqlCommand(identityOnQuery, conn);
                        identityOnCmd.ExecuteNonQuery();

                        // Update the SNO to sequential number
                        string updateQuery = "UPDATE SafetyTrainings SET SNO = @NewSNO WHERE SNO = @CurrentSNO";
                        SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                        updateCmd.Parameters.AddWithValue("@NewSNO", newSNO);
                        updateCmd.Parameters.AddWithValue("@CurrentSNO", currentSNO);
                        updateCmd.ExecuteNonQuery();

                        // Turn off IDENTITY_INSERT
                        string identityOffQuery = "SET IDENTITY_INSERT SafetyTrainings OFF";
                        SqlCommand identityOffCmd = new SqlCommand(identityOffQuery, conn);
                        identityOffCmd.ExecuteNonQuery();
                    }
                }

                conn.Close();
            }
        }
    }
}