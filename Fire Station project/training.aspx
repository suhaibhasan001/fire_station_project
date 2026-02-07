<%@ Page Language="C#" 
    MasterPageFile="~/Site.master"
    AutoEventWireup="true" 
    CodeBehind="training.aspx.cs" 
    Inherits="fireportalopenPIA.training" %>

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
        <h4 class="mb-0">Training Records</h4>
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

            <div class="row">

                <div class="col-md-6 mb-3">
                    <label>Station</label>
                    <asp:TextBox ID="txtStation" runat="server"
                        CssClass="form-control readonly-field"
                        ReadOnly="true" />
                </div>

                <div class="col-md-6 mb-3">
                    <label>Department</label>
                    <asp:TextBox ID="txtDepartment" runat="server"
                        CssClass="form-control" />
                </div>

                <div class="col-md-6 mb-3">
                    <label>Name</label>
                    <asp:TextBox ID="txtName" runat="server"
                        CssClass="form-control" />
                </div>

                <div class="col-md-6 mb-3">
                    <label>Section</label>
                    <asp:TextBox ID="txtSection" runat="server"
                        CssClass="form-control" />
                </div>

                <div class="col-md-6 mb-3">
                    <label>PNo / SNo</label>
                    <asp:TextBox ID="txtPno" runat="server"
                        CssClass="form-control" />
                </div>

                <div class="col-md-6 mb-3">
                    <label>Designation</label>
                    <asp:TextBox ID="txtDesignation" runat="server"
                        CssClass="form-control" />
                </div>

                <div class="col-md-6 mb-3">
                    <label>Training Date</label>
                    <asp:TextBox ID="txtTrainingDate" runat="server"
                        CssClass="form-control"
                        TextMode="Date" />
                </div>

                <div class="col-md-6 mb-3">
                    <label>Valid Date</label>
                    <asp:TextBox ID="txtValidDate" runat="server"
                        CssClass="form-control"
                        TextMode="Date" />
                </div>

                <div class="col-12 mb-3">
                    <label>Remarks</label>
                    <asp:TextBox ID="txtRemarks" runat="server"
                        CssClass="form-control"
                        TextMode="MultiLine" />
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

                        <asp:BoundField DataField="Name" HeaderText="Name" />
                        <asp:BoundField DataField="PNo_SNo" HeaderText="PNo/SNo" />
                        <asp:BoundField DataField="Designation" HeaderText="Designation" />
                        <asp:BoundField DataField="Department" HeaderText="Department" />
                        <asp:BoundField DataField="Section" HeaderText="Section" />
                        <asp:BoundField DataField="StationName" HeaderText="Station" />
                        <asp:BoundField DataField="TrainingDate" HeaderText="Training Date" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="ValidDate" HeaderText="Valid Date" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="Remarks" HeaderText="Remarks" />

                        <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />

                    </Columns>

                </asp:GridView>

            </div>
        </div>

    </div>

</asp:Content>
