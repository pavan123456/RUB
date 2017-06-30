using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
public partial class _Default : System.Web.UI.Page 
{
    private ArrayList obj = new ArrayList();
    protected void Page_Load(object sender, EventArgs e)
    {
        obj.Add("Shiv");
        obj.Add("Raju");
        obj.Add("Shiv1");
        obj.Add("Shiv1");
    }
    protected void Button1_Click(object sender, EventArgs e)
    {
        foreach (string x in obj)
        {
            Response.Write(x);
        }
        DropDownList1.DataSource = obj;
        DropDownList1.DataBind();
        ListBox1.DataSource = obj;
        ListBox1.DataBind();
        GridView1.DataSource = obj;
        GridView1.DataBind();
    }
}
