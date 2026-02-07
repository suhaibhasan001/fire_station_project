<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="FireProtectionPortal.Login" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Login - Fire Protection Portal</title>
    <style type="text/css">
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background: #e0e5ec;
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }
        .login-container { width: 100%; max-width: 380px; }
        .login-card {
            background: #e0e5ec;
            border-radius: 25px;
            padding: 35px 30px;
            box-shadow: 20px 20px 60px #bec3cf, -20px -20px 60px #ffffff;
            transition: all 0.3s ease;
        }
        .login-card:hover { transform: translateY(-5px); }

        .login-header { text-align: center; margin-bottom: 30px; }
        .neu-icon {
            width: 65px; height: 65px; margin: 0 auto 20px;
            background: #e0e5ec; border-radius: 50%;
            display: flex; align-items: center; justify-content: center;
            box-shadow: 8px 8px 20px #bec3cf, -8px -8px 20px #ffffff;
        }
        .neu-icon svg { width: 32px; height: 32px; color: #6c7293; }
        .login-header h2 { color: #3d4468; font-size: 1.65rem; font-weight: 600; margin-bottom: 6px; }
        .login-header p { color: #9499b7; font-size: 14px; }

        /* Form Group */
        .form-group { margin-bottom: 22px; position: relative; }
        
        /* Input Container with Icon */
        .neu-input {
            position: relative;
            background: #e0e5ec;
            border-radius: 15px;
            box-shadow: inset 8px 8px 16px #bec3cf, inset -8px -8px 16px #ffffff;
            transition: all 0.3s ease;
            height: 52px;
            display: flex;
            align-items: center;
        }
        .neu-input:focus-within {
            box-shadow: inset 4px 4px 8px #bec3cf, inset -4px -4px 8px #ffffff;
        }

        /* Icon Styling */
        .input-icon {
            position: absolute;
            left: 20px;
            top: 50%;
            transform: translateY(-50%);
            color: #9499b7;
            pointer-events: none;
            z-index: 1;
        }
        .input-icon svg {
            width: 20px;
            height: 20px;
        }

        /* Labels - Floating Style */
        .neu-input label {
            position: absolute;
            left: 52px;
            top: 50%;
            transform: translateY(-50%);
            color: #9499b7;
            font-size: 16px;
            font-weight: 500;
            pointer-events: none;
            transition: all 0.3s ease;
            background: #e0e5ec;
            padding: 0 6px;
        }
        
        /* When field has value or focused */
        .neu-input.has-value label,
        .neu-input select:focus + label,
        .neu-input input:focus + label,
        .neu-input input:not(:placeholder-shown) + label {
            top: 10px;
            left: 52px;
            font-size: 12px;
            color: #6c7293;
            transform: translateY(-50%);
        }

        /* Dropdown & TextBox Styling */
        .neu-input select,
        .neu-input input {
            width: 100%;
            height: 100%;
            background: transparent;
            border: none;
            padding: 20px 45px 8px 52px;
            color: #3d4468;
            font-size: 16px;
            font-weight: 500;
            outline: none;
            appearance: none;
            -webkit-appearance: none;
        }
        .neu-input select {
            cursor: pointer;
        }
        
        /* Dropdown Arrow - ONLY for select elements */
        .neu-input select::-ms-expand { display: none; }
        .neu-input.has-select::after {
            content: '▼';
            position: absolute;
            right: 20px;
            top: 50%;
            transform: translateY(-50%);
            color: #9499b7;
            font-size: 12px;
            pointer-events: none;
        }

        /* Error Message */
        .error-message {
            color: #ff3b5c;
            font-size: 12px;
            font-weight: 500;
            margin-top: 8px;
            margin-left: 20px;
            opacity: 0;
            transition: opacity 0.3s;
        }
        .error-message.show { opacity: 1; }

        /* Login Button */
        .neu-button {
            width: 100%;
            background: #e0e5ec;
            border: none;
            border-radius: 15px;
            padding: 18px;
            color: #3d4468;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            box-shadow: 8px 8px 20px #bec3cf, -8px -8px 20px #ffffff;
            transition: all 0.3s ease;
            margin-top: 10px;
        }
        .neu-button:hover {
            transform: translateY(-2px);
            box-shadow: 12px 12px 30px #bec3cf, -12px -12px 30px #ffffff;
        }
        .neu-button:active {
            box-shadow: inset 4px 4px 10px #bec3cf, inset -4px -4px 10px #ffffff;
        }

        @media (max-width: 480px) {
            .login-card { padding: 35px 25px; border-radius: 20px; }
            .login-header h2 { font-size: 1.75rem; }
        }
    </style>

    <script type="text/javascript">
        function handleRoleChange() {
            var role = document.getElementById('<%= ddlRole.ClientID %>').value;
            var stationGroup = document.getElementById('stationGroup');
            if (role === "Admin") {
                stationGroup.style.display = "none";
            } else {
                stationGroup.style.display = "block";
            }
            updateLabelStates();
        }

        function updateLabelStates() {
            document.querySelectorAll('.neu-input').forEach(function (container) {
                var input = container.querySelector('select, input');
                if (input && (input.value !== "" && input.value !== null)) {
                    container.classList.add('has-value');
                } else {
                    container.classList.remove('has-value');
                }
            });
        }

        window.onload = function () {
            updateLabelStates();
            handleRoleChange();
        };
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        
        <div class="login-container">
            <div class="login-card">
                <div class="login-header">
                    <div class="neu-icon">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <path d="M17.982 18.725A7.488 7.488 0 0012 15.75a7.488 7.488 0 00-5.982 2.975m11.963 0a9 9 0 10-11.963 0m11.963 0A8.966 8.966 0 0112 21a8.966 8.966 0 01-5.982-2.275M15 9.75a3 3 0 11-6 0 3 3 0 016 0z"/>
                        </svg>
                    </div>
                    <h2>Welcome Back</h2>
                    <p>Fire Protection Portal</p>
                </div>

                <!-- Role -->
                <div class="form-group">
                    <div class="neu-input has-select">
                        <span class="input-icon">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <path d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"/>
                            </svg>
                        </span>
                        <asp:DropDownList ID="ddlRole" runat="server" onchange="handleRoleChange(); updateLabelStates();">
                            <asp:ListItem Value="">--Select Role--</asp:ListItem>
                        </asp:DropDownList>
                        <label>Select Role</label>
                    </div>
                    <asp:Label ID="lblRoleError" runat="server" CssClass="error-message"></asp:Label>
                </div>

                <!-- Station (Hidden for Admin) -->
                <div class="form-group" id="stationGroup">
                    <div class="neu-input has-select">
                        <span class="input-icon">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <path d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"/>
                                <path d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"/>
                            </svg>
                        </span>
                        <asp:DropDownList ID="ddlStation" runat="server" onchange="updateLabelStates();">
                            <asp:ListItem Value="">--Select Station--</asp:ListItem>
                        </asp:DropDownList>
                        <label>Select Station</label>
                    </div>
                </div>

                <!-- Username -->
                <div class="form-group">
                    <div class="neu-input">
                        <span class="input-icon">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <path d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
                            </svg>
                        </span>
                        <asp:TextBox ID="txtUsername" runat="server" placeholder=" " onkeyup="updateLabelStates();"></asp:TextBox>
                        <label>username</label>
                    </div>
                    <asp:Label ID="lblUserError" runat="server" CssClass="error-message"></asp:Label>
                </div>

                <!-- Password -->
                <div class="form-group">
                    <div class="neu-input">
                        <span class="input-icon">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/>
                                <path d="M7 11V7a5 5 0 0110 0v4"/>
                            </svg>
                        </span>
                        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" placeholder=" " onkeyup="updateLabelStates();"></asp:TextBox>
                        <label>Password</label>
                    </div>
                    <asp:Label ID="lblPassError" runat="server" CssClass="error-message"></asp:Label>
                </div>

                <!-- Submit Button -->
                <asp:Button ID="btnLogin" runat="server" Text="SIGN IN" CssClass="neu-button" OnClick="btnLogin_Click" />

                <!-- General Message -->
                <asp:Label ID="lblMessage" runat="server" CssClass="error-message" style="display:block; text-align:center; margin-top:20px;"></asp:Label>
            </div>
        </div>
    </form>
</body>
</html>