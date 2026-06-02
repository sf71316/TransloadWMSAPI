<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TranTest.aspx.cs" Inherits="YAEP.WMS.API.TranTest" %>

<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title>DTC Test</title>
    <style>
        pre {
            background-color: #eee;
            padding: 6px;
        }

            pre.pass {
                background-color: #c9e488;
            }

            pre.err {
                color: brown;
            }
    </style>
</head>
<body>
    <form runat="server">
        <table>
            <tr>
                <td>Data Source</td>
                <td>
                    <asp:TextBox runat="server" ID="txtDataSource"></asp:TextBox></td>
            </tr>
            <tr>
                <td>User Id</td>
                <td>
                    <asp:TextBox runat="server" ID="txtUserId"></asp:TextBox></td>
            </tr>
            <tr>
                <td>Password</td>
                <td>
                    <asp:TextBox runat="server" ID="txtPassword" TextMode="Password"></asp:TextBox></td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td>

                    <asp:RadioButtonList ID="RadioButtonList1" runat="server">
                        <asp:ListItem Value="1" Enabled="true" Selected="True">TransactionScope</asp:ListItem>
                        <asp:ListItem Value="2">CommittableTransaction</asp:ListItem>
                    </asp:RadioButtonList>
                </td>
            </tr>
        </table>
        <pre runat="server" id="preDisplay"><asp:Button ID="btnTest" Text="Test DTC" runat="server" OnClick="btnTest_Click" />
        </pre>
    </form>
</body>
</html>
