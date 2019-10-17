using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace S4PIDemoFE
{
    public partial class ViewModList : Form
    {
        private string workDir = "";
        public ViewModList():this("")
        {
            //SystemInformation.MouseWheelPresent.ToString();
            //获取鼠标滚轮在滚动时所获得的行数  
            //SystemInformation.MouseWheelScrollLines.ToString();  
            //判断该操作系统是否支持滚轮鼠标  
            //SystemInformation.NativeMouseWheelSupport.ToString();   
        }
        Dictionary<string, Image> dicPics = new Dictionary<string, Image>() {
            
        };
        public ViewModList(string workDir)
        {
            InitializeComponent();
            this.workDir = workDir;
            panel1.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Panel1_MouseWheel);
            panel1.MouseClick += new MouseEventHandler(Panel1_MouseClick);
            listBox1.DrawMode = DrawMode.OwnerDrawVariable;
            listBox1.DrawItem += new DrawItemEventHandler(listBox1_DrawItem);
            listBox1.MeasureItem += new MeasureItemEventHandler(listBox1_MeasureItem);
            listBox1.SelectedIndexChanged+=new EventHandler(listBox1_OnItemSelectChanged);
            listBox1.MouseWheel += new MouseEventHandler(this.Panel1_MouseWheel);
            listBox1.ItemHeight = 25;
            string[] files= Directory.GetFiles(workDir+"Mods/Pose/", "*.jpg",SearchOption.AllDirectories);
            foreach (string s in files) {
                listBox1.Items.Add(s);
            }
            

            //Queue< string> tQ = new Queue<string>();
            //string n = tQ.FirstOrDefault<string>; ;
        }

    //internal class Cache : LruCache<string, Image>
    //{
    //    public Cache(int totalCount) : base(totalCount)
    //    {
    //    }

    //        public override int SizeOfObject(Image value)
    //        {
    //            return value.ToARGBData().Length/1024;
    //        }
    //    }
        public int mouseIndex = 0;
        public int listBoxIndex = 0;
        private void listBox1_OnItemSelectChanged(object sender, EventArgs e)
           
        {
            int index=listBox1.SelectedIndex;
            if (index != -1) {
                String name = listBox1.Items[index].ToString();
                //pictureBox1.Image = null;
                pictureBox1.Load(name);
            }
        }

            private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();
            ////让文字位于Item的中间
            float difH = (e.Bounds.Height - e.Font.Height) / 2;
            RectangleF rf = new RectangleF(e.Bounds.X, e.Bounds.Y + difH, e.Bounds.Width, e.Font.Height);
            if (e.Index != -1) {
                e.Graphics.DrawString(listBox1.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), rf);

            }
            //e.Graphics.DrawString(listBox1.Items[e.Index].ToString(), e.Font, new SolidBrush(Color.Black), e.Bounds);
        }


        private void listBox1_MeasureItem(object sender, MeasureItemEventArgs e)
        {


            e.ItemHeight = e.ItemHeight + 12;
            //if (e.Index == 2)//只设置第三项的高度
            //{
            //    e.ItemHeight = 50;
            //}
        }
        private void Panel1_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            System.Drawing.Size t = this.pictureBox1.Size;
            t.Width += e.Delta;
            t.Height += e.Delta;
            pictureBox1.Width = t.Width;
            pictureBox1.Height = t.Height;
            mouseIndex += e.Delta;

            listBoxIndex = listBox1.SelectedIndex;
            int indexAdd = 0;
            if (e.Delta > 0)
            {
                indexAdd = -1;
            }
            else {
                indexAdd = 1;
            }
            int finalIndex = listBoxIndex + indexAdd;
            if (finalIndex <=listBox1.Items.Count - 1 && finalIndex >=0)
            {
                listBox1.SelectedIndex = finalIndex;
                this.textBox1.Text = "选中了" + "" + listBox1.Items[finalIndex];
            }

        }
        private void Panel1_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            panel1.Focus();
        }
    }
   
}
