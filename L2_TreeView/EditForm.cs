using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace L2_TreeView
{
    public partial class EditForm : Form
    {
        private DataSet dataSet;
        private SqlDataAdapter sqlAdapter;
        public EditForm(DataSet ds, SqlDataAdapter adapter, Main.ActionType actionType)
        {
            InitializeComponent();
            DataGridView.DataSource = ds.Tables[0];
            switch (actionType)
            {
                case Main.ActionType.Edit:
                    DataGridView.AllowUserToAddRows = false;
                    DataGridView.AllowUserToDeleteRows = false;
                    break;
                case Main.ActionType.Create:
                    DataGridView.CellClick += dataGridView1_CellClick;
                    break;
            }
            dataSet = ds;
            sqlAdapter = adapter;
        }
        private void Edit_Click(object sender, EventArgs e)
        {
            try
            {
                SqlCommandBuilder local = new SqlCommandBuilder(sqlAdapter);
                local.ConflictOption = ConflictOption.OverwriteChanges;
                sqlAdapter.UpdateCommand = local.GetUpdateCommand();
                sqlAdapter.Update(dataSet.Tables[0]);
                dataSet.AcceptChanges();
                DataGridView.DataSource = dataSet.Tables[0];
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка при работе с базой данных", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            for (int i = 0; i < DataGridView.RowCount; i++)
            {
                for (int j = 0; j < DataGridView.ColumnCount; j++)
                {
                    if (Convert.ToString(DataGridView.Rows[i].Cells[j].Value) != "")
                    {
                        DataGridView.Rows[i].Cells[j].ReadOnly = true;
                    }
                }
            }
        }
    }
}
