<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="formn.aspx.cs" Inherits="FrontEnd.formn" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
        <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Button ID="btnList" runat="server" Text="Get Data" />
            <br />

            Name : <asp:TextBox ID="txtname" runat="server"></asp:TextBox><br />
            Time : <asp:DropDownList ID="ddlTime" runat="server"></asp:DropDownList><br />
            Duration : <asp:DropDownList ID="ddlDuration" runat="server"></asp:DropDownList><br />
            Number: <asp:TextBox ID="txtNumber" runat="server"></asp:TextBox><br />

            <asp:Button ID="btnSubmit" runat="server" Text="Book" />
        </div>
    </form>
</body>
</html>


<script>
    $(document).ready(function () {
        $("#btnSubmit").click(function () {
            $.ajax({
                url: "https://localhost:7142/api/", // Replace with your server-side script URL
                type: "GET", // or "POST", "PUT", "DELETE"
                dataType: "json", // Expected data type from the server (e.g., "json", "html", "text")
                data: {
                    param1: "value1", // Data to send to the server (if any)
                    param2: "value2"
                },
                success: function (response) {
                    // This function runs if the AJAX call is successful
                    $("#response").html("Server response: " + JSON.stringify(response));
                    console.log("Success:", response);
                },
                error: function (xhr, status, error) {
                    // This function runs if there's an error with the AJAX call
                    $("#response").html("Error: " + error);
                    console.error("Error:", status, error);
                }
            });
        });

        $("#btnList").click(function () {
            event.preventDefault();

            $.ajax({
                url: "http://localhost:7142/api/meetingrooms", 
                method: 'GET', // The HTTP method (GET, POST, PUT, DELETE, etc.)
                dataType: 'json', // The expected data type of the response (e.g., 'json', 'xml', 'text')
                success: function (response) {
                    // This function runs if the AJAX call is successful
                    //$("#response").html("Server response: " + JSON.stringify(response));
                    console.log("Success:", response);
                },
                error: function (xhr, status, error) {
                    // This function runs if there's an error with the AJAX call
                    $("#response").html("Error: " + error);
                    console.error("Error:", status, error);
                }
            });
        });
    });
</script>