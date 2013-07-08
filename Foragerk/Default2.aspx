<!DOCTYPE html>
<head>
    <style type="text/css">
        .auto-style3 {
            width: 770px;
            height: 493px;
        }
        #form1 {
            height: 255px;
        }
    </style>
    <link href="Account/StyleSheet.css" rel="stylesheet" type="text/css" />
</head>
<script runat="server">

    Protected Sub Page_Load(sender As Object, e As EventArgs)

    End Sub
</script>

<html>
<body>

<div id="container" style="width:1000px"/>

<div id="header" style="background-color:#B5C7DE; color: #000000;">
<h1 style="margin-bottom:0;">Forager</h1></div>

<div id="content" style="background-color:#EEEEEE;height:1000px;width:438px; float:left; text-align: right;"/>
<form id="form1" runat="server">
    <div style="width: 398px">
    <asp:LoginView ID="LoginView1" runat="server">
<LoggedInTemplate>
<br />
You are logged in as
<asp:LoginName ID="LoginName1" runat="server" />
.<br />
<br />
<asp:LoginStatus ID="LoginStatus1" runat="server" />
</LoggedInTemplate>
<AnonymousTemplate>
<strong>You are not logged in. Please Login.<br />
</strong>
<br />

<asp:Login ID="Login1" runat="server" BackColor="#EFF3FB" BorderColor="#B5C7DE" BorderPadding="4"
BorderStyle="Solid" BorderWidth="5px" Font-Names="Verdana" ForeColor="#333333" Width="391px">
<TitleTextStyle BackColor="#507CD1" Font-Bold="True" Font-Size="0.9em" ForeColor="White" />
<LoginButtonStyle BackColor="White" BorderColor="#507CD1" BorderStyle="Solid" BorderWidth="1px"
Font-Names="Verdana" ForeColor="#284E98" />
<InstructionTextStyle Font-Italic="True" ForeColor="Black" />
</asp:Login>
</AnonymousTemplate>
</asp:LoginView> 

    
    </div>

    
    </form>





<div id="footer" style="background-color:#b5c7de; clear:both;text-align:center; height: 3px;">
    <div/>



</body>
</html>