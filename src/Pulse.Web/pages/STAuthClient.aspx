<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="STAuthClient.aspx.cs" Inherits="Pulse.Web.pages.STAuthClient" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">

    <title>ST Login Page - Request</title>

</head>

<body style="font-family: 'Segoe UI'">

    <div style='height: 50%; width: 100%;'>

        <form runat="server">

            <div style='width: 100%; height: 100%; margin-top: 10%'>

                <div style='background-color: darkblue'>

                    <div align='center' style='color: white; font-family: Arial; font-size: 22px;'>
                        Enter your credentials

                    </div>

                </div>

                <div align='center'>

                    <asp:Label ID="lblErrorMsg" runat="server" EnableTheming="True" Font-Size="Medium" ForeColor="Red"></asp:Label>

                    <div style='width: 210px;'>

                        <div style='color: Blue; font-size: 14px; padding-top: 2%;' align='left'>

                            <label for='userId'>User ID:</label>

                        </div>

                        <div style='color: Blue; font-size: 14px; padding-right: 3px;'>

                            <asp:TextBox ID='txtUserId' runat="server" Width="200px" Height="20px" required Text="" />

                            &nbsp;
                        </div>

                    </div>

                    <div style='width: 210px;'>

                        <div style='color: Blue; font-size: 14px; padding-top: 2%;' align='left'>

                            <label for='password'>Password:</label>

                        </div>

                        <div style='color: Blue; font-size: 14px; padding-right: 3px;'>

                            <asp:TextBox TextMode="Password" ID='txtPassword' runat="server" Width="200px" Height="20px" required Text="" />

                            <br />

                        </div>

                    </div>

                    <div>

                        <div style='color: Blue; font-size: 14px; padding: 1% 0px 1% 145px;'>

                            <asp:Button ID="btnLogin" Text='Login' runat="server" Height="25px" Width="60px" OnClick="btnLogin_Click" />

                        </div>

                    </div>

                </div>

                <div>

                    <div style='background-color: darkblue; color: white; font-family: verdana; font-size: 12px; padding-top: 1px; padding-bottom: 3px;'>

                        <div align='center'>Please note that this is FALLBACK LOGIN MECHANISM and Single Sign On and Single Sign Out is not available</div>

                    </div>

                </div>

            </div>

        </form>

    </div>

</body>
</html>

