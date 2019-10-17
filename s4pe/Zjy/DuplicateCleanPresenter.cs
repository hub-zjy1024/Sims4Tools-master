using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace S4PIDemoFE.Zjy
{
    public class DuplicateCleanPresenter
    {
        public interface IView {
            void onDataOk(DataTable data, string msg);
            void moveFinish(string msg);
            void onProGress(int process);
            void chageThread(Action moveFinish, object[] v);
            void chageThread<T>(Action<T> moveFinish, object[] v);
            void onError(string msg);
        }

        private IView mVIew;
        public DuplicateCleanPresenter(IView mVIew) {
            this.mVIew = mVIew;
            bakPath = System.AppDomain.CurrentDomain.BaseDirectory + "/back/duplicated/";
            Console.WriteLine("bkdir=" + bakPath);
            if (!Directory.Exists(bakPath)) {
                Directory.CreateDirectory(bakPath);
            }
        }
        public Dictionary<string, string> mData = new Dictionary<string, string>();
        public Dictionary<string, List<DuplicatItem>> mData3 = new Dictionary<string, List<DuplicatItem>>();

        List<DuplicatItem> dupulicatss = new List<DuplicatItem>();
        private string parentDir = "";
        private string bakPath = "";
        public string GetBackupDir() {
            return bakPath;
        }
        public void AsyncSearchDupulicateMods(string path) {

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
                        msg += secondes - fen * 60 + "秒";
                    }
                    else
                    {
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
                    mVIew.onError("扫描目录" +
                        "" + path +
                        " 出现异常");
                }

                //BeginInvoke(mListenter2,table);
                //BeginInvoke(mListenter);
            }, null);

        }
        public DataTable searchDupulicateMods(string path)
        {
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

            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".package") || s.EndsWith(".ts4script"));
            int len = files.Count();
            for (int i = 0; i < len; i++)
            {
                //string tfile = files[i];
                string tfile = files.ElementAt(i);
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
            foreach (string mname in names) {
                mData3.Remove(mname);
            }
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
                tbale.Rows.Add(new object[] { mindex, mItem.fileName, mItem.filepath, mItem.modifytime });
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
            Console.WriteLine("before time=" + DateTime.Now.ToString());
            long tick1 = DateTime.Now.Ticks;
            export.BeginInvoke((de) => {
                export.EndInvoke(de);
                Console.WriteLine("end time=" + DateTime.Now.ToString());
                long tick2= DateTime.Now.Ticks;
                long time = tick2 - tick1;
                float secondes = time / 10000000f;
                string msg = "用时";
                if (secondes > 60)
                {
                    int fen =(int ) (secondes / 60);
                    msg += fen + "分";
                    msg += secondes - fen * 60 + "秒";
                }
                else {
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
                //}
                foreach (string name in mData3.Keys) {
                    List<DuplicatItem> items = mData3[name];
                    if (items.Count > 1) {
                        items.Sort();
                        Console.WriteLine("--now name=" + name);
                        foreach (DuplicatItem item in items)
                        {
                            Console.WriteLine(item.filepath + "mypath =" + item.modifytime);
                        }
                        for (int i = items.Count - 1; i > 0; i--) {
                            //items.Remove()
                            DuplicatItem item = items[i];
                            string fname = item.fileName;
                            string path = item.filepath;
                            //MoveFileById(fname, path);
                            try
                            {
                                MoveFileById(fname, path);
                            }
                            catch (Exception e)
                            {
                                throw new Exception("移动 " + path + "出现异常", e);
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                mVIew.onError("移动异常," + e.StackTrace);

            }
          
        }
    }
}
