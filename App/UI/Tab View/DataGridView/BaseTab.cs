﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace App
{
    public partial class BaseTab : UserControl
    {
        protected List<Package> softwareList = new List<Package>();
        protected List<Package> selectedSoftwareList = new List<Package>();

        public BaseTab()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor, true);
            Program.SetDoubleBuffered(this);
        }

        #region Windows State
        private void exitButton_Click(object sender, EventArgs e)
        {
            this.Parent.Controls.Remove(this);
        }

        private void BaseTab_Leave(object sender, EventArgs e)
        {
            LoadingWindow.LoadAfterDone();
        }
        #endregion

        #region GridView Loading Functions
        protected void loadSoftwareToGridView(List<Package> softwareList)
        {
            softwareGridView.Rows.Clear();
            for (int i = 0; i < softwareList.Count; i++)
            {
                softwareGridView.Rows.Add(softwareList[i].Displayname, softwareList[i].Version);
            }
        }
        protected void loadSoftwareToGridView_Role(List<Package> softwareList, Role role)
        {
            softwareGridView.Rows.Clear();
            for (int i = 0; i < softwareList.Count; i++)
            {
                if (softwareList[i].Role == role)
                    softwareGridView.Rows.Add(softwareList[i].Displayname, softwareList[i].Version);
            }
        }
        #endregion

        #region GridView Changing Functions
        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < softwareGridView.RowCount; i++)
            {
                if (softwareGridView.Rows[i].Cells[0].Value != null && softwareGridView.Rows[i].Cells[0].Value.ToString().ToLower().Contains(searchBox.Text.ToLower()))
                    softwareGridView.Rows[i].Visible = true;
                else softwareGridView.Rows[i].Visible = false;
            }
        }
        private void softwareGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                for (int i = 0; i < softwareList.Count; i++)
                {
                    if (softwareList[i].Displayname == softwareGridView.Rows[e.RowIndex].Cells[0].Value.ToString())
                    {
                        selectedSoftwareList.Add(softwareList[i]);
                        softwareGridView.Rows.RemoveAt(e.RowIndex);
                        softwareList.RemoveAt(i);
                        return;
                    }
                }
                for (int i = 0; i < selectedSoftwareList.Count; i++)
                {
                    if (selectedSoftwareList[i].Displayname == softwareGridView.Rows[e.RowIndex].Cells[0].Value.ToString())
                    {
                        softwareList.Add(selectedSoftwareList[i]);
                        softwareGridView.Rows.RemoveAt(e.RowIndex);
                        selectedSoftwareList.RemoveAt(i);
                        return;
                    }
                }
            }
        }
        private void softwareGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            softwareGridView.Rows[0].Selected = false;
        }
        #endregion

        #region Menu
        private void menuButton_Click(object sender, EventArgs e)
        {
            if (menuPanel.Width != 300)
                menuPanel.Width = 300;
            else menuPanel.Width = 78;
        }
        private void IT_Button_Click(object sender, EventArgs e)
        {
            loadSoftwareToGridView_Role(softwareList, Role.It);
        }
        private void Tech_Button_Click(object sender, EventArgs e)
        {
            loadSoftwareToGridView_Role(softwareList, Role.Tech);
        }
        private void Graphic_Button_Click(object sender, EventArgs e)
        {
            loadSoftwareToGridView_Role(softwareList, Role.Graphic);
        }
        private void None_Button_Click(object sender, EventArgs e)
        {
            loadSoftwareToGridView_Role(softwareList, Role.None);
        }
        private void ImportSoftwareList_Button_Click(object sender, EventArgs e)
        {
            FileDialog dialog = new OpenFileDialog();
            dialog.Title = "Open AutoStudentDataExport";
            dialog.Filter = "AS files (*.AS)|*.AS";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = dialog.FileName;
                string passImport = DataAccess.Instance.GetPassCry();
                if (File.Exists(filePath))
                {
                    try
                    {
                        List<string> names = new List<string>();
                        using (StreamReader sr = File.OpenText(filePath))
                        {
                            string dataImport = sr.ReadToEnd();
                            string decrypt = Cryptography.Decrypt(dataImport, passImport);
                            string temp;
                            using (System.IO.StringReader reader = new System.IO.StringReader(decrypt))
                            {
                                while ((temp = reader.ReadLine()) != null)
                                {
                                    names.Add(temp);
                                }
                            }
                        }
                        selectedSoftwareList = new List<Package>(DataAccess.Instance.GetPackagesOfName(names));
                        loadSoftwareToGridView(selectedSoftwareList);
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("Lỗi đọc file");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show("Không có quyền đọc ở thư mục chọn");
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Lỗi không xác định");
                    }
                }
                else
                {
                    MessageBox.Show("Không tồn tại thư mục");
                }
            }
        }
        #endregion

        #region Main Buttons
        private void selectedSoftwareView_Button_Click(object sender, EventArgs e)
        {
            selectedSoftwareView_Button.Tag = "clicked";
            loadSoftwareToGridView(selectedSoftwareList);
        }
        private void allSoftwareView_Button_Click(object sender, EventArgs e)
        {
            selectedSoftwareView_Button.Tag = "unclicked";
            loadSoftwareToGridView(softwareList);
        }
        private void confirmButton_Click(object sender, EventArgs e)
        {
            if (selectedSoftwareList.Count != 0)
            {
                DialogResult dialogResult = MessageBox.Show("Bạn có chắc chắn muốn tiếp tục?", "TIẾP TỤC", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    Program.mainUI.Controls.Remove(this);
                    exec();
                }
            }
            else MessageBox.Show("Bạn chưa chọn phần mềm nào");
        }
        protected virtual void exec() { }
        protected virtual void init() { }
        #endregion
    }
}