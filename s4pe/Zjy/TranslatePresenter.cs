using s4pi.Interfaces;
using s4pi.Package;
using s4pi.WrapperDealer;
using StblResource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace S4PIDemoFE.Zjy
{
    public class TranslatePresenter
    {



        class ConTainer
        {
            public Dictionary<string, DetailItem> zhDic;
            public Dictionary<string, DetailItem> enDic;
            public string filename;
            public string strType;
            public string group;
            public string mInstance;
        }

        class DetailItem
        {
            public string keyHash;
            public string keyValue;
            public string tempNs;
            public string strNS;
            public string strType;
            public string group;
            public string mInstance;
        }
        class ExportedItem
        {

            public string beforeLang;
            public string TargetLang;
            public string strNS;
            public string filename;
            public string dkeyName;
            //220557DA-80000000-00000000D67DACAE-351C970F
        }

        public interface IView
        {

            void onProgress(int percent);
            void onFinished(string targetPath);
            void onError(string msg);
            void onMain(Action action);
        }
        IView mView;

        public TranslatePresenter(IView mView)
        {
            this.mView = mView;
        }

        public void TransToTarget

            (string mFile, string target)
        {
            //C: \Users\js\source\repos\Sims4Tools - master\s4pe\Zjy\TranslateUtil.cs
            TranslateUtil mutil = new TranslateUtil();
        }


        private IResource getEntryRes(IPackage tempPackage, IResourceIndexEntry entry)
        {
            bool useDefHandler = false;
            IResource res = null;
            try
            {
                res = WrapperDealer.GetResource(0, tempPackage, entry, useDefHandler);
            }
            catch (Exception e)
            {
                throw new Exception("获取entry异常,", e);
                //Could not find a resource handler
            }
            return res;
        }

        public void MergeNewTransTo(string srcDir) {
            string newKeysFilePath = srcDir + "/newKeys_exported.xml";
            string oldXml = srcDir + "/zh_exported.xml";
            try
            {
                //oldXml
                List<ExportedItem> newItems = ImportFromXml(newKeysFilePath);

                Dictionary<string, ExportedItem> dics = new Dictionary<string, ExportedItem>();
                for (int i = 0; i < newItems.Count; i++)
                {
                    ExportedItem mitem = newItems[i];
                    string mkey = mitem.dkeyName;
                    dics.Add(mkey, mitem);
                }

                List<ExportedItem> mergedItems = ImportFromXml(oldXml);
                for (int i = 0; i < mergedItems.Count; i++)
                {
                    ExportedItem mitem = mergedItems[i];
                    string mkey = mitem.dkeyName;
                    if (dics.ContainsKey(mkey))
                    {
                        mitem.TargetLang = dics[mkey].TargetLang;
                    }
                }
                string mergedFile = srcDir + "/merged_exported.xml";
                exportedItemToXml(mergedItems, mergedFile);
                string resultMsg = "合并完成,新目录总条数为:" + mergedItems.Count;
                Action action = () => mView.onFinished(resultMsg);
                outMsg("finished ------------------------------------------");
                mView.onMain(action);
                //mView.onMain();
            }
            catch (Exception e)
            {
                outMsg("ExporteZhXmlToEngXml error,", e);
                string errmsg = ExMsg("合并异常 error", e);
                Action action = () => mView.onFinished(errmsg);
                mView.onMain(action);
            }

        }

        public void ExporteZhXmlToEngXml(string srcDir,string targetDir) {
            try
            {
                List<ExportedItem> srcItems = getPackeageStblInfos(srcDir);
                outMsg(string.Format("src={0},count={1}", srcDir, srcItems.Count));
                List<ExportedItem> afterItems = getPackeageStblInfos(targetDir);
                outMsg(string.Format("target={0},count={1}", targetDir, afterItems.Count));
                Dictionary<string, ExportedItem> dics = new Dictionary<string, ExportedItem>();
                for (int i = 0; i < srcItems.Count; i++)
                {
                    ExportedItem mitem = srcItems[i];
                    string mkey = mitem.dkeyName;
                    dics.Add(mkey, mitem);
                }


                List<ExportedItem> newItems =new List<ExportedItem>();
                for (int i = 0; i < afterItems.Count; i++)
                {
                    ExportedItem mitem = afterItems[i];
                    string mkey = mitem.dkeyName;
                    if (dics.ContainsKey(mkey))
                    {
                        mitem.TargetLang = dics[mkey].TargetLang;
                    }
                    else
                    {
                        newItems.Add(mitem);
                    }
                    //dics.Add(mkey, mitem);
                }
                //mFile + "/zh_exported.xml"
                string filepath = targetDir + "/zh_exported.xml";
                exportedItemToXml(afterItems, filepath);

                string newKeysFilePath= targetDir + "/newKeys_exported.xml";
                exportedItemToXml(newItems, newKeysFilePath);
                string resultMsg = "转移完成,待翻译新条目数量为:"+newItems.Count+",原有数量为="+ srcItems.Count+",新目录总条数为:"+afterItems.Count;
                Action action = () => mView.onFinished(resultMsg);
                outMsg("finished ------------------------------------------");
                mView.onMain(action);
                //mView.onMain();
            }
            catch (Exception e) {
                outMsg("ExporteZhXmlToEngXml error,",e);
                string errmsg = ExMsg("ExporteZhXmlToEngXml error", e);
                Action action = () => mView.onFinished(errmsg);
                mView.onMain(action);
            }
         
        }
        private List<ExportedItem>  ImportFromXml(string mfile) {
            List<ExportedItem> srcItems = new List<ExportedItem>();
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(mfile);
            XmlNodeList lists=xmldoc.GetElementsByTagName("Entry");
            for (int i = 0; i < lists.Count; i++)
            {
               XmlNode mNode= lists.Item(i);
                ExportedItem item = new ExportedItem();
               string beforeStr= mNode.Attributes.GetNamedItem("A").Value ;
               string after= mNode.Attributes.GetNamedItem("B").Value ;
                string Namespace = mNode.Attributes.GetNamedItem("Namespace").Value ;
                string dkeyname = Namespace.Substring(Namespace.LastIndexOf("-")+1);
                string filename = Namespace.Substring(0,Namespace.LastIndexOf("|"));
                string strNS = Namespace.Substring(Namespace.LastIndexOf("|")+1);
                item.filename = filename;
                item.dkeyName = dkeyname;
                item.beforeLang = beforeStr;
                item.TargetLang = after;
                item.strNS = strNS;
                srcItems.Add(item);
            }
            return srcItems;
            ////3.创建第一行描述信息，添加到xmldoc文档中
            //XmlDeclaration xmldec = xmldoc.CreateXmlDeclaration("1.0", "utf-8", null);
            //xmldoc.AppendChild(xmldec);
            ////4.创建根节点，xml文档有且只能有一个根节点
            //XmlElement xmlele1 = xmldoc.CreateElement("Dictionary");

            //xmlele1.SetAttribute("A", "0x00");
            //xmlele1.SetAttribute("B", "0x02");
            //for (int i = 0; i < items.Count; i++)
            //{
            //    ExportedItem item = items[i];
            //    XmlElement xmlele2 = xmldoc.CreateElement("Entry");
            //    xmlele2.SetAttribute("A", item.beforeLang);
            //    xmlele2.SetAttribute("B", item.TargetLang);
            //    xmlele2.SetAttribute("Namespace", item.filename + "|" + item.strNS);
            //    xmlele1.AppendChild(xmlele2);
            //}
            //xmldoc.AppendChild(xmlele1);
            //outMsg("finalResult=" + result);
            //xmldoc.Save(path);
        }
        private void  exportedItemToXml(List<ExportedItem> items, string path) {
            XmlDocument xmldoc = new XmlDocument();
            //3.创建第一行描述信息，添加到xmldoc文档中
            XmlDeclaration xmldec = xmldoc.CreateXmlDeclaration("1.0", "utf-8", null);
            xmldoc.AppendChild(xmldec);
            //4.创建根节点，xml文档有且只能有一个根节点
            XmlElement xmlele1 = xmldoc.CreateElement("Dictionary");

            xmlele1.SetAttribute("A", "0x00");
            xmlele1.SetAttribute("B", "0x02");
            for (int i = 0; i < items.Count; i++)
            {
                ExportedItem item = items[i];
                XmlElement xmlele2 = xmldoc.CreateElement("Entry");
                xmlele2.SetAttribute("A", item.beforeLang);
                xmlele2.SetAttribute("B", item.TargetLang);
                xmlele2.SetAttribute("Namespace",item.filename+"|"+ item.strNS);
                xmlele1.AppendChild(xmlele2);
            }
            xmldoc.AppendChild(xmlele1);
            //outMsg("finalResult=" + result);
            xmldoc.Save(path);
        }

        private List<ExportedItem>  getPackeageStblInfos(string mFile) {
            List<ExportedItem> items = new List<ExportedItem>();
            List<ConTainer> mContainers = new List<ConTainer>();
            try
            {
                string directoryPath = mFile;
                string searchPattern = "*.package";
                bool isSearchChild = true;
                //如果目录不存在，则抛出异常  
                string[] files = getChilds(directoryPath, searchPattern, isSearchChild);
                Dictionary<string, ConTainer> keyCotainer = new Dictionary<string, ConTainer>();
                for (int i = 0; i < files.Length; i++)
                {
                    string tfile = files[i];
                    string name = tfile.Substring(tfile.LastIndexOf("\\") + 1);
                    IPackage tempPackage = Package.OpenPackage(0, tfile, true);
                    List<IResourceIndexEntry> list = tempPackage.GetResourceList;
                    int mCount = 0;
                    int mCount2 = 0;
                    ConTainer tempContainer = null;

                    Dictionary<string, Dictionary<string, DetailItem>> mlangs = new Dictionary<string, Dictionary<string, DetailItem>>();
                    string engKey = "";
                    foreach (IResourceIndexEntry tEnry in list)
                    {


                        uint mType = tEnry.ResourceType;
                        ulong instanceKey = tEnry.Instance;
                        string strInstance = string.Format("{0:X16}", instanceKey);
                        string strGroup = string.Format("{0:X8}", tEnry.ResourceGroup);
                        string strType = string.Format("{0:X8}", mType);
                        string testType = string.Format("{0:X16}", instanceKey);
                        //instanceKey >> 16;
                        if (mType == 0x220557DA)
                        {


                            Dictionary<string, DetailItem> dicKey = new Dictionary<string, DetailItem>();

                            IResource res = getEntryRes(tempPackage, tEnry);
                            StblResource.StblResource stblRes = (StblResource.StblResource)WrapperDealer.GetResource(0, tempPackage, tEnry, false);
                            StringEntryList mStrs = stblRes.Entries;
                            for (int m = 0; m < mStrs.Count; m++)
                            {

                                StringEntry sTry = mStrs[m];
                                uint kint = sTry.KeyHash;
                                string kvalue = sTry.StringValue;
                                string key = string.Format("0x{0:X8}", kint);
                                string dMsg = string.Format("stbl res key=0x{0:X8},value={1}", kint, kvalue);
                                DetailItem deItem = new DetailItem();
                                deItem.keyHash = key;
                                deItem.keyValue = kvalue;
                                string tempNs = string.Format("{0}-{1}-{2}-{3}", strType, strGroup, strInstance, key);
                                deItem.strNS = tempNs;
                                if (dicKey.ContainsKey(key))
                                {
                                    string oldKey = dicKey[key].keyValue;
                                    if (!oldKey.Equals(kvalue))
                                    {
                                        outMsg("[bug]  name duplicated at" +
                                        "old=" + dicKey[key] +
                                        ",new=" + kvalue +
                                        " |" + tempNs + "");
                                    }
                                }
                                else
                                {
                                    dicKey.Add(key, deItem);
                                }
                                //outMsg(name + "\n stbl" + dMsg);
                            }
                            mCount++;
                            if (strInstance.StartsWith("00"))
                            {
                                engKey = strInstance;
                                if (tempContainer == null)
                                {
                                    tempContainer = new ConTainer();
                                    tempContainer.strType = strType;
                                    tempContainer.group = strGroup;
                                    tempContainer.mInstance = strInstance;
                                }
                            }
                            mlangs.Add(strInstance, dicKey);
                        }
                    }
                    if (!"".Equals(engKey))
                    {
                        string zhKey = "02" + engKey.Substring(2);
                        tempContainer.enDic = mlangs[engKey];
                        tempContainer.zhDic = mlangs[zhKey];
                    }
                    //if (mCount != 2) {
                    //    outMsg(name+" [bug] count !=2,count="+mCount
                    //        );
                    //}
                    if (mCount > 0)
                    {
                        outMsg("dangqianID:" +
                  "" + name + "" +
                  "\t" + ",hasStbl ,cout=" + mCount);
                    }
                    tempPackage.Dispose();
                    if (tempContainer != null)
                    {
                        keyCotainer.Add(name, tempContainer);
                        tempContainer.filename = name;
                        mContainers.Add(tempContainer);
                    }

                    //tempPackage.AddResource()
                    //tempPackage.AddResource()
                }

                foreach (ConTainer mCt in mContainers)
                {
                    if (mCt.enDic != null)
                    {
                        foreach (string mKey in mCt.enDic.Keys)
                        {
                            if (mCt.zhDic == null)
                            {
                                outMsg("noZh " + mCt.filename);
                            }
                            DetailItem item = mCt.enDic[mKey];
                            if (mCt.zhDic.ContainsKey(mKey))
                            {

                                DetailItem item2 = mCt.zhDic[mKey];
                                //if (item2 == null)
                                //{
                                //    outMsg("item null2");
                                //    break;
                                //}
                                ExportedItem tempItem = new ExportedItem();
                                tempItem.strNS = item.strNS;
                                tempItem.dkeyName = item.keyHash;
                                tempItem.beforeLang = item.keyValue;
                                tempItem.TargetLang = item2.keyValue;
                                tempItem.filename = mCt.filename;
                                items.Add(tempItem);
                            }
                            else
                            {
                                ExportedItem tempItem = new ExportedItem();
                                tempItem.dkeyName = item.keyHash;
                                tempItem.strNS = item.strNS;
                                tempItem.beforeLang = item.keyValue;
                                tempItem.TargetLang = item.keyValue;
                                tempItem.filename = mCt.filename;
                                items.Add(tempItem);

                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                outMsg("getpackInfo error", e);
                string errMsg = ExMsg("导出xml异常", e);
                Action action = () => mView.onError(errMsg);
                mView.onMain(action);
            }
            return items;
        }
        public void ExportXmlOnly(string mFile)
        {
            List<ExportedItem> items = new List<ExportedItem>();
            List<ConTainer> mContainers = new List<ConTainer>();
            try
            {
                string directoryPath = mFile;
                string searchPattern = "*.package";
                bool isSearchChild = true;
                //如果目录不存在，则抛出异常  
                string[] files = getChilds(directoryPath, searchPattern, isSearchChild);
                Dictionary<string, ConTainer> keyCotainer = new Dictionary<string, ConTainer>();
                for (int i = 0; i < files.Length; i++)
                {
                    string tfile = files[i];
                    string name = tfile.Substring(tfile.LastIndexOf("\\") + 1);
                    IPackage tempPackage = Package.OpenPackage(0, tfile, true);
                    List<IResourceIndexEntry> list = tempPackage.GetResourceList;
                    int mCount = 0;
                    ConTainer tempContainer = null;

                    Dictionary<string, Dictionary<string, DetailItem>> mlangs = new Dictionary<string, Dictionary<string, DetailItem>>();
                    string engKey = "";
                    foreach (IResourceIndexEntry tEnry in list)
                    {

                        
                        uint mType = tEnry.ResourceType;
                        ulong instanceKey = tEnry.Instance;
                        string strInstance = string.Format("{0:X16}", instanceKey);
                        string strGroup = string.Format("{0:X8}", tEnry.ResourceGroup);
                        string strType = string.Format("{0:X8}", mType);
                        string testType= string.Format("{0:X16}", instanceKey);
                        //instanceKey >> 16;
                        if (mType == 0x220557DA)
                        {
                          

                            Dictionary<string, DetailItem> dicKey = new Dictionary<string, DetailItem>();
                          
                            IResource res = getEntryRes(tempPackage, tEnry);
                            StblResource.StblResource stblRes = (StblResource.StblResource)WrapperDealer.GetResource(0, tempPackage, tEnry, false);
                            StringEntryList mStrs = stblRes.Entries;
                            for (int m = 0; m < mStrs.Count; m++)
                            {
                             
                                StringEntry sTry = mStrs[m];
                                uint kint = sTry.KeyHash;
                                string kvalue = sTry.StringValue;
                                string key = string.Format("0x{0:X8}", kint);
                                string dMsg = string.Format("stbl res key=0x{0:X8},value={1}", kint, kvalue);
                                DetailItem deItem = new DetailItem();
                                deItem.keyHash = key;
                                deItem.keyValue = kvalue;
                                string tempNs = string.Format("{0}-{1}-{2}-{3}", strType, strGroup,strInstance,key);
                                deItem.strNS = tempNs;
                                if (dicKey.ContainsKey(key)) {
                                    string oldKey = dicKey[key].keyValue;
                                    if (!oldKey.Equals(kvalue)) {
                                        outMsg("[bug]  name duplicated at" +
                                        "old=" + dicKey[key] +
                                        ",new=" + kvalue +
                                        " |" + tempNs + "");
                                    }
                                }
                                else
                                {
                                    dicKey.Add(key, deItem);
                                }
                                //outMsg(name + "\n stbl" + dMsg);
                            }
                            mCount++;
                            if (strInstance.StartsWith("00"))
                            {
                                engKey = strInstance;
                                if (tempContainer == null)
                                {
                                    tempContainer = new ConTainer();
                                    tempContainer.strType = strType;
                                    tempContainer.group = strGroup;
                                    tempContainer.mInstance = strInstance;
                                }
                            }
                            mlangs.Add(strInstance,dicKey);
                        }
                    }
                    if (!"".Equals(engKey)) {
                        string zhKey = "02" + engKey.Substring(2);
                        tempContainer.enDic = mlangs[engKey];
                        tempContainer.zhDic = mlangs[zhKey];
                    }
                    //if (mCount != 2) {
                    //    outMsg(name+" [bug] count !=2,count="+mCount
                    //        );
                    //}
                    if (mCount > 0) {
                        outMsg("dangqianID:" +
                  "" + name + "" +
                  "\t" + ",hasStbl ,cout=" + mCount);
                    }
                    tempPackage.Dispose();
                    if (tempContainer != null) {
                        keyCotainer.Add(name, tempContainer);
                        tempContainer.filename = name;
                        mContainers.Add(tempContainer);
                    }
                    
                    //tempPackage.AddResource()
                    //tempPackage.AddResource()
                }

                foreach (ConTainer mCt in mContainers)
                {
                    if (mCt.enDic != null)
                    {
                        foreach (string mKey in mCt.enDic.Keys)
                        {
                            if (mCt.zhDic == null)
                            {
                                outMsg("noZh "+mCt.filename);
                            }
                            DetailItem item = mCt.enDic[mKey];
                            if (mCt.zhDic.ContainsKey(mKey))
                            {
                              
                                DetailItem item2 = mCt.zhDic[mKey];
                                //if (item2 == null)
                                //{
                                //    outMsg("item null2");
                                //    break;
                                //}
                                ExportedItem tempItem = new ExportedItem();
                                tempItem.strNS = item.strNS;
                                tempItem.beforeLang = item.keyValue;
                                tempItem.TargetLang = item2.keyValue;
                                tempItem.filename = mCt.filename;
                                items.Add(tempItem);
                            }
                            else
                            {
                                ExportedItem tempItem = new ExportedItem();
                                tempItem.strNS = item.strNS;
                                tempItem.beforeLang = item.keyValue ;
                                tempItem.TargetLang = item.keyValue;
                                tempItem.filename = mCt.filename;
                                items.Add(tempItem);

                            }
                        }
                        //foreach (string key in mCt.zhDic.Keys)
                        //{
                           
                        //}
                    }

                }

                //outMsg("totalSize=" + items.Count);
                StringBuilder sb = new StringBuilder();
   //             <? xml version = "1.0" encoding = "utf-8" ?>
   //< Dictionary A = "0x2" B = "0x0" >
                      sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sb.Append("\r\n");
                sb.Append("<Dictionary A=\"0x2\" B=\"0x0\">");
                sb.Append("\r\n");
                //             <? xml version = "1.0" encoding = "utf-8" ?>
                //<Dictionary A = "0x2" B = "0x0" >
                XmlDocument xmldoc = new XmlDocument();
                //3.创建第一行描述信息，添加到xmldoc文档中
                XmlDeclaration xmldec = xmldoc.CreateXmlDeclaration("1.0", "utf-8", null);
                xmldoc.AppendChild(xmldec);
                //4.创建根节点，xml文档有且只能有一个根节点
                XmlElement xmlele1 = xmldoc.CreateElement("Dictionary");
                xmldoc.AppendChild(xmlele1);
                for (int i = 0; i < items.Count; i++)
                {
                    ExportedItem item = items[i];

                    string data = string.Format("<Entry A=\"{0}\" B=\"{1} \" Namespace=\"{2}|{3}\" />", item.beforeLang.Replace("\"", "'"), item.TargetLang.Replace("\"", "'"), item.filename.Replace("\"", "'"), item.strNS.Replace("\"", "'"));
                    sb.Append(data);
                    sb.Append("\r\n");
                    XmlElement xmlele2 = xmldoc.CreateElement("Entry");
                    xmlele2.SetAttribute("A", item.beforeLang);
                    xmlele2.SetAttribute("B", item.TargetLang);
                    xmlele2.SetAttribute("Namespace", item.strNS);
                    xmlele1.AppendChild(xmlele2);
                    //outMsg(string.Format(" A=\"{0}\",B=\"{1},\"  Namespace=\"{2}\"", item.beforeLang,item.TargetLang,item.strNS));
                }
                sb.Append("</Dictionary>");
                string result = sb.ToString();
                outMsg("finalResult=" + result);
                xmldoc.Save(mFile + "/zh_exported.xml");
                //SavedToFile(mFile, result);
                string retMsg = "待翻译条目：" + items.Count;
                Action action = () => mView.onFinished(retMsg);
                mView.onMain(action);
            }
            catch (Exception e)
            {
                //outMsg("导出xml异常", e);
                string errMsg = ExMsg("导出xml异常", e);
                Action action = () => mView.onError(errMsg);
                mView.onMain(action);
            }
          
        }

        void SavedToFile(string srcDir,string data) {
            //string saveFile = srcDir+"/" + new DateTime().ToString("MM-dd_HH_mm_ss") + ".xml";
            string saveFile = srcDir+"/" + "zh_exported.xml";
            DirectoryInfo info= Directory.GetParent(saveFile);
            if (!info.Exists) {
                Directory.CreateDirectory(info.FullName);
            }
            using (FileStream fStream = File.Open(saveFile, FileMode.OpenOrCreate)) {
                byte[] dataByte = UTF8Encoding.UTF8.GetBytes(data);
                fStream.Write(dataByte,0, dataByte.Length);
            }
       


        }

        private string ExMsg(string tag, Exception e)
        {

            return "异常," + tag + "," + e.Message + "\t,stack=" + e.StackTrace;
        }

        private void outMsg(string tag,Exception e) {
            outMsg("异常,"+ tag+"," + e.Message + "\t,stack=" + e.StackTrace);
        }
        private void outMsg(string msg) {
            Console.WriteLine(msg);
        }
        public string[] getChilds(string directoryPath, string searchPattern, bool isSearchChild)
        {
            outMsg( "nowDir= \n" + directoryPath);
            string[] files = null;
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    throw new FileNotFoundException(String.Format("目录'{0}'不存在", directoryPath));
                }
                files = Directory.GetFiles(directoryPath, searchPattern, isSearchChild ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            }
            catch (Exception e)
            {
                throw new FileNotFoundException("搜索package出错：" + e.Message + ",stack=" + e.StackTrace);
            }
            return files;
        }
    }
}
