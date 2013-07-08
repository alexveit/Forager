using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Odbc;
using System.Configuration;

public partial class result : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        try
        {
            OdbcConnection connection = new OdbcConnection(ConfigurationManager.ConnectionStrings["Forager_Connection"].ConnectionString);

            connection.Open();
            OdbcCommand command = new OdbcCommand("SELECT * FROM report", connection);

            OdbcDataReader dr = command.ExecuteReader();

            Response.Write("<table border=1> <tr><th> Link</th> <th>Error </th> </tr>");
            while (dr.Read())
            {
                
                Response.Write("<tr><td>" + dr["link"].ToString() + "</td>  ");
                Response.Write("<td>" + dr["error"].ToString() + "</td>  </tr>");
                
            }
            Response.Write("</table>");
            dr.Close();
            connection.Close();

        }
        catch (Exception ex)
        {
            Response.Write("An error occured: " + ex.Message);
        }

    }
}