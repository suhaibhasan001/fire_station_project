<%@ Page Language="C#"
    MasterPageFile="~/Site.master"
    AutoEventWireup="true"
    CodeBehind="extinguishers.aspx.cs"
    Inherits="Fire_Station_project.extinguishers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .readonly-field {
            background-color: #f8f9fa !important;
            cursor: not-allowed;
        }
        .table th {
            background-color: #28a745 !important;
            color: white !important;
        }
    </style>

    <!-- TOP BAR -->
    <nav class="navbar navbar-main navbar-expand-lg px-4 shadow-sm bg-white mb-3">
        <h4 class="mb-0">Fire Extinguishers Records</h4>
        <div class="ms-auto">
            <asp:HyperLink ID="lnkDashboard" runat="server"
                NavigateUrl="dashboard.aspx"
                CssClass="btn btn-outline-success btn-sm">
                Back to Dashboard
            </asp:HyperLink>
        </div>
    </nav>

    <div class="container-fluid py-2">
        <asp:Button ID="btnAddDetails" runat="server"
            CssClass="btn btn-success mb-3"
            Text="+ Add Details"
            OnClick="btnAddDetails_Click" />

        <!-- ADD / EDIT PANEL -->
        <asp:Panel ID="pnlForm" runat="server" Visible="false"
            CssClass="card p-4 mb-4">
            <div class="row g-3">
                <div class="col-md-6 mb-3">
                    <label class="form-label">Station</label>
                    <asp:TextBox ID="txtStation" runat="server"
                        CssClass="form-control readonly-field"
                        ReadOnly="true" />
                </div>
                <div class="col-md-6 mb-3">
                    <label class="form-label">Extinguisher No</label>
                    <asp:TextBox ID="txtExtinguisherNo" runat="server"
                        CssClass="form-control" />
                </div>
                <div class="col-md-6 mb-3">
                    <label class="form-label">Type</label>
                    <asp:TextBox ID="txtType" runat="server"
                        CssClass="form-control" />
                </div>
                <div class="col-md-6 mb-3">
                    <label class="form-label">Location</label>
                    <asp:TextBox ID="txtLocation" runat="server"
                        CssClass="form-control" />
                </div>
                <div class="col-md-6 mb-3">
                    <label class="form-label">Last Inspection</label>
                    <asp:TextBox ID="txtLastInspection" runat="server"
                        CssClass="form-control"
                        TextMode="Date" />
                </div>
                <div class="col-md-6 mb-3">
                    <label class="form-label">Status</label>
                    <asp:TextBox ID="txtStatus" runat="server"
                        CssClass="form-control" />
                </div>
                <div class="col-md-6 mb-3">
                    <label class="form-label">Inspection Due Date</label>
                    <asp:TextBox ID="txtInspectionDueDate" runat="server"
                        CssClass="form-control"
                        TextMode="Date" />
                </div>
                <div class="col-md-6 mb-3">
                    <label class="form-label">Last Maintenance</label>
                    <asp:TextBox ID="txtLastMaintenance" runat="server"
                        CssClass="form-control"
                        TextMode="Date" />
                </div>
                <div class="col-12 mb-3">
                    <label class="form-label">Remarks</label>
                    <asp:TextBox ID="txtRemarks" runat="server"
                        CssClass="form-control"
                        TextMode="MultiLine" Rows="3" />
                </div>
            </div>
            <div class="text-end">
                <asp:Button ID="btnCancel" runat="server"
                    CssClass="btn btn-secondary"
                    Text="Cancel"
                    OnClick="btnCancel_Click" />
                <asp:Button ID="btnSave" runat="server"
                    CssClass="btn btn-success ms-2"
                    Text="Save"
                    OnClick="btnSave_Click" />
            </div>
        </asp:Panel>

        <!-- GRID -->
        <div class="card">
            <div class="table-responsive p-3">
                <asp:GridView ID="GridView1" runat="server"
                    AutoGenerateColumns="false"
                    CssClass="table table-bordered table-hover"
                    DataKeyNames="SNO"
                    OnRowEditing="GridView1_RowEditing"
                    OnRowUpdating="GridView1_RowUpdating"
                    OnRowCancelingEdit="GridView1_RowCancelingEdit"
                    OnRowDeleting="GridView1_RowDeleting"
                    OnRowDataBound="GridView1_RowDataBound">
                    <Columns>
                        <asp:TemplateField HeaderText="S.No">
                            <ItemTemplate>
                                <asp:Label ID="lblSerial" runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="StationName" HeaderText="Station" />
                        <asp:BoundField DataField="ExtinguisherNo" HeaderText="Extinguisher No" />
                        <asp:BoundField DataField="Type" HeaderText="Type" />
                        <asp:BoundField DataField="Location" HeaderText="Location" />
                        <asp:BoundField DataField="LastInspection" HeaderText="Last Inspection" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="Status" HeaderText="Status" />
                        <asp:BoundField DataField="InspectionDueDate" HeaderText="Inspection Due" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="LastMaintenance" HeaderText="Last Maintenance" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="Remarks" HeaderText="Remarks" />
                        <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>