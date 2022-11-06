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
    public partial class Main : Form
    {
        private const string connectionString = @"Data Source=DESKTOP-461J672;Initial Catalog=TreeView;Integrated Security=True";
        public Main()
        {
            InitializeComponent();
        }
        public enum ActionType
        {
            Edit, Create
        }
        private TreeNode CreateTreeNode(string name, string tag, string text, int img)
        {
            TreeNode n = new TreeNode(text);
            n.Name = name;
            n.Tag = tag;
            n.ImageIndex = img;
            return n;
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            LoadTree();
        }
        private void LoadTree()
        {
            TreeView.Nodes.Clear();
            using (var cnn = new SqlConnection())
            {
                cnn.ConnectionString = connectionString;
                cnn.Open();
                var command = new SqlCommand("Select * from Post", cnn);
                using (var dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        TreeNode n = CreateTreeNode("Post", dr["ID"].ToString(), $"{dr["Post"]}", 0);
                        TreeView.Nodes.Add(n);
                        LoadStaff((int)(dr["ID"]), n);
                    }
                }
            }
        }
        private void LoadStaff(int PostId, TreeNode parent)
        {
            using (var cnn = new SqlConnection())
            {
                cnn.ConnectionString = connectionString;
                cnn.Open();
                var command = new SqlCommand(@"Select S.Surname + ' ' + S.Name + ' ' + S.Patronymic as ФИО, ID 
                                                from Staff S
                                                where S.Post_ID = @PostId", cnn);
                command.Parameters.AddWithValue("@PostId", PostId);
                using (var dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        TreeNode n = CreateTreeNode("Staff", dr["ID"].ToString(), $"{dr["ФИО"]}",1);
                        parent.Nodes.Add(n);
                        LoadProject((int)(dr["ID"]), n);
                    }
                }
            }
        }
        private void LoadProject(int StaffId, TreeNode parent)
        {
            using (var cnn = new SqlConnection())
            {
                cnn.ConnectionString = connectionString;
                cnn.Open();
                var command = new SqlCommand("Select * from Project where Staff_ID=@StaffId", cnn);
                command.Parameters.AddWithValue("@StaffId", StaffId);
                using (var dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        TreeNode n = CreateTreeNode("Staff", dr["ID"].ToString(), $"{dr["Project"]}", 1);
                        parent.Nodes.Add(n);
                        LoadDesc((int)(dr["ID"]), n);
                    }
                }
            }
        }
        private void LoadDesc(int StaffId, TreeNode parent)
        {
            using (var cnn = new SqlConnection())
            {
                cnn.ConnectionString = connectionString;
                cnn.Open();
                var command = new SqlCommand(@"Select P.Description, ID
                                                from Project P
                                                where P.ID = @StaffId", cnn);
                command.Parameters.AddWithValue("@StaffId", StaffId);
                using (var dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        parent.Nodes.Add(CreateTreeNode("Project", dr["ID"].ToString(), $"{dr["Description"]}", 2));
                    }
                }
            }
        }

        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TreeView.SelectedNode == null) return;

                using (var cnn = new SqlConnection())
                {
                    cnn.ConnectionString = connectionString;
                    cnn.Open();
                    var command = new SqlCommand($"delete from {TreeView.SelectedNode.Name} where ID=@ID", cnn);
                    command.Parameters.AddWithValue("@ID", int.Parse(TreeView.SelectedNode.Tag.ToString()));
                    command.ExecuteNonQuery();
                    TreeView.SelectedNode.Remove();
                }
        }

        private void EditDataBase(string command, ActionType actionType)
        {
            if (TreeView.SelectedNode == null) return;

            using (var cnn = new SqlConnection())
            {
                cnn.ConnectionString = connectionString;
                cnn.Open();
                var adapter = new SqlDataAdapter(command, cnn);
                var cb = new SqlCommandBuilder(adapter);
                var dataSet = new DataSet();
                adapter.Fill(dataSet);
                var edit = new EditForm(dataSet, adapter, actionType);
                edit.ShowDialog();
                LoadTree();
            }
        }

        private void редактироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TreeView.SelectedNode == null) return;
            EditDataBase($"select * from {TreeView.SelectedNode.Name} where ID={int.Parse(TreeView.SelectedNode.Tag.ToString())}", ActionType.Edit);
        }

        private void добавитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TreeView.SelectedNode == null) return;
            EditDataBase($"select * from {TreeView.SelectedNode.Name}", ActionType.Create);
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (TreeView.SelectedNode != null && TreeView.SelectedNode.Level < 4)
            {
                удалитьToolStripMenuItem.Enabled = true;
            }
            else
            {
                удалитьToolStripMenuItem.Enabled = false;
            }
        }
    }
}
