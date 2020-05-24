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
            string filepath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string modDir = filepath + "/Electronic Arts/Mods/";
            textBox1.Text = modDir;
            mPresent = new DuplicateCleanPresenter(this);
            dataGridView1.RowContextMenuStripNeeded += Listener_DataGridView;
            dataGridView1.ReadOnly = true;
            dataGridView1.RowsDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AllowUserToResizeColumns = true;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            dataGridView1.BackgroundColor = Color.White;
        }

        public DonDataOk mListenter2;   //创建委托对象;
        public delegate void DonDataOk(DataTable t);

        public void onProGress(int process)
        {
            //throw new NotImplementedException();
            label2.Text = "" + process;
            //BeginInvoke(new Action<int>(onProGressShow), process);
        }

        public void onProGressShow(int process) {
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
            MessageBox.Show(msg+",移除完成,已被保存到" + mPresent.GetBackupDir());
            //throw new NotImplementedException();
        }

        public void onDataOk(DataTable data,string msg)
        {
            button2.Enabled=true;
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

            dataGridView1.DataSource = data;
            int cWidth=mview.Width;
            mview.Columns[data.Columns[0].ColumnName].Width = 30;
            DataGridViewCellStyle p1=       new DataGridViewCellStyle();
            //p1.Font = new Font(Font.Name   );
            //mview.Columns[0].DefaultCellStyle = p1;
         
            mview.Columns[1].DefaultCellStyle.ForeColor = Color.FromArgb(255, 103, 58, 183);
            //Font oldFont= mview.Columns[1].DefaultCellStyle.Font;
            Font tFont =new Font("黑体",11,FontStyle.Bold);
            
            mview.Columns[1].DefaultCellStyle.Font = tFont;
       //mview.Columns[data.Columns[0].ColumnName].ToolTipText  = 30;
       mview.Columns[data.Columns[1].ColumnName].Width = 280;
            mview.Columns[data.Columns[2].ColumnName].Width =646;
            mview.Columns[data.Columns[3].ColumnName].Width = 100;
            lbl_duplicated_count.Text = "" + data.Rows.Count;
            //label2.Text = "" + 100;
            onProGressShow(100);
            MessageBox.Show(msg+",总的记录条数为=" + data.Rows.Count);
            ListViewItem tItemt = new ListViewItem();
            //dataGridView1.DataBind();
        }
        public void Listener_DataGridView(object sender, DataGridViewRowContextMenuStripNeededEventArgs e) {
           int index= e.RowIndex ;

            if (index >= dataGridView1.Rows.Count) {
                return;
            }
            int id=(int)(dataGridView1.Rows[index].Cells["ID"].Value) ;
            //dataGridView1.SelectedRows = dataGridView1.Rows[index];
            dataGridView1.Rows[index].Selected = true;
            contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);//弹出操作菜单

            Console.WriteLine("now id=" + id);
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            mPresent = new DuplicateCleanPresenter(this);
            dataGridView1.RowsRemoved+=mPresent.onDataRemove;
            string path = textBox1.Text.ToString();
            //path = path.Replace("\\", "/");
            path = path.Replace("/", "\\");
            mPresent.AsyncSearchDupulicateMods(path);
            //mListenter = reFresh;
        }
        public void chageThread<T>(Action<T> moveFinish, object[] v) {
            BeginInvoke(moveFinish, v);
        }

        public void chageThread(Action d,object[] arg) {
            BeginInvoke(d, arg);
        }
         public void onError(string msg) {
            Action maction = ()=>MessageBox.Show(msg);
            try
            {
                BeginInvoke(maction);
            }
            catch (Exception e) {
                Console.WriteLine("onError ,error " + e.StackTrace);
            }
       
        }
       
        private void Button2_Click(object sender, EventArgs e)
        {
            mPresent.AsyncMvDupulicatedModsToBak();
        }

        private void ToolStripMenuItem_removeitem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0) {
                DataGridViewCellCollection cells = dataGridView1.SelectedRows[0].Cells;
                string fname = (string)(cells["文件名"].Value);
                string path = (string)(cells["路径"].Value);
                dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[0].Index);
                   
                //lvi.SubItems.Add("第2列,第" + data.Rows[i]["文件名"]);
                //lvi.SubItems.Add("第3列,第" + data.Rows[i]["路径"]);
            
                mPresent.RemoveItem( path);
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
            Console.WriteLine("loaded formwidth= " + firstWidth);
            //X = this.Width;//获取窗体的宽度

            //Y = this.Height;//获取窗体的高度
        }
        public int getDvWidth() {
            return (dataGridView1.Left  + dataGridView1.Margin.Left)*2;

        }
        int firstWidth = 0;
        private void Form1_Resize(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            int formW = this.Width;
            int dvWidth = formW-getDvWidth();
            if (firstWidth == 0)
            {
                firstWidth = formW;
            }
            else {
                if (formW>firstWidth) {

               
                }
            }
            if (dvWidth < 820) {
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
          
            if (e.RowIndex < dataGridView1.Rows.Count) {
                string path = dataGridView1.Rows[e.RowIndex].Cells["路径"].Value.ToString();
                //MessageBox.Show("选中行" + (e.RowIndex + 1) + ",路径=" + path);
                string mDir = path;
                int pIndex = path.LastIndexOf("\\");
                if (pIndex > 0) {
                    mDir = path.Substring(0, pIndex);
                }
                //string path = "";
                mPresent.openDir(mDir);
            }

         
            //e.ColumnIndex


        }
    }
}
