using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;

namespace FireProtectionPortal
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PopulateRoles();
                PopulateStations();

                // Initial hide/show based on default selection
                ScriptManager.RegisterStartupScript(this, GetType(), "initRole",
                    "document.addEventListener('DOMContentLoaded', function() { handleRoleChange(); });", true);
            }
        }

        private void PopulateRoles()
        {
            string connStr = ConfigurationManager.ConnectionStrings["FireProtectionDBConnection"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT DISTINCT Role FROM Users WHERE Role IS NOT NULL AND Role != '' ORDER BY Role";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                ddlRole.DataSource = reader;
                ddlRole.DataTextField = "Role";
                ddlRole.DataValueField = "Role";
                ddlRole.DataBind();
                conn.Close();
            }
            ddlRole.Items.Insert(0, new System.Web.UI.WebControls.ListItem("--Select Role--", ""));
        }

        private void PopulateStations()
        {
            string connStr = ConfigurationManager.ConnectionStrings["FireProtectionDBConnection"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT StationID, StationName FROM Stations ORDER BY StationName";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                ddlStation.DataSource = reader;
                ddlStation.DataTextField = "StationName";
                ddlStation.DataValueField = "StationID";
                ddlStation.DataBind();
                conn.Close();
            }
            ddlStation.Items.Insert(0, new System.Web.UI.WebControls.ListItem("--Select Station--", ""));
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            ClearErrorMessages();

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();
            string role = ddlRole.SelectedValue;

            if (string.IsNullOrEmpty(role))
            {
                ShowError(lblRoleError, "Please select a role.");
                return;
            }

            if (string.IsNullOrEmpty(username))
            {
                ShowError(lblUserError, "Username is required.");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError(lblPassError, "Password is required.");
                return;
            }

            int? stationId = null;
            if (role != "Admin")
            {
                if (string.IsNullOrEmpty(ddlStation.SelectedValue))
                {
                    lblMessage.Text = "Please select a station.";
                    lblMessage.CssClass = "error-message show";
                    return;
                }
                stationId = int.Parse(ddlStation.SelectedValue);
            }

            string connStr = ConfigurationManager.ConnectionStrings["FireProtectionDBConnection"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = role == "Admin"
                    ? @"SELECT COUNT(*) FROM Users WHERE Username=@Username AND PasswordHash=@PasswordHash AND Role=@Role"
                    : @"SELECT COUNT(*) FROM Users WHERE Username=@Username AND PasswordHash=@PasswordHash AND Role=@Role
                        AND (StationID=@StationID OR (@StationID IS NULL AND StationID IS NULL))";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@PasswordHash", password); // Note: In real app, use hashed password!
                cmd.Parameters.AddWithValue("@Role", role);
                if (role != "Admin")
                    cmd.Parameters.AddWithValue("@StationID", (object)stationId ?? DBNull.Value);

                conn.Open();
                int count = (int)cmd.ExecuteScalar();

                if (count > 0)
                {
                    Session["Username"] = username;
                    Session["Role"] = role;
                    Session["StationID"] = stationId;
                    Session["StationName"] = stationId.HasValue ? ddlStation.SelectedItem.Text : null;

                    Response.Redirect("~/Dashboard.aspx");
                }
                else
                {
                    lblMessage.Text = "Invalid credentials. Please try again.";
                    lblMessage.CssClass = "error-message show";
                }
            }
        }

        private void ClearErrorMessages()
        {
            lblMessage.Text = "";
            lblMessage.CssClass = "error-message";
            lblRoleError.Text = "";
            lblUserError.Text = "";
            lblPassError.Text = "";
        }

        private void ShowError(System.Web.UI.WebControls.Label lbl, string message)
        {
            lbl.Text = message;
            lbl.CssClass = "error-message show";
        }
    }
}