﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace S4PIDemoFE.Zjy
{
    public class DuplicateCleanPresenter
    {
        HLogHandler mlogger;
        public interface IView {
            void onDataOk(DataTable data, string msg);
            void onDataOk2(Dictionary<string, List<DuplicatItem>> data, string msg);
            void moveFinish(string msg);
            void onProGress(int process);
            void chageThread(Action moveFinish, object[] v);
            void chageThread<T>(Action<T> moveFinish, object[] v);
            void onError(string msg);
        }
        public static Color fontW1 = Color.FromArgb(255, 103, 58, 183);
        private IView mVIew;

        public static string getModsDefault() {
            string filepath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string modDir = filepath + "/Electronic Arts/The Sims 4/Mods/";
            return modDir;
        }
        public DuplicateCleanPresenter(IView mVIew) {
            this.mVIew = mVIew;
            bakPath =PackageHandler.dirMod + "/backup/duplicated/"+LogTagUtil.getYYmmDDStr()+"/";
            if (!Directory.Exists(bakPath)) {
                Directory.CreateDirectory(bakPath);
            }

            string logDir = PackageHandler.dirMod + "log/";
            if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
            //string time = DateTime.Now.ToString("HHmmss");
            string time = LogTagUtil.getYYmmStr();
            string logFileName = string.Format("log_duplicate_{0}.txt", time);
            mlogger = new HLogHandler(logDir+logFileName);
        }
        public Dictionary<string, string> mData = new Dictionary<string, string>();
        public Dictionary<string, List<DuplicatItem>> mData3 = new Dictionary<string, List<DuplicatItem>>();

        List<DuplicatItem> dupulicatss = new List<DuplicatItem>();
        private string parentDir = "";
        private string bakPath = "";
        public string GetBackupDir() {
            return bakPath;
        }
        public void openDir(string path) {
            if (Directory.Exists(path))
            {
                //System.Diagnostics.Process.Start("Explorer", path);部分目录不支持跳转，比如我的文档目录下面的子目录无法跳转
                System.Diagnostics.Process.Start(path);
            }
            else {
                MessageBox.Show("不存在路径:"+path);
            }
            
        }
         
        public void Release() {
            mlogger.Close();
        }

        public void AsyncSearchDupulicateMods(string path) {
            mData3.Clear();
            Func<DataTable> export = () => searchDupulicateMods(path);


            long tick1 = DateTime.Now.Ticks;
            Console.WriteLine("start time=" + DateTime.Now.ToString());


            export.BeginInvoke((de) => {
                try {
                    DataTable table = export.EndInvoke(de);
                    long tick2 = DateTime.Now.Ticks;
                    long time = tick2 - tick1;
                    float secondes = time / 10000000f;
                    string msg = "用时";
                    if (secondes > 60)
                    {
                        int fen = (int)(secondes / 60);
                        msg += fen + "分";
                        secondes= (float)Math.Round(secondes, 2);
                        msg += secondes - fen * 60 + "秒";
                    }
                    else
                    {
                        secondes = (float)Math.Round(secondes, 2);
                        msg += secondes + "秒";
                    }
                    Console.WriteLine("end time=" + DateTime.Now.ToString());
                    //Action<string> maction = mVIew.moveFinish;
                    //mVIew.chageThread(maction, new object[] { msg });
                    //BeginInvoke(new Action<DataTable>(onDataOk), table);
                    //Action<DataTable> mCallback = mVIew.onDataOk;
                    Action mCallback = () => mVIew.onDataOk(table, msg);

                    mVIew.chageThread(mCallback, new object[] { });
                }
                catch (Exception e) {
                    string errmsg = $"扫描目录{path}出现异常,{e.Message}";
                    string logMsg = $"{errmsg},{e.StackTrace}";
                    mlogger.InfoNewLine(logMsg);
                    mVIew.onError(errmsg);
                }

                //BeginInvoke(mListenter2,table);
                //BeginInvoke(mListenter);
            }, null);

        }
        public DataTable searchDupulicateMods(string path)
        {
            long tick1 = DateTime.Now.Ticks;
            parentDir = path;
            //string[] files = Directory.GetFiles(path, "*.package", SearchOption.AllDirectories);
            string pattern = "(";
            pattern += "*.package";
            pattern += "|" + "*.ts4script";
            pattern += ")";
            //(*.exe | *.txt)
            //string[] files = Directory.GetFiles(path, pattern, SearchOption.AllDirectories);
            //string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
            //    .Where(s => s.EndsWith(".mp3") || s.EndsWith(".jpg"));
            pattern = "*.*";
            //var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
            //    .Where(s => s.EndsWith(".package") || s.EndsWith(".ts4script"));
            if (!Directory.Exists(path)) {
                throw new Exception("路径不存在");
            }
            string[] files = Directory.GetFiles(path, pattern, SearchOption.AllDirectories);
            int len = files.Count();
            for (int i = 0; i < len; i++)
            {
                //string tfile = files[i];
                string tfile = files.ElementAt(i);
                if (tfile.EndsWith(".package")||tfile.EndsWith(".ts4script"))
                {
                    
                }
                else {
                    continue;
                }
                string fname = tfile.Substring(tfile.LastIndexOf("\\") + 1);

                DuplicatItem item = new DuplicatItem();
                DateTime time = File.GetLastWriteTime(tfile);
                string strTime = time.ToString("yyyy-MM-dd HH:mm:ss");

                item.fileName = fname;
                item.filepath = tfile;
                item.modifytime = strTime;
                if (mData3.ContainsKey(fname))
                {
                    List<DuplicatItem> tList = mData3[fname];
                    tList.Add(item);
                    mData3[fname] = tList;
                    //mData.Add(fname, "");
                    //dupulicatss.Add(item);
                }
                else
                {
                    List<DuplicatItem> tList = new List<DuplicatItem>();

                    tList.Add(item);
                    mData3.Add(fname, tList);
                }
                int p = i + 1;
                int percent = (int)(p * 100f / len);
                //Action<int> action = (t) => mVIew.onProGress(percent);
                Action action = ()=> mVIew.onProGress(percent);
                mVIew.chageThread(action, new object[] {  });
            }
            for (int i = 0; i < mData3.Count; i++) {

            }
            var obj = mData3.GetEnumerator();
            Console.WriteLine("total keys="+mData3.Count);
            List<string> names = new List<string>();
            while (obj.MoveNext()) {
                string fname = obj.Current.Key;
                List<DuplicatItem> list=    obj.Current.Value ;
                if (list.Count > 1)
                {
                    dupulicatss.AddRange(list);
                }
                else {
                    names.Add(fname);
                }
            }
            //排除不重复的文件
            foreach (string mname in names) {
                mData3.Remove(mname);
            }
            Action<string> maction = mVIew.moveFinish;
            Action< Dictionary<string, List<DuplicatItem>>,string> mDataCallback = mVIew.onDataOk2;


            long tick2 = DateTime.Now.Ticks;
            long timeDur = tick2 - tick1;
            float secondes = timeDur / 10000000f;
            string msg = "用时";
            if (secondes > 60)
            {
                int fen = (int)(secondes / 60);
                msg += fen + "分";
                secondes = (float)Math.Round(secondes, 2);
                msg += secondes - fen * 60 + "秒";
            }
            else
            {
                secondes = (float)Math.Round(secondes, 2);
                msg += secondes + "秒";
            }
            Action mCallback = () => mVIew.onDataOk2(mData3, msg);
            mVIew.chageThread(mCallback,new object[] { });
            //mVIew.chageThread(mDataCallback,new object[] { mData3, "" });
            Console.WriteLine("total keys2=" + mData3.Count);
            //foreach (string tkey in mData3.Keys) {
            //    List<DuplicatItem> list = mData3[tkey];
            //    if (list.Count > 1) {
            //        dupulicatss.AddRange(list);
            //    }
            //}
            //dupulicatss.Sort();
            dupulicatss.Sort(new DuplicatItem.Comparator());
            //dupulicatss.OrderByDescending();
            //mvDupulicatedModsToBak();
            DataTable tbale = new DataTable();

            tbale.Columns.Add("ID", Type.GetType("System.Int32"));
            tbale.Columns[0].AutoIncrement = true;
            tbale.Columns[0].AutoIncrementSeed = 1;
            tbale.Columns[0].AutoIncrementStep = 1;

            tbale.Columns.Add("文件名", Type.GetType("System.String"));
            tbale.Columns.Add("路径", Type.GetType("System.String"));
            tbale.Columns.Add("修改时间", Type.GetType("System.String"));
            int mindex = 1;
            foreach (DuplicatItem mItem in dupulicatss) {
                //DataRow row = tbale.NewRow();
                tbale.Rows.Add(new object[] { mindex,string.Format("{0}\r\n{1}\r\n{2}", mItem.fileName, mItem.filepath, mItem.modifytime), mItem.filepath, mItem.modifytime });
                mindex++;

            }
            return tbale;
        }

        internal void onDataRemove(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            //throw new NotImplementedException();
            //DataGridView view = (DataGridView)sender;
            //int index = e.RowIndex;
            //Console.WriteLine("removedId= " + index);
            //string path= dupulicatss[index].filepath;
            //Console.WriteLine("onDelete Size " + dupulicatss.Count);
            //Console.WriteLine("onDelete "+path);
            //DuplicatItem item=  FindItemByPath(path);
            
            //dupulicatss.Remove(item);
        }

        public void TestSizeChange
            () {
            //Console.WriteLine("TestSizeChange Size " + dupulicatss.Count);
            //mVIew.onError("测试 onerror");

        }
        public void AsyncMvDupulicatedModsToBak() {
            //Func<void> export = () => mvDupulicatedModsToBak();
            Action export = mvDupulicatedModsToBak;
            //() => mvDupulicatedModsToBak();
            //Console.WriteLine("before time=" + DateTime.Now.ToString());
            long tick1 = DateTime.Now.Ticks;
            export.BeginInvoke((de) => {
                export.EndInvoke(de);
                //Console.WriteLine("end time=" + DateTime.Now.ToString());
                long tick2= DateTime.Now.Ticks;
                long time = tick2 - tick1;
                float secondes = time / 10000000f;
                string msg = "用时";
                if (secondes > 60)
                {
                    int fen =(int ) (secondes / 60);
                    msg += fen + "分";
                    secondes = (float)Math.Round(secondes, 2);
                    msg += secondes - fen * 60 + "秒";
                }
                else {
                    secondes = (float)Math.Round(secondes, 2);
                    msg += secondes + "秒";
                }
                //BeginInvoke(new Action<DataTable>(onDataOk), table);
                Action<string> maction = mVIew.moveFinish;
                mVIew.chageThread(maction, new object[] { msg });
                //BeginInvoke(mListenter2,table);
                //BeginInvoke(mListenter);
            }, null);

        }
        public void MoveFileById(string fname,string path){

            string srcPath = path;
            string mdir = srcPath.Substring(parentDir.Length);
            string newDir = bakPath + mdir;
            DirectoryInfo parentinfo = Directory.GetParent(newDir);
            if (!parentinfo.Exists) {
                parentinfo.Create();
            }
            string newPath = newDir ;
            Console.WriteLine("new dir=" + newPath);
            File.Move(srcPath, newPath);
          
          
        }
        public void RemoveItem(string path)
        {
            //for (int i = dupulicatss.Count-1; i >= 0; i--) {
            //  DuplicatItem temp=  dupulicatss[i];
            //    if (path.Equals(temp.filepath)) {
            //        dupulicatss.Remove(temp);
            //    }
            //}
            string fname = path.Substring(path.LastIndexOf("\\") + 1);
            List<DuplicatItem> items = mData3[fname];

            for (int i = items.Count-1; i >=0; i--) {
                DuplicatItem mitem = items[i];
                if (mitem.filepath.Equals(path)) {
                    //string fname = mitem.fileName;
                    items.RemoveAt(i);
                    MoveFileById(fname, path);
                    break;
                }
            }
            mData3[fname] = items;
            //mPresent.MoveFileById(fname, path);

        }
            private DuplicatItem FindItemByPath(string path) {
            return null;
        }
        public void mvDupulicatedModsToBak()
        {

            FileStream mStr = null;
            try
            {

                //for (int i = 0; i < dupulicatss.Count; i++)
                //{
                //    DuplicatItem item = dupulicatss[i];
                //   string fname = item.fileName;
                //    string path = item.filepath;
                //    string srcPath = item.filepath;
                //    string mdir = srcPath.Substring(parentDir.Length);
                //    string newDir = bakPath + mdir;
                //    string newPath = newDir + item.fileName;
                //    Console.WriteLine("new dir=" + newPath);
                //    //File.Move(srcPath, newDir + item.fileName);
                //    try
                //    {
                //        MoveFileById(fname, path);
                //    }
                //    catch (Exception e)
                //    {
                //        mVIew.onError("移动文件出错,"+e.StackTrace);
                //        break;

                //    }
                //}]
                string notFile = bakPath + "/说明.txt";
                mStr = File.Open(notFile, FileMode.Append);
                Encoding mEncoding = Encoding.UTF8;
             
                string msg1 = $"{HLogHandler.newNowStr()} 备份目录{bakPath}";
                string newLine = "\r\n";

                StreamWriter mWriter = new StreamWriter(mStr, mEncoding);
                mWriter.Write(msg1);
                mWriter.Write(newLine);
                if (mData3.Keys.Count== 0) {
                    throw new Exception("当前重复列表为空");
                }
                foreach (string name in mData3.Keys)
                {
                    List<DuplicatItem> items = mData3[name];

                    if (items.Count > 1)
                    {
                        items.Sort(new DuplicatItem.ComparatorByTime());
                        //Lis
                        //Array.Reverse(items);
                        //Console.WriteLine($"--now name={name},time={}" );
                        foreach (DuplicatItem item in items)
                        {
                            Console.WriteLine(item.filepath + " \r\nmyTime =" + item.modifytime);
                        }
                        //排序过后保留最新的一个文件，从倒数第二个删除
                        for (int i = items.Count - 2; i >=0; i--)
                        {
                            DuplicatItem item = items[i];
                            string fname = item.fileName;
                            string path = item.filepath;
                            try
                            {
                                MoveFileById(fname, path);
                                string msg = $"{HLogHandler.newNowStr()} {path} 移动到备份目录";
                                mWriter.Write(msg);
                                mWriter.Write(newLine);
                                mWriter.Flush();
                            }
                            catch (Exception e)
                            {
                                throw new Exception("移动 " + path + "出现异常", e);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //mVIew.onError("移动异常," + e.StackTrace);
                //mlogger.InfoNewLine();

                string errmsg = $"移动异常,{e.Message}";
                string logMsg = $"{errmsg},{e.StackTrace}";
                mlogger.InfoNewLine(logMsg);
                mVIew.onError(errmsg);
            }
            finally {
                if (mStr != null) {
                    mStr.Flush();
                    mStr.Close();
                }
            }
          
        }
    }
}