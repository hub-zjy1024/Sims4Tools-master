using S4PIDemoFE.Zjy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace S4PIDemoFE
{
    public partial class DuplicatedForm : Form, S4PIDemoFE.Zjy.DuplicateCleanPresenter.IView
    {

        DuplicateCleanPresenter mPresent;
        public DuplicatedForm()
        {
            InitializeComponent();
            string modDir = DuplicateCleanPresenter.getModsDefault(); ;
            textBox1.Text = modDir;
            mPresent = new DuplicateCleanPresenter(this);
           
            dataGridView1.RowContextMenuStripNeeded += Listener_DataGridView;
            dataGridView1.ReadOnly = true;
            dataGridView1.RowsDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AllowUserToResizeColumns = true;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            dataGridView1.BackgroundColor = Color.White;

            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.MultiSelect = true;
            //listView1.DrawItem 
            listView1.Columns.Add("文件名", 100, HorizontalAlignment.Left); //添加表头
            listView1.Columns.Add("路径", 500, HorizontalAlignment.Left);
            listView1.Columns.Add("更新时间", 170, HorizontalAlignment.Left);
       
        }

        public DonDataOk mListenter2;   //创建委托对象;
        public delegate void DonDataOk(DataTable t);

        public void onProGress(int process)
        {
            //throw new NotImplementedException();
            label2.Text = "" + process;
            //BeginInvoke(new Action<int>(onProGressShow), process);
        }

        public void onProGressShow(int process)
        {
            label2.Text = "" + process;
        }
        public void moveFinish(string msg)
        {
            button2.Enabled = false;
            Console.WriteLine("移除完成");
            //DataTable dt = (DataTable)dataGridView1.DataSource;

            //dt.Rows.Clear();
            //dgvData.DataSource = dt;
            dataGridView1.DataSource = null;
            listView1.BeginUpdate();
            listView1.Items.Clear();
            listView1.EndUpdate();
            listView1.Refresh();
            DialogResult mResult = MessageBox.Show(this, $"{msg},移除完成,已被保存到{ mPresent.GetBackupDir()},是否打开", "提示", MessageBoxButtons.YesNo);
            if (mResult == DialogResult.Yes)
            {
                mPresent.openDir(mPresent.GetBackupDir());
            }
            //throw new NotImplementedException();
        }

        public void onDataOk2(Dictionary<string,List<DuplicatItem>> mData2, string msg)
        {
            //listView1.Tag=new List<>
            ListView groupedListView = listView1;
            if (mData2.Count == 0) {
                groupedListView.DataBindings.Clear();
                groupedListView.Items.Clear();
                listView1.Refresh();
                onError("没有重复列表");
                return;
            }

            groupedListView.BeginUpdate();
            //Dictionary<string, DataTable> mData2 = mPresent.ConvertToGroup(data);
            string parent = textBox1.Text;

            listView1.Columns[1].Text = "路径:" + parent;
            List<DuplicatItem> listItems = new List<DuplicatItem>();
            for (int j = 0; j < mData2.Count; j++)
            {
                KeyValuePair<string,List<DuplicatItem>> tempEntry = mData2.ElementAt(j);
                string tempKey = tempEntry.Key;
                ListViewGroup mGroup = new ListViewGroup();
                mGroup.Name = tempKey;
                mGroup.Header = tempKey;
                List<DuplicatItem> tempValue = tempEntry.Value;
                tempValue.ForEach((tempdata) =>
                {
                    ListViewItem tItemt = new ListViewItem();
                    //tItemt.Text = string.Format("{0}\r\n{1}\r\n{2}", tempRow["文件名"], tempRow["路径"], tempRow["修改时间"]);
                    tItemt.Text = "\t\t";
                    ListViewItem.ListViewSubItem sub1 = new ListViewItem.ListViewSubItem();
                    //sub1.Text = tempRow["路径"].ToString();
                    sub1.Text = tempdata.filepath.Substring(parent.Length); ;
                    ListViewItem.ListViewSubItem sub2 = new ListViewItem.ListViewSubItem();
                    sub2.Text = tempdata.modifytime;
                    tItemt.SubItems.Add(sub1);
                    tItemt.SubItems.Add(sub2);
                    mGroup.Items.Add(tItemt);
                    listView1.Items.Add(tItemt);
                    listItems.Add(tempdata);
                });
                groupedListView.Groups.Add(mGroup);
                //DataTable tempData = tempEntry.Value;
                //ListViewGroup mGroup = new ListViewGroup();
                //mGroup.Name = tempKey;
                //mGroup.Header = tempKey;
                //for (int i = 0; i < tempData.Rows.Count; i++)
                //{
                //    ListViewItem tItemt = new ListViewItem();
                //    DataRow tempRow = tempData.Rows[i];
                //    //tItemt.Text = string.Format("{0}\r\n{1}\r\n{2}", tempRow["文件名"], tempRow["路径"], tempRow["修改时间"]);
                //    tItemt.Text = tempRow["文件名"].ToString();
                //    //string mRow=tempRow["文件名"].ToString().Length;
                //    tItemt.Text = "\t\t";
                //    ListViewItem.ListViewSubItem sub1 = new ListViewItem.ListViewSubItem();
                //    //sub1.Text = tempRow["路径"].ToString();
                //    sub1.Text = tempRow["路径"].ToString().Substring(parent.Length);
                //    ListViewItem.ListViewSubItem sub2 = new ListViewItem.ListViewSubItem();
                //    sub2.Text = tempRow["修改时间"].ToString();
                //    tItemt.SubItems.Add(sub1);
                //    tItemt.SubItems.Add(sub2);
                //    mGroup.Items.Add(tItemt);
                //    listView1.Items.Add(tItemt);
                //    //DuplicatItem mItem;
                //    //listItems.Add();
                //}
                //groupedListView.Groups.Add(mGroup);
            }
            groupedListView.Tag = listItems;
            groupedListView.EndUpdate();
            groupedListView.Refresh();
            listView1.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
            button2.Enabled = true;
            lbl_duplicated_count.Text = "" + mData2.Count;
            label2.Text = "" + 100;
            onProGressShow(100);
            MessageBox.Show(msg + ",重复组数为=" + mData2.Count);
         
        }

        public void onDataOk(DataTable data, string msg)
        {
            button2.Enabled = true;
            //listView1.CheckBoxes = true;
            //for (int i = 0; i < data.Rows.Count; i++) {
            //    ListViewItem lvi = new ListViewItem();

            //    lvi.Selected = false;
            //    lvi.SubItems.Add("第1列," + data.Rows[i]["ID"]);
            //    lvi.SubItems.Add("第2列,第" + data.Rows[i]["文件名"]);
            //    lvi.SubItems.Add("第3列,第" + data.Rows[i]["路径"]);
            //    lvi.SubItems.Add("第4列,第" + data.Rows[i]["修改时间"]);
            //    listView1.Items.Add(lvi);
            //}

            //listView1.EndUpdate();
            //throw new NotImplementedException();

            DataGridView mview = dataGridView1;

            //dataGridView1.DataSource = data;
            //int cWidth = mview.Width;
            //mview.Columns[data.Columns[0].ColumnName].Width = 30;

            //mview.Columns[1].DefaultCellStyle.ForeColor = Color.FromArgb(255, 103, 58, 183);
            ////Font oldFont= mview.Columns[1].DefaultCellStyle.Font;
            //Font tFont = new Font("黑体", 11, FontStyle.Bold);

            //mview.Columns[1].DefaultCellStyle.Font = tFont;
            ////mview.Columns[data.Columns[0].ColumnName].ToolTipText  = 30;
            //mview.Columns[data.Columns[1].ColumnName].Width = 280;
            //mview.Columns[data.Columns[2].ColumnName].Width = 646;
            //mview.Columns[data.Columns[3].ColumnName].Width = 100;
     


           
            //tbale.Columns.Add("文件名", Type.GetType("System.String"));
            //tbale.Columns.Add("路径", Type.GetType("System.String"));
            //tbale.Columns.Add("修改时间", Type.GetType("System.String"));

            //dataGridView1.DataBind();
        }
        public void Listener_DataGridView(object sender, DataGridViewRowContextMenuStripNeededEventArgs e)
        {
            int index = e.RowIndex;

            if (index >= dataGridView1.Rows.Count)
            {
                return;
            }
            int id = (int)(dataGridView1.Rows[index].Cells["ID"].Value);
            //dataGridView1.SelectedRows = dataGridView1.Rows[index];
            dataGridView1.Rows[index].Selected = true;
            contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);//弹出操作菜单

            Console.WriteLine("now id=" + id);
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            //mPresent = new DuplicateCleanPresenter(this);
            dataGridView1.RowsRemoved += mPresent.onDataRemove;
            string path = textBox1.Text.ToString();
            //path = path.Replace("\\", "/");
            path = path.Replace("/", "\\");
            mPresent.AsyncSearchDupulicateMods(path);
            //mListenter = reFresh;
        }
        public void chageThread<T>(Action<T> moveFinish, object[] v)
        {
            BeginInvoke(moveFinish, v);
        }

        public void chageThread(Action d, object[] arg)
        {
            BeginInvoke(d, arg);
        }
        public void onError(string msg)
        {
            Action maction = () => MessageBox.Show(msg,"提示");
            try
            {
                BeginInvoke(maction);
            }
            catch (Exception e)
            {
                Console.WriteLine("onError ,error " + e.StackTrace);
            }

        }

        private void Button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            mPresent.AsyncMvDupulicatedModsToBak();
        }

        private void ToolStripMenuItem_removeitem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewCellCollection cells = dataGridView1.SelectedRows[0].Cells;
                string fname = (string)(cells["文件名"].Value);
                string path = (string)(cells["路径"].Value);
                dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[0].Index);

                //lvi.SubItems.Add("第2列,第" + data.Rows[i]["文件名"]);
                //lvi.SubItems.Add("第3列,第" + data.Rows[i]["路径"]);

                mPresent.RemoveItem(path);
                mPresent.TestSizeChange();
            }
        }

        private void DuplicatedForm_Load(object sender, EventArgs e)
        {
            this.Resize += new EventHandler(Form1_Resize);//窗体调整大小时引发事件

            if (firstWidth == 0)
            {
                firstWidth = this.Width;
            }
            //Padding mPadding = new Padding(100, 10, 100, 10);
            //dataGridView1.Margin = mPadding;
            dataGridView1.Width = firstWidth - getDvWidth();
            //dataGridView1.Width = (firstWidth - dataGridView1.Left) ;
            dataGridView1.Refresh();
            listView1.Width = dataGridView1.Width;
            Console.WriteLine("loaded formwidth= " + firstWidth);
            //X = this.Width;//获取窗体的宽度

            //Y = this.Height;//获取窗体的高度
        }
        public int getDvWidth()
        {
            return (dataGridView1.Left + dataGridView1.Margin.Left) * 2;

        }
        int firstWidth = 0;
        private void Form1_Resize(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            int formW = this.Width;
            int dvWidth = formW - getDvWidth();
            if (firstWidth == 0)
            {
                firstWidth = formW;
            }
            else
            {
                if (formW > firstWidth)
                {


                }
            }
            if (dvWidth < 820)
            {
                return;
            }
            dataGridView1.Width = dvWidth;
            int cWidth = dvWidth;
            DataGridView mview = dataGridView1;
            if (mview.Columns.Count > 4)
            {
                //mview.Columns["ID"].Width = 30;
                //mview.Columns["name"].Width = 100;
                //mview.Columns["path"].Width = (int)(cWidth * 0.8);
                //mview.Columns[""].Width = 200;

            }
            dataGridView1.Update();
            listView1.Width = dataGridView1.Width;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //FileDialog fileDialog = new FileDialog();
            //OpenFileDialog openfile = new OpenFileDialog();
            ////初始显示文件目录
            ////openfile.InitialDirectory = @"";
            //openfile.Title = "请选择目录";
            ////过滤文件类型
            //openfile.Filter = "文本文件|*.txt|可执行文件|*.exe|STOCK|STOCK.txt|所有文件类型|*.*";
            //if (DialogResult.OK == openfile.ShowDialog())
            //{
            //    //将选择的文件的全路径赋值给文本框
            //    textBox1.Text = openfile.FileName;
            //}
            FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "请选择模组文件夹";
            string lastPath = textBox1.Text;
            dialog.SelectedPath = lastPath;
            //dialog.RootFolder = Environment.SpecialFolder.Personal;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    MessageBox.Show(this, "文件夹路径不能为空", "提示");
                    return;
                }
                textBox1.Text = dialog.SelectedPath;
                //this.LoadingText = "处理中...";
                //this.LoadingDisplay = true;
                //Action<string> a = DaoRuData;
                //a.BeginInvoke(dialog.SelectedPath, asyncCallback, a);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            //dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            //dataGridView1.Columns[0].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            //dataGridView1.Columns

            if (e.RowIndex < dataGridView1.Rows.Count)
            {
                string path = dataGridView1.Rows[e.RowIndex].Cells["路径"].Value.ToString();
                //MessageBox.Show("选中行" + (e.RowIndex + 1) + ",路径=" + path);
                string mDir = path;
                int pIndex = path.LastIndexOf("\\");
                if (pIndex > 0)
                {
                    mDir = path.Substring(0, pIndex);
                }
                //string path = "";
                mPresent.openDir(mDir);
            }


            //e.ColumnIndex


        }

        private void DuplicatedForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            mPresent.Release();
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //listView1.HitTest
            ListViewHitTestInfo info = listView1.HitTest(e.X, e.Y);
            if (info.Item != null)
            {
                //gbxplayer1.Text = "视频1";
                //player1.playlist.stop();
                //player1.playlist.items.clear();
                var videoitem = info.Item as ListViewItem;
                try
                {
                    List<DuplicatItem> mItem = (List<DuplicatItem>)listView1.Tag;
                    int itemIndex = info.Item.Index;
                    DuplicatItem nowItem = mItem[itemIndex];
                    string mPath = nowItem.filepath;
                    int index = mPath.Length;
                    int tempIndex = mPath.LastIndexOf("\\");
                    if (tempIndex > 0)
                    {
                        index = tempIndex;
                    }
                  
                    //mItem[index].filepath;
                    string parent = mPath.Substring(0, index);

                    string rootDir = textBox1.Text;
                    //string finalPath = mItem[itemIndex].filepath;
                    //rootDir + parent;
                    string finalPath = nowItem.filepath.Substring(0, index);
                    mPresent.openDir(finalPath);
                }
                catch(Exception ex) {
                    Console.WriteLine($"open dir error,{ex.Message},{ex.StackTrace}" );
                
                }
               
            }
            else
            {

            }
        }
    }
}
