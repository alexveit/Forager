<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Forager.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <table>
            <tr>
                <td colspan="2">URL:</td>
            </tr>
            <tr>
                <td><asp:TextBox ID="TextBox1" runat="server" Width="200px"></asp:TextBox></td>
                <td><asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Button" /></td>
            </tr>
        </table>  
    </form>
</body>
</html>
