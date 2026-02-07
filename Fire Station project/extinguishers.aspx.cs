using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;

namespace Fire_Station_project
{
    public partial class extinguishers : System.Web.UI.Page
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
                        query = @"SELECT fe.SNO, st.StationName, fe.ExtinguisherNo, fe.Type, fe.Location,
                                         fe.LastInspection, fe.Status, fe.InspectionDueDate, fe.LastMaintenance, fe.Remarks
                                  FROM FireExtinguishers fe
                                  INNER JOIN Stations st ON fe.StationID = st.StationID
                                  ORDER BY fe.SNO";
                    }
                    else
                    {
                        query = @"SELECT fe.SNO, st.StationName, fe.ExtinguisherNo, fe.Type, fe.Location,
                                         fe.LastInspection, fe.Status, fe.InspectionDueDate, fe.LastMaintenance, fe.Remarks
                                  FROM FireExtinguishers fe
                                  INNER JOIN Stations st ON fe.StationID = st.StationID
                                  WHERE fe.StationID = @StationID
                                  ORDER BY fe.SNO";
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
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Error loading extinguisher records: " + ex.Message.Replace("'", "\\'") + "');", true);
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
                string extNo = txtExtinguisherNo.Text.Trim();
                string type = txtType.Text.Trim();
                string location = txtLocation.Text.Trim();
                string lastIns = txtLastInspection.Text.Trim();
                string status = txtStatus.Text.Trim();
                string dueDate = txtInspectionDueDate.Text.Trim();
                string lastMaint = txtLastMaintenance.Text.Trim();
                string remarks = txtRemarks.Text.Trim();

                int? stationID = Session["StationID"] as int?;

                if (string.IsNullOrEmpty(extNo) || string.IsNullOrEmpty(type) || string.IsNullOrEmpty(location) || stationID == null)
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Please fill required fields: Extinguisher No, Type, Location');", true);
                    return;
                }

                int nextSNO = GetNextSequentialSNO();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SET IDENTITY_INSERT FireExtinguishers ON;
                                     INSERT INTO FireExtinguishers (SNO, ExtinguisherNo, Type, Location, LastInspection, Status,
                                                                   InspectionDueDate, LastMaintenance, Remarks, StationID)
                                     VALUES (@SNO, @ExtinguisherNo, @Type, @Location, @LastInspection, @Status,
                                             @InspectionDueDate, @LastMaintenance, @Remarks, @StationID);
                                     SET IDENTITY_INSERT FireExtinguishers OFF;";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@SNO", nextSNO);
                    cmd.Parameters.AddWithValue("@ExtinguisherNo", extNo);
                    cmd.Parameters.AddWithValue("@Type", type);
                    cmd.Parameters.AddWithValue("@Location", location);
                    cmd.Parameters.AddWithValue("@LastInspection", string.IsNullOrEmpty(lastIns) ? (object)DBNull.Value : DateTime.Parse(lastIns));
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(status) ? (object)DBNull.Value : status);
                    cmd.Parameters.AddWithValue("@InspectionDueDate", string.IsNullOrEmpty(dueDate) ? (object)DBNull.Value : DateTime.Parse(dueDate));
                    cmd.Parameters.AddWithValue("@LastMaintenance", string.IsNullOrEmpty(lastMaint) ? (object)DBNull.Value : DateTime.Parse(lastMaint));
                    cmd.Parameters.AddWithValue("@Remarks", string.IsNullOrEmpty(remarks) ? (object)DBNull.Value : remarks);
                    cmd.Parameters.AddWithValue("@StationID", stationID);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Extinguisher record saved successfully!');", true);
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
                string query = "SELECT SNO FROM FireExtinguishers ORDER BY SNO";
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
            txtExtinguisherNo.Text = txtType.Text = txtLocation.Text = txtLastInspection.Text =
            txtStatus.Text = txtInspectionDueDate.Text = txtLastMaintenance.Text = txtRemarks.Text = "";

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

                string extNo = ((TextBox)row.Cells[2].Controls[0]).Text;
                string type = ((TextBox)row.Cells[3].Controls[0]).Text;
                string location = ((TextBox)row.Cells[4].Controls[0]).Text;
                string lastIns = ((TextBox)row.Cells[5].Controls[0]).Text;
                string status = ((TextBox)row.Cells[6].Controls[0]).Text;
                string dueDate = ((TextBox)row.Cells[7].Controls[0]).Text;
                string lastMaint = ((TextBox)row.Cells[8].Controls[0]).Text;
                string remarks = ((TextBox)row.Cells[9].Controls[0]).Text;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"UPDATE FireExtinguishers SET ExtinguisherNo=@ExtinguisherNo, Type=@Type, Location=@Location,
                                     LastInspection=@LastInspection, Status=@Status, InspectionDueDate=@InspectionDueDate,
                                     LastMaintenance=@LastMaintenance, Remarks=@Remarks WHERE SNO=@SNO";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ExtinguisherNo", extNo);
                    cmd.Parameters.AddWithValue("@Type", type);
                    cmd.Parameters.AddWithValue("@Location", location);
                    cmd.Parameters.AddWithValue("@LastInspection", string.IsNullOrEmpty(lastIns) ? (object)DBNull.Value : DateTime.Parse(lastIns));
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(status) ? (object)DBNull.Value : status);
                    cmd.Parameters.AddWithValue("@InspectionDueDate", string.IsNullOrEmpty(dueDate) ? (object)DBNull.Value : DateTime.Parse(dueDate));
                    cmd.Parameters.AddWithValue("@LastMaintenance", string.IsNullOrEmpty(lastMaint) ? (object)DBNull.Value : DateTime.Parse(lastMaint));
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
                    SqlCommand cmd = new SqlCommand("DELETE FROM FireExtinguishers WHERE SNO=@SNO", conn);
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
                string selectQuery = "SELECT SNO FROM FireExtinguishers ORDER BY SNO";
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
                        SqlCommand on = new SqlCommand("SET IDENTITY_INSERT FireExtinguishers ON", conn);
                        on.ExecuteNonQuery();

                        SqlCommand update = new SqlCommand("UPDATE FireExtinguishers SET SNO = @New WHERE SNO = @Old", conn);
                        update.Parameters.AddWithValue("@New", newSNO);
                        update.Parameters.AddWithValue("@Old", current);
                        update.ExecuteNonQuery();

                        SqlCommand off = new SqlCommand("SET IDENTITY_INSERT FireExtinguishers OFF", conn);
                        off.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}