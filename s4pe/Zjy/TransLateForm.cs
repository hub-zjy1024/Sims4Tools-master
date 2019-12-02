using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace S4PIDemoFE.Zjy
{
    public partial class TransLateForm : Form,TranslatePresenter.IView
    {
        public TransLateForm()
        {
            InitializeComponent();
        }

        public void onError(string msg)
        {
            MessageBox.Show(msg, "错误");
        }

        public void onFinished(string targetPath)
        {
            //throw new NotImplementedException();

            MessageBox.Show(targetPath, "提示");
        }

        public void onMain(Action action)
        {
            Invoke(action);
        }

        public void onProgress(int percent)
        {
            throw new NotImplementedException();
        }

        private void Button1_Click(object sender, EventArgs e)
        {

            string src = textBox1.Text.ToString();
            string pathTarget = textBox2.Text.ToString();
            //if ("".Equals(src) || "".Equals(pathTarget)   ) {
            //    onError("源目录和新目录都不能为空");
            //    return;
            //}
            //TranslatePresenter mPresenter = new TranslatePresenter(this);
            //Action act = () => mPresenter.ExporteZhXmlToEngXml(src,pathTarget);
            //BeginInvoke(act);
            TranslatePresenter mPresenter = new TranslatePresenter(this);
            Action act = () => mPresenter.ExporteZhXmlToEngXml(src, pathTarget);
            if ("".Equals(src) && !"".Equals(pathTarget))
            {
                act = () => mPresenter.MergeNewTransTo(pathTarget);
            }
            else if ("".Equals(src) && "".Equals(pathTarget))
            {
                onError("源目录和新目录不能同时为空");
                return;
            }
            BeginInvoke(act);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            string src = textBox1.Text.ToString();
            string pathTarget = textBox2.Text.ToString();
            TranslatePresenter mPresenter = new TranslatePresenter(this);
            Action act=()=>  mPresenter.ExportXmlOnly(src);
            BeginInvoke(act);
        }
    }
}
