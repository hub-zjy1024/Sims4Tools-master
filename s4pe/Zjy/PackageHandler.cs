using CASPartResource;
using s4pi.Animation;
using s4pi.DataResource;
using s4pi.ImageResource;
using s4pi.Interfaces;
using s4pi.Package;
using s4pi.WrapperDealer;
using StblResource;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using S4Version = S4PIDemoFE.Settings.Version;
namespace S4PIDemoFE
{
    public class PackageHandler
    {
    public  static  Encoding mCoding= Encoding.UTF8;
        ExportConifg config;
        public static string dirMod = "D:/S4pe_Files/";
        public static string modSavedDir = dirMod+"Mods/";
        public static string modSavedAbDir = modSavedDir + "功能mod/";
        public static string modSavedThDir = modSavedDir + "替换mod/";

        HLogHandler hLogHandler = null;
        CASRecorder cRecoder = null;
        CasTypeRecorder coutsRecorder = null;

       public TihuanModRecoder tiLog;
        int poseCounts = 0;
        int notCASMod = 0;
        string rootDir = "";
        public bool removeAbilityMod = false;
        public bool removeTiHuan = false;
        public bool createCasInfoFile = false;
        List<Dictionary<string, string>> waitToMove = new List<Dictionary<string, string>>();
        //List<Dictionary<string, string>> typeAndRes = new List<Dictionary<string, string>>();
        Dictionary<string, string> typeAndRes = new Dictionary<string, string>();
        
        private static Dictionary<string, string> typeCName = new Dictionary<string, string>();
        static PackageHandler() {
            typeCName.Add("Blush", "腮红");
            typeCName.Add("Body", "全身");
            typeCName.Add("Bottom", "下身");
            typeCName.Add("Top", "上身");
            typeCName.Add("Tights", "内裤");
            typeCName.Add("TattooTorsoFrontUpper", "纹身(前上)");
            typeCName.Add("TattooTorsoBackUpper", "纹身(后上)");
            typeCName.Add("Hat", "帽子");
            typeCName.Add("TattooArmLowerLeft", "纹身(左手)");
            typeCName.Add("Socks", "袜子");
            typeCName.Add("Shoes", "鞋子");
            typeCName.Add("Earrings", "耳环");
            typeCName.Add("BraceletLeft", "手镯(左)");
            typeCName.Add("BraceletRight", "手镯(右)");
            typeCName.Add("Eyebrows", "眉毛");
            typeCName.Add("Eyeliner", "眼线");
            typeCName.Add("Eyeshadow", "眼影");
            typeCName.Add("Facepaint", "脸妆");
            typeCName.Add("FacialHair", "脸部毛发");
            typeCName.Add("Glasses", "眼镜");
            typeCName.Add("Freckles", "雀斑");
            typeCName.Add("Gloves", "手套");
            typeCName.Add("Hair", "头发");
            typeCName.Add("Lipstick", "唇彩");
            typeCName.Add("MoleLeftLip", "痣(左)");
            typeCName.Add("MoleRightCheek", "痣(左脸)");
            typeCName.Add("MouthCrease", "嘴皱纹");
            typeCName.Add("Necklace", "项链");
            typeCName.Add("NoseRingRight", "鼻环(右)");
            typeCName.Add("RingIndexLeft", "戒指(左)");
            typeCName.Add("RingIndexRight", "戒指(右)");
            typeCName.Add("RingMidLeft", "戒指(左)");
            typeCName.Add("RingMidRight", "戒指(右)");
            typeCName.Add("RingThirdLeft", "戒指(左)");
            typeCName.Add("RingThirdRight", "戒指(右)");
            typeCName.Add("ForeheadCrease", "皱纹(前额)");
        }
        public PackageHandler(string rootDir, bool removeAbilityMod) : this(rootDir)
        {
            this.removeAbilityMod = removeAbilityMod;
        }

        public PackageHandler(string parentDir)
        {
            this.rootDir = parentDir;
        }

        StringBuilder sbtihuan = new StringBuilder();
        public CASRecorder CRecoder { get => cRecoder; set => cRecoder = value; }
        public HLogHandler HLogHandler { get => hLogHandler; set => hLogHandler = value; }
        public CasTypeRecorder CoutsRecorder { get => coutsRecorder; set => coutsRecorder = value; }
        public ExportConifg Config { get => config; set => config = value; }

        public void ApendLineLog(string info)
        {
            if (hLogHandler != null)
            {
                hLogHandler.InfoNewLine(info);
            }
        }
        public void ApendLineRecordCas(string s)
        {
            if (CRecoder != null)
            {
                CRecoder.InfoNewLine(s);
            }
        }

        public void AppendCasCount(string s)
        {
            if (CRecoder != null)
            {
                CRecoder.InfoNewLine(s);
            }
        }
        public void AddPoseCounts()
        {
            poseCounts++;
        }
        public void AddNotCASCounts()
        {
            notCASMod++;
        }
        public void addCasCounts()
        {
            if (CoutsRecorder != null)
            {
                CoutsRecorder.addFile();
            }
        }
        public void putCasType(string type)
        {
            if (CoutsRecorder != null)
            {
                CoutsRecorder.putType(type);
            }
        }
        public void Release()
        {
            if (CRecoder != null)
            {
                cRecoder.Close();
            }
            if (HLogHandler != null)
            {
                HLogHandler.Close();
            }
            if (CoutsRecorder != null)
            {
                CoutsRecorder.Close();
            }
            if (tiLog != null)
            {
                tiLog.Close();
            }
           
            //throw new Exception();
        }

        private static string[] GetFileNames(string directoryPath, string searchPattern, bool isSearchChild)
        {
            //如果目录不存在，则抛出异常  
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    throw new FileNotFoundException(String.Format("目录'{0}'不存在", directoryPath));
                }

                return Directory.GetFiles(directoryPath, searchPattern, isSearchChild ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            }
            catch (Exception e)
            {
                throw new FileNotFoundException("搜索package出错：" + e);
            }
        }

      
        public class ExportConifg{
            public bool checkTihuan = false;

        }

        public string exportAll(string dirPath, MainForm form)
        {


            if (!Directory.Exists(dirMod))
            {
                Directory.CreateDirectory(dirMod);
            }
            string[] files = null;
            DateTime date1 = DateTime.Now;
            try
            {

                files = GetFileNames(dirPath, "*.package", true);
            }
            catch (Exception dirEx)
            {
                return "获取目录权限失败，请使用管理员身份运行,详细错误：" + dirEx.Message;
            }
            if (files == null || files.Length == 0)
            {
                return "当前目录无package结尾的模组文件";
            }
            ApendLineLog("当前处理目录为：" + dirPath);

            int len = files.Length;
            DateTime t1 = DateTime.Now;
            string durTime = ExecDateDiff(date1, t1);
            string debug1 = string.Format("Get File counts={0},user time {1}", len, durTime);
            ApendLineLog(debug1);
            string name = Thread.CurrentThread.Name;
            string tId = Thread.CurrentThread.ManagedThreadId.ToString();
            try
            {
                for (int i = 0; i < len; i++)
                {
                    string tempFilePath = files[i];
                    int persent = i * 100 / len;
                    DateTime dt2 = DateTime.Now;

                    ReadPackageAndExport(tempFilePath, null);
                    DateTime dt3 = DateTime.Now;
                    string durTime2 = ExecDateDiff(dt2, dt3);
                    //ApendLineLog(string.Format("readPack {0},user time {1},Thread={2}", tempFilePath.Substring(dirPath.Length), durTime2,name+tId));
                    form.BeginInvoke(new Action<int>(form.updateProgress), persent);
                }
            }
            catch (Exception mainEx)
            {
                string msg = "程序出现异常" + "\n";
                msg += GetExMsg(mainEx);
                ApendLineLog("[error]");
                ApendLineLog(msg);
                return "程序出现未知错误，请查看日志";
            }
            form.BeginInvoke(new Action<int>(form.updateProgress), 100);
            for (int i = 0; i < waitToMove.Count; i++)
            {
                Dictionary<string, string> dic = waitToMove[i];
                string path1 = dic["src_path"];
                string path2 = dic["target_path"];
                ApendLineLog("move from" + path1 + " to" + path2);
                FileMoveKeepFilePath(path1, path2);
            }
            ApendLineLog("动作mod数量:" + poseCounts);
            ApendLineLog("CAS mod数量:" + coutsRecorder.GetCasFileCounts());
            ApendLineLog("其他 mod数量:" + notCASMod);
            ApendLineLog("所有mod数量:" + len);

           
            StringBuilder bKeyHandlers = new StringBuilder();
            foreach (string key in typeAndRes.Keys)
            {
                bKeyHandlers.AppendLine(string.Format("{0} use {1}", key, typeAndRes[key]));
            }
            ApendLineLog("Handlers:" + bKeyHandlers.ToString());

            double duration = (double)((DateTime.Now - t1).TotalMilliseconds);
            int minute = (int)(duration / 1000 / 60);
            int second = (int)(duration / 1000);
            string timeLong = "";
            if (minute == 0)
            {
                timeLong = second + "秒";
            }
            else
            {
                second = second - minute * 60;
                timeLong = minute + "分" + second + "秒";
            }

            string result = "导出完成，总共找到模组文件：" + len + ",耗时：" + timeLong;
            if (removeTiHuan)
            {
                result += "\n";
                result += "移除的替换模组保存在："+ modSavedThDir;
            }
            if (removeAbilityMod)
            {
                result += "\n";
                result += "移除的功能模组保存在："+ modSavedAbDir;
            }
            return result ;
        }
        /// 程序执行时间测试
        /// </summary>
        /// <param name="dateBegin">开始时间</param>
        /// <param name="dateEnd">结束时间</param>
        /// <returns>返回(秒)单位，比如: 0.00239秒</returns>
        public static string ExecDateDiff(DateTime dateBegin, DateTime dateEnd)
        {
            TimeSpan ts1 = new TimeSpan(dateBegin.Ticks);
            TimeSpan ts2 = new TimeSpan(dateEnd.Ticks);
            TimeSpan ts3 = ts1.Subtract(ts2).Duration();
            //你想转的格式
            return ts3.TotalMilliseconds.ToString();
        }
        public bool ReadPackageAndExport(string filePath, StringBuilder builder2)
        {
            //EndOfStreamException
            StringBuilder builder = new StringBuilder();
            IPackage tempPackage = null;
            string relatedPath = filePath.Substring(rootDir.Length);
            builder.AppendLine("currentFile:" + relatedPath);
            try
            {
                tempPackage = Package.OpenPackage(0, filePath);
            }
            catch (Exception e)
            {
                builder.AppendLine("[error] 无法解析，不是合法模组文件");
                builder.AppendLine(GetExMsg(e));
            }
            if (tempPackage == null)
            {
                ApendLineLog(builder.ToString());
                return false;
            }
            int nIndex1 = filePath.LastIndexOf("\\");
            int nIndex2 = filePath.LastIndexOf(".");
            string pkName = filePath.Substring(nIndex1 + 1, nIndex2 - nIndex1 - 1);
            string modParentDir = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);

            List<IResourceIndexEntry> tempListEntry = tempPackage.GetResourceList;
            string strEntry = "";
            string casTypes = "";
            StringBuilder casBuilder = new StringBuilder();
            List<S4PoseInfo> poseList = new List<S4PoseInfo>();
            string poseDir = dirMod + "Mods/Pose/";
            if (!Directory.Exists(poseDir))
            {
                Directory.CreateDirectory(poseDir);
            }
            CasModInfo casModInfo = new CasModInfo();
            Dictionary<string, string> dicStrEntry = new Dictionary<string, string>();
            IResourceIndexEntry debugEntry = null;
            int poseCC = 0;
            int dataCounts = 0;
            bool isPose = false;
            ObjectMod objMod = null;
            Dictionary<string, int> unHandledTagAndCounts = new Dictionary<string, int>();

            bool isXieE = false;
            Dictionary<string, string> notDuplicateDic = new Dictionary<string, string>();
            //notDuplicateDic = new Dictionary<string, string>;
            List<IResourceIndexEntry> poseEntrys = new List<IResourceIndexEntry>();
            for (int i = 0; i < tempListEntry.Count; i++)
            {
                try
                {
                    IResourceIndexEntry entry = tempListEntry[i];
                    uint type = entry.ResourceType;
                    uint group = entry.ResourceGroup;
                    string typeKey = "" + type;
                    IResource res = null;
                    if (type != 0x015A1849)
                    {
                        poseEntrys.Add(entry);
                    }
                    else
                    {
                        continue;
                    }
                    string unHanldedKey = string.Format("0x{0:X8}-0x{1:X8}-{2}", type, group, false);
                    //bool useDefHandler = false;

                    //11720834为图片    2113017500为xml文件，解析xml文件得到图片和动作代码，文件的映射
                    //_xml
                    if (type == 2113017500)
                    {

                        if (notDuplicateDic.ContainsKey(typeKey))
                        {
                            continue;
                        }
                        else
                        {
                            notDuplicateDic.Add(typeKey, "has");
                        }
                        if (dataCounts > 0 || poseList.Count > 0)
                        {
                            continue;
                        }
                        string result = "";
                        try
                        {
                            res = getEntryRes(builder, tempPackage, entry, unHanldedKey, unHandledTagAndCounts);
                            if (res == null)
                            {
                                continue;
                            }
                            result = getResText(res, "Xml", builder);
                            poseList = ParseXmlTolist(result, builder);
                            poseCC++;
                            if (poseList == null || poseList.Count == 0)
                            {
                                //builder.AppendLine("[bug] No pose at Xml");
                            }
                            else
                            {
                                //poseEntrys.Add(entry);
                            }

                        }
                        catch (Exception e2)
                        {
                            builder.AppendLine("[error] xml 解析异常");
                            builder.AppendLine(GetExMsg(e2));
                            continue;
                        }
                    }//STBL
                    else if (type == 0x220557DA)
                    {

                        if (notDuplicateDic.ContainsKey(typeKey))
                        {
                            continue;
                        }
                        else
                        {
                            notDuplicateDic.Add(typeKey, "has");
                        }

                        res = getEntryRes(builder, tempPackage, entry, unHanldedKey, unHandledTagAndCounts);
                        StblResource.StblResource stblRes = (StblResource.StblResource)WrapperDealer.GetResource(0, tempPackage, entry, false);
                        StringEntryList mStrs = stblRes.Entries;
                        for (int m = 0; m < mStrs.Count; m++)
                        {
                            StringEntry sTry = mStrs[m];
                            uint kint = sTry.KeyHash;
                            string kvalue = sTry.StringValue;
                            string key = String.Format("0x{0:X8}", kint);
                            if (!dicStrEntry.ContainsKey(key))
                            {
                                dicStrEntry.Add(key, kvalue);
                            }
                            string dMsg = String.Format("stbl res key=0x{0:X8},value={1}", kint, kvalue);
                            Console.WriteLine(filePath + "\n" + dMsg);
                            //builder.AppendLine(dMsg);
                        }
                        if (res == null)
                        {
                            builder.AppendLine(String.Format("[bug] parse STBL failed", strEntry));
                            continue;
                        }
                        if (dicStrEntry.Count == 0)
                        {
                            builder.AppendLine(String.Format("[bug] STBL Entries=0,content={0}", strEntry));
                        }
                    }
                    //cas图片
                    else if (type == 0x00B2D882)
                    {
                    }
                    //导出CAS服装信息：
                    //BodyType: 0x00000001(Hat)
                    //BodySubType: 0x00000001
                    //AgeGender: 0x00002078(Teen, YoungAdult, Adult, Elder, Female)
                    else if (type == 0x034AEECB)
                    {
                        if (notDuplicateDic.ContainsKey(typeKey))
                        {
                            continue;
                        }
                        else
                        {
                            notDuplicateDic.Add(typeKey, "has");
                        }
                        if ("".Equals(casTypes))
                        {
                            //res = getEntryRes(builder, tempPackage, entry, unHanldedKey, unHandledTagAndCounts);

                            string casInfo = "";
                            try
                            {
                                //res = WrapperDealer.GetResource(0, tempPackage, entry, useDefHandler);
                                CASPartResource.CASPartResource mCas = (CASPartResource.CASPartResource)WrapperDealer.GetResource(0, tempPackage, entry, false);
                                casInfo = mCas.ToString();
                                string strType = mCas.BodyType.ToString();
                                string BodySubType = mCas.BodySubType.ToString();
                                string AgeGender = mCas.AgeGender.ToString();

                                string vMainType = strType;
                                string vSubType = BodySubType;
                                string vAgeGender = AgeGender;
                                casModInfo.modPath = filePath;
                                casModInfo.modType = vMainType;
                                casModInfo.modSubType = vSubType;
                                casModInfo.ageSuitable = vAgeGender;
                                casBuilder.AppendLine(string.Format("类别:{0}\n小分类:{1}\n适用年龄:{2}", vMainType, vSubType, vAgeGender));
                                casTypes = casBuilder.ToString();
                                if ("".Equals(vMainType) || "".Equals(vSubType) || "".Equals(vAgeGender))
                                {
                                    builder.AppendLine("[bug] CAS not found info,cas=" + casInfo);
                                }

                                if (!"".Equals(vMainType))
                                {
                                    putCasType(vMainType);
                                }
                                if (createCasInfoFile) {
                                    //生成分类文件
                                    string rMainType = vMainType;
                                    if (typeCName.ContainsKey(vMainType))
                                    {
                                        rMainType = typeCName[vMainType];
                                    }
                                    string infoTxtName = string.Format("{0}_cas分类_{1}.txt", pkName, rMainType);
                                    string absPath = modParentDir + infoTxtName;

                                    if (!File.Exists(absPath))
                                    {
                                        FileStream casFis = new FileStream(absPath, FileMode.OpenOrCreate);
                                        string wStr = casBuilder.ToString();
                                        byte[] bt = mCoding.GetBytes(wStr);
                                        casFis.Write(bt, 0, bt.Length);
                                        casFis.Close();
                                    }
                                }
                               

                            }
                            catch (Exception casEx)
                            {
                                casBuilder.AppendLine("[bug] casTypeInfo is not found ");
                                casBuilder.AppendLine("CASInfo:" + casInfo);
                                casBuilder.AppendLine(GetExMsg(casEx));
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    //家具OBJD 0xC0DB5AE7
                    else if (type == 0xC0DB5AE7)
                    {
                        if (notDuplicateDic.ContainsKey(typeKey))
                        {
                            continue;
                        }
                        else
                        {
                            notDuplicateDic.Add(typeKey, "has");
                        }
                        if (objMod == null)
                        {
                            res = getEntryRes(builder, tempPackage, entry, unHanldedKey, unHandledTagAndCounts);
                            if (res == null)
                            {
                                continue;
                            }
                            objMod = new ObjectMod();
                            StringBuilder objBuilder = new StringBuilder();
                            string resStr = getResText(res, "Object", builder);
                            StringBuilder sbob = new StringBuilder();
                            try
                            {
                                string tagName = "Name:";
                                string tagTuning = "Tuning:";
                                string tagPrice = "SimoleonPrice:";
                                string tagPoiEnvScore = "PositiveEnvironmentScore:";

                                string vName = getValueFromStr(resStr, tagName);
                                string vTuning = getValueFromStr(resStr, tagTuning);
                                string vPrice = getValueFromStr(resStr, tagPrice);
                                string vPoiEnvScore = getValueFromStr(resStr, tagPoiEnvScore);

                                if ("".Equals(vPoiEnvScore))
                                {
                                    sbob.AppendLine("not find vPoiEnvScore");
                                }
                                //string rMainType = vTuning;
                                //string infoTxtName = string.Format("{0}_物品分类_{1}.txt", pkName, rMainType);
                                //string absPath = modParentDir + infoTxtName;
                                //if (!File.Exists(absPath))
                                //{
                                //    FileStream casFis = new FileStream(modParentDir + infoTxtName, FileMode.OpenOrCreate);
                                //    string wStr = casBuilder.ToString();
                                //    byte[] bt = mCoding.GetBytes(wStr);
                                //    casFis.Write(bt, 0, bt.Length);
                                //    casFis.Close();
                                //}
                                objMod.name = vName.Trim();
                                objMod.tuning = vTuning.Trim();
                                objMod.price = vPrice.Trim();
                                objMod.score = vPoiEnvScore.Trim();
                                if ("".Equals(vName) || "".Equals(vTuning) || "".Equals(vTuning))
                                {
                                    builder.AppendLine(string.Format("illegal objMod,name:{0}\nTuning{1}\nSimoleonPrice{2}\nPositiveEnvironmentScore{3}\n TxtContent={4}",
                                    objMod.name, objMod.tuning, objMod.price, objMod.score, resStr));
                                }
                                builder.AppendLine(objBuilder.ToString());
                            }
                            catch (Exception casEx)
                            {
                                builder.AppendLine("[bug] ObjMod is not found");
                                builder.AppendLine("CASInfo:" + resStr);
                                builder.AppendLine(GetExMsg(casEx));
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    //0x7DF2169C邪恶包特殊Xml 0x0166038C _Key

                    else if (type == 0x7DF2169C)
                    {
                        isXieE = true;
                    }
                    //0x0166038C _Key 邪恶包
                    else if (type == 0x0166038C)
                    {
                        isXieE = true;
                    }
                    //功能性
                    else if (type == 0x545AC67A)
                    {


                        if (notDuplicateDic.ContainsKey(typeKey))
                        {
                            continue;
                        }
                        else
                        {
                            notDuplicateDic.Add(typeKey, "has");
                        }
                        try
                        {
                            DataResource data = (DataResource)WrapperDealer.GetResource(0, tempPackage, entry, false);
                        }
                        catch (Exception casEx)
                        {
                            builder.AppendLine("[err] get ability is errr");
                            builder.AppendLine(GetExMsg(casEx));
                        }
                        if (dataCounts == 0)
                        {
                            builder.AppendLine("is Ability");
                        }
                        else
                        {
                            continue;
                        }
                        dataCounts++;
                    }
                    //替换动作信息
                    else if (type == 0x6B20C4F3)
                    {
                        if (notDuplicateDic.ContainsKey(typeKey))
                        {
                            continue;
                        }
                        else
                        {
                            notDuplicateDic.Add(typeKey, "has");
                        }
                        if (isPose)
                        {
                            continue;
                        }
                        try
                        {
                            ClipResource clip = (ClipResource)WrapperDealer.GetResource(14, tempPackage, entry, false);
                            if (clip == null)
                            {
                                builder.AppendLine("[error] parse rpMod failed =" + filePath);
                                continue;
                            }
                            string clipName = clip.ClipName;
                            Console.WriteLine("clip " + clipName);
                            builder.AppendLine("[mod_th_pose] " + clipName);
                            tiLog.InfoNewLine("替换mod:" + filePath);
                            tiLog.InfoNewLine("gameName:" + clipName);
                            isPose = true;
                        }
                        catch (Exception e)
                        {
                            string errror = GetExMsg(e);
                            builder.AppendLine("[error] parse replace mod error=" + errror);
                        }

                    }
                }
                catch (Exception e)
                {
                    string errror = GetExMsg(e);
                    builder.AppendLine("[loop]loop error=" + errror);
                }
            }
            //记录getResource失败的entry信息
            if (unHandledTagAndCounts.Count > 0)
            {
                builder.AppendLine(string.Format("[bug] Could not find a resource handler"));
                foreach (string key in unHandledTagAndCounts.Keys)
                {
                    builder.AppendLine(string.Format("{0},Counts={1}", key, unHandledTagAndCounts[key]));
                }
            }
            bool makeOk = false;
            try
            {

                //记录替换动作
                if (poseList.Count == 0)
                {
                    if (isPose)
                    {
                        //既不是CAS mod也不是物品Object mod,也不能包含邪恶包xml
                        if ("".Equals(casTypes) && objMod == null&!isXieE)
                        {
                            if (removeTiHuan) {
                                addMovedToList(filePath, modSavedThDir);
                            }
                            builder.AppendLine("[debug] 当前文件为替换动作mod");
                        }
                    }
                }
                //导出功能模组
                if (dataCounts != 0)
                {
                    if ("".Equals(casTypes))
                    {
                        if (removeAbilityMod)
                        {
                            addMovedToList(filePath, modSavedAbDir);
                        }
                    }
                    builder.Append("功能模组entry="+dataCounts);
                }
                //处理CAS信息
                if (!"".Equals(casTypes))
                {
                    //string modParent = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                    //string desTypeFilePath = modParent + pkName + "_cas位置信息.txt";
                    //if (File.Exists(desTypeFilePath))
                    //{
                    //    File.Delete(desTypeFilePath);
                    //}
                    //Stream s = new FileStream(desTypeFilePath, FileMode.OpenOrCreate);
                    //byte[] b = mCoding.GetBytes(casTypes + "\n");
                    //s.Write(b, 0, b.Length);
                    //s.Close();
                    //string type = casModInfo.modType;
                    //string childPath = filePath.Substring(rootDir.Length);
                    //string newPath = dirMod + "CasMod/" + type + childPath;
                    //string newPathDir = newPath.Substring(0, newPath.LastIndexOf("\\"));
                    //if (!Directory.Exists(newPathDir))
                    //{
                    //    Directory.CreateDirectory(newPathDir);
                    //}
                    //FileInfo casPackInfo = new FileInfo(filePath);
                    //string desPath = dirMod + "CasMod/"+ type+"/";
                    //FileCopyKeepFilePath(filePath, desPath);
                    //DirectoryInfo casModDir = Directory.GetParent(filePath);
                    //FileInfo[] casPath = casModDir.GetFiles();
                    //for (int i = 0; i < casPath.Length; i++) {
                    //    //File f;
                    //    FileInfo fi = casPath[i];
                    //    if (".png".Equals(fi.Extension) || ".jpg".Equals(fi.Extension)
                    //        || ".bmp".Equals(fi.Extension) || ".webp".Equals(fi.Extension)
                    //        || ".jpeg".Equals(fi.Extension) || ".gif".Equals(fi.Extension))
                    //    {
                    //        childPath = fi.FullName.Substring(rootDir.Length);
                    //        newPath = dirMod + "CasMod/" + type + childPath;
                    //        newPathDir = newPath.Substring(0, newPath.LastIndexOf("\\"));
                    //        if (!Directory.Exists(newPathDir))
                    //        {
                    //            Directory.CreateDirectory(newPathDir);
                    //        }
                    //        if (!File.Exists(newPath))
                    //        {
                    //            //fi.CopyTo(newPath);
                    //        }
                    //    }
                    //    else {
                    //        if (!fi.FullName.Equals(casPackInfo.FullName)) {
                    //            //builder.AppendLine("[otherfile] " + fi.Name);
                    //        }
                    //    }
                    //}
                    ApendLineRecordCas(filePath+"\n"+ casTypes);
                }
                else
                {
                    AddNotCASCounts();
                }
                if (poseCC > 1)
                {
                    builder.AppendLine("[warn] pose 多个xml," + relatedPath);
                }
                if (poseList != null && poseList.Count > 0)
                {
                    AddPoseCounts();
                    //DirectoryInfo fileDir= Directory.GetParent(filePath);
                    //FileInfo[] tFile = fileDir.GetFiles();
                    //for (int i = 0; i < tFile.Length; i++)
                    //{
                    //    //File f;
                    //    FileInfo fi = tFile[i];
                    //    string childPath = "";
                    //    if (".png".Equals(fi.Extension)|| ".jpeg".Equals(fi.Extension) || ".jpg".Equals(fi.Extension) || ".webp".Equals(fi.Extension) ||
                    //        ".bmp".Equals(fi.Extension))
                    //    {
                    //        childPath = fi.FullName.Substring(rootDir.Length);
                    //        string srcName=fi.Name;
                    //        DirectoryInfo grandDir = fileDir.Parent;
                    //        string tempImageName = pkName+"$"+srcName;
                    //        string tempDir = poseDir + "动作列表/";
                    //        string tempImgDir= poseDir + "tempImg/";
                    //        string tempImgPath = tempImgDir + fileDir.Name+"@"+srcName;
                    //        string newPath = tempDir + tempImageName + fi.Extension;
                    //        if (!Directory.Exists(tempDir))
                    //        {
                    //            Directory.CreateDirectory(tempDir);
                    //        }
                    //        if (!Directory.Exists(tempImgDir))
                    //        {
                    //            Directory.CreateDirectory(tempImgDir);
                    //        }
                    //        if (!File.Exists(tempImgPath))
                    //        {
                    //            fi.CopyTo(tempImgPath);
                    //            if (!File.Exists(newPath))
                    //            {
                    //                fi.CopyTo(newPath);
                    //            }
                    //        }

                    //    }
                    //}

                    S4PoseInfo tempInfo = poseList[0];
                    string tempDexName = tempInfo.mainName;
                    string realName = "";
                    if (dicStrEntry.ContainsKey(tempDexName))
                    {
                        realName = dicStrEntry[tempDexName];

                        string[] leager = new string[] { "<", ">", "/", "\\", "\"", ":", "*", "?", "|" };

                        foreach (string ts in leager)
                        {
                            if (realName.Contains(ts))
                            {
                                builder.AppendLine("[bug] poseListName illeage " + realName);
                                realName = realName.Replace(ts, "$");
                            }
                        };
                        string newStr = string.Format("{0}游戏名称：《{1}》.txt", pkName, realName);
                        string newPoseName = modParentDir + newStr;
                        builder.AppendLine("make file_" + newStr);
                        if (!File.Exists(newPoseName))
                        {
                            FileStream fs = new FileStream(newPoseName, FileMode.OpenOrCreate);
                            fs.Close();
                        }
                        makeOk = true;
                    }
                    else
                    {
                        builder.AppendLine("[bug]  no Pose name for" + pkName);
                    }
                }
                //导出动作 图片
                //ExportPoseImg(filePath, builder, tempPackage, pkName, poseList, poseDir, dicStrEntry, poseEntrys,unHandledTagAndCounts);
            }
            catch (Exception e)
            {
                if (poseList != null && poseList.Count > 0)
                {
                    if (!makeOk)
                    {
                        builder.AppendLine("[bug] 没有生成动作对应名");
                    }
                }
                builder.AppendLine("[error] deal Package Error," + GetExMsg(e));
            }
            finally
            {
                try
                {
                    if (tempPackage != null)
                    {
                        tempPackage.Dispose();
                    }
                }
                catch (Exception closeEx)
                {
                    builder.AppendLine("[error] Dispose Package Error");
                    builder.AppendLine(GetExMsg(closeEx));
                }
                ApendLineLog(builder.ToString());
            }
            return true;
        }

        public void addMovedToList(string filePath,string desPath) {
            Dictionary<string, string> tempDic = new Dictionary<string, string>();
            tempDic.Add("src_path", filePath);
            tempDic.Add("target_path", desPath);
            waitToMove.Add(tempDic);
        }

        private IResource getEntryRes(StringBuilder builder,IPackage tempPackage,IResourceIndexEntry entry,string unHanldedKey,Dictionary<string,int> unHandledTagAndCounts) {
            bool useDefHandler = false;
            IResource res = null;
            try
            {
                res = WrapperDealer.GetResource(0, tempPackage, entry, useDefHandler);
            }
            catch (Exception e)
            {
                if (unHandledTagAndCounts.ContainsKey(unHanldedKey))
                {
                    unHandledTagAndCounts[unHanldedKey] = unHandledTagAndCounts[unHanldedKey] + 1;
                }
                else
                {
                    unHandledTagAndCounts.Add(unHanldedKey, 1);
                }
                builder.AppendLine(string.Format("[bug] get res Failed2 {0},msg={1}", unHanldedKey, GetExMsg(e)));
                //Could not find a resource handler
            }
            return res;
        }

        private string getCasInfoFromStr(string content, string tag)
        {
            string value = "";
            string casInfo = content;
            int index1 = content.IndexOf(tag);
            int dot1 = casInfo.IndexOf("(", index1);
            int dot2 = casInfo.IndexOf(")", dot1+1);
            value = casInfo.Substring(dot1 + 1, dot2 - dot1 - 1);
           
            return value;
        }
        private string getValueFromStr(string content, string tag) {
            string S = "";
            int index1 = content.IndexOf(tag);
            if (index1 >=0) {
                int index2 = content.IndexOf("\n", index1);
                if (index2 > index1)
                {
                    int start = index1 + tag.Length;
                    S = content.Substring(start, index2 - start);
                }
            }
            return S;
        }

        private void ExportPoseImg(string filePath, StringBuilder builder, IPackage tempPackage, string pkName, List<S4PoseInfo> poseList, string poseDir, Dictionary<string, string> dicStrEntry, List<IResourceIndexEntry> poseEntrys,Dictionary<string,int> mUnhandler
            )
        {
            //只导出pose图片
            if (poseList != null && poseList.Count > 0)
            {
                AddPoseCounts();
                for (int i = 0; i < poseEntrys.Count; i++)
                {
                    //图片img资源
                    IResourceIndexEntry entry = poseEntrys[i];
                    uint type = entry.ResourceType;
                    int reqApi = entry.RequestedApiVersion;
                    uint group = entry.ResourceGroup;
                    int recommendedApi = entry.RecommendedApiVersion;
                    string unHanldedKey = string.Format("0x{0:X8}-0x{1:X8}-{2}", type, group, false);
                    IResource res = getEntryRes(builder,tempPackage,entry,unHanldedKey, mUnhandler) ;
                    if (res == null)
                    {
                        continue;
                    }
                    Stream data = res.Stream;
                    ulong valueInstance = entry.Instance;
                    string str16 = valueInstance.ToString("X8");
                    //string str16 = Convert.ToString(valueInstance,16);
                    int len = 16 - str16.Length;
                    if (len > 0)
                    {
                        for (int l = 0; l < len; l++)
                        {
                            str16 = "0" + str16;
                        }
                    }
                    string path = "";

                    //以路径为参数创建文件
                    string formatPath = filePath.Replace("\\", "/");
                    path = formatPath.Substring(0, formatPath.LastIndexOf(".")) + "_" + str16 + ".jpg";
                    FileStream fs = null;
                    //   THUM 0x3C1AF1F2
                    //   _IMG 11720834
                    if (type == 11720834)
                    {
                        try
                        {
                            if (data == null || data == Stream.Null)
                            {
                                builder.AppendLine("[error]:data is null");
                                continue;
                            }
                            string detailPath = poseDir + "detailImg/";
                            if (!Directory.Exists(detailPath))
                            {
                                Directory.CreateDirectory(detailPath);
                            }
                            string otherDir = poseDir + "otherIcon/";
                            if (!Directory.Exists(otherDir))
                            {
                                Directory.CreateDirectory(otherDir);
                            }
                            path = otherDir + pkName + "@_mainIcon_" + str16 + ".jpg";
                            for (int k = 0; k < poseList.Count; k++)
                            {

                                S4PoseInfo info = poseList[k];
                                if (info.PoseIcon.ToUpper().Equals(str16.ToUpper()))
                                {
                                    string dexName = info.PoseName;
                                    string value = "";
                                    if (dicStrEntry.ContainsKey(dexName))
                                    {
                                        value = dicStrEntry[dexName];
                                    }
                                    else
                                    {
                                        value = dexName;
                                        builder.AppendLine("[error]not found value matched" + dexName + "\t@" + pkName);
                                    }
                                    string newName = string.Format("{1}({2}).jpg", "", value, info.PoseCmd.Replace(":", "%"));
                                    string tempDir = detailPath + pkName + "/";
                                    if (!Directory.Exists(tempDir))
                                    {
                                        Directory.CreateDirectory(tempDir);
                                    }
                                    path = tempDir + newName;
                                    info.imgPath = path;
                                    break;
                                }

                            }
                            if (path.Contains("mainIcon"))
                            {
                                if (str16 != poseList[0].mainIcon)
                                {
                                    path = otherDir + pkName + "@other_" + str16 + ".jpg";
                                }
                            }
                            try
                            {
                                if (File.Exists(path))
                                {
                                    continue;
                                }
                                fs = File.Open(path, FileMode.Create);
                                Stream resStream = data;
                                if (data == null || data.Length == 0)
                                {
                                    builder.AppendLine(string.Format("[bug] imgData is null, type={0},group={1}", type, group));
                                }

                                data.Position = 0;
                                s4pi.ImageResource.RLEResource.RLEInfo header = new s4pi.ImageResource.RLEResource.RLEInfo();
                                header.Parse(data);
                                if (header.pixelFormat.Fourcc == FourCC.DST1 || header.pixelFormat.Fourcc == FourCC.DST3 || header.pixelFormat.Fourcc == FourCC.DST5)
                                {
                                    data.Position = 0;
                                    resStream = (new DSTResource(1, data)).ToDDS();
                                }
                                resStream.Position = 0;
                                DdsFile ddsFile = new DdsFile();
                                ddsFile.Load(resStream, false);
                                Image img = ddsFile.Image;
                                if (img == null)
                                {
                                    builder.AppendLine("[error]:img is null");
                                    continue;
                                }
                                Image cImg = new Bitmap(img, new Size(img.Width, img.Height));
                                cImg.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
                                ddsFile.Dispose();
                                fs.Close();
                            }
                            catch (Exception we)
                            {
                                builder.AppendLine(GetExMsg(we));
                            }
                            finally
                            {
                                if (fs != null)
                                {
                                    fs.Close();
                                }
                            }

                        }
                        catch (Exception temp)
                        {
                            string s = "";
                            s += GetExMsg(temp);
                            IResourceIndexEntry rie = entry;
                            if (rie != null)
                            {
                                s += "Error reading resource " + rie;
                            }
                            s += string.Format("\r\nFront-end Distribution: {0}\r\nLibrary Distribution: {1}\r\n",
                           S4Version.CurrentVersion,
                             S4Version.LibraryVersion);
                            builder.AppendLine("[error] " + s);
                        }
                    }

                }
            }
        }

        public void FileMoveKeepFilePath(string filePath, string newDir)
        {
            string newPath = newDir + filePath.Substring(rootDir.Length);
            string newPathDir = newPath.Substring(0, newPath.LastIndexOf("\\"));
            FileInfo dataFile = new FileInfo(filePath);
            if (!Directory.Exists(newPathDir))
            {
                Directory.CreateDirectory(newPathDir);
            }
            if (File.Exists(newPath))
            {
                File.Delete(newPath);
            }
            dataFile.MoveTo(newPath);
        }
        public Dictionary<string, string> getReplaceMod (IResource res, string tag, List<string> keys)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            if (this.HasStringValueContentField(res))
            {
              string  mData = res["Value"];
            }
            else
            {
                string result = "";
                StreamReader stream = new StreamReader(res.Stream);
                while ((result = stream.ReadLine()) != null)
                {
                    string line = result;

                    for (int i = keys.Count - 1; i >= 0; i--)
                    {
                        string k = keys[i];
                        int index = line.IndexOf(k);
                        if (index != -1)
                        {
                            string value = line.Substring(index + k.Length);
                            dic.Add(k, value);
                            keys.Remove(k);
                        }
                    }
                    if (0 == keys.Count)
                    {
                        break;
                    }
                }
            }
          
            return dic;
        }
            public string getResText(IResource res, string tag, StringBuilder builder)
        {
            DateTime d1 = DateTime.Now;
            string result;
            if (this.HasStringValueContentField(res))
            {
                DateTime d11 = DateTime.Now;
                result = res["Value"];
                DateTime d2 = DateTime.Now;
                string dur2 = ExecDateDiff(d11, d2);
                double nDouble = Double.Parse(dur2);
                if (nDouble > 500)
                {
                    string dbugStr = string.Format("[debug] get {0} ResouceText by Value,use time={1}", tag, dur2);
                    Console.WriteLine(dbugStr);
                    builder.AppendLine(dbugStr);
                }
            }
            else
            {
                result = new StreamReader(res.Stream).ReadToEnd();
                DateTime d3 = DateTime.Now;
                string dur = ExecDateDiff(d1, d3);
                double dDur = Double.Parse(dur);
                string debugstr = string.Format("[debug] get {0} ResouceText by Stream,use time={1}", tag, dur);
                Console.WriteLine(debugstr);
                if (dDur > 100) {
                    builder.AppendLine(debugstr);
                }
            }
            return result;
        }
        public void FileCopyKeepFilePath(string filePath, string newDir)
        {
            string newPath = newDir + filePath.Substring(rootDir.Length);
            string newPathDir = newPath.Substring(0, newPath.LastIndexOf("\\"));
            FileInfo dataFile = new FileInfo(filePath);
            if (!Directory.Exists(newPathDir))
            {
                Directory.CreateDirectory(newPathDir);
            }
            if (!File.Exists(newPath))
            {
                dataFile.CopyTo(newPath);
            }
        }
        IBuiltInValueControl getValue(int type, Stream s)
        {
            IBuiltInValueControl ibvc =
             ABuiltInValueControl.Lookup((uint)type, s);
            return ibvc;
        }

        public List<S4PoseInfo> ParseXmlTolist(string xmlStr, StringBuilder builder2)

        {
            StringBuilder builder = new StringBuilder();
            List<S4PoseInfo> poses = new List<S4PoseInfo>();
            string mainIcon = "";
            string listName = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlStr);
                XmlNodeList rList = doc.LastChild.ChildNodes;
                foreach (XmlNode node in rList)
                {
                    string rName = node.Attributes[0].Value;
                    if ("icon".Equals(rName))
                    {
                        mainIcon = node.InnerText.Substring(node.InnerText.LastIndexOf(":") + 1);
                    }
                    else if ("display_name".Equals(rName))
                    {
                        listName = node.InnerText;
                    }
                }
                if (mainIcon.Equals(""))
                {
                    return poses;
                }
                XmlNodeList listNode = doc.GetElementsByTagName("L");
                if (listNode.Count == 0)
                {
                    return poses;
                }
                XmlNode ln = listNode[0];
                foreach (XmlNode courseNode in ln.ChildNodes)
                {
                    XmlNodeList finalValues = courseNode.ChildNodes;
                    S4PoseInfo poseInfo = new S4PoseInfo();
                    poseInfo.mainIcon = mainIcon;
                    poseInfo.mainName = listName;
                    foreach (XmlNode fn in finalValues)
                    {
                        string name = fn.Attributes[0].Value;
                        string asName = "";
                        string asValue = "";
                        switch (name)
                        {
                            case "pose_name":
                                asName = "动作指令:";
                                poseInfo.PoseCmd = fn.InnerText;
                                break;
                            case "pose_description":
                                asName = "动作描述:";
                                break;
                            case "pose_display_name":
                                asName = "显示名称:";
                                poseInfo.PoseName = fn.InnerText;
                                break;
                            case "icon":
                                asName = "显示图标名称（对照生成的图片文件名）:";
                                string value = fn.InnerText;
                                poseInfo.PoseIcon = value.Substring(value.LastIndexOf(":") + 1);
                                break;
                            case "sort_order":
                                asName = "序号";
                                break;
                        }
                        asValue = fn.InnerText;
                        if (name.Equals("icon"))
                        {
                            asValue = asValue.Substring(asValue.LastIndexOf(":") + 1);
                        }
                        //builder.AppendLine(asName + asValue);
                    }
                    if (poseInfo.PoseName == null)
                    {
                        builder.AppendLine("[bug] PoseName null ,XML=" + xmlStr);
                    }
                    if (poseInfo.PoseCmd != null)
                    {
                        poses.Add(poseInfo);
                    }
                }
                ApendLineLog(builder.ToString());
            }
            catch (Exception e)
            {
                builder.AppendLine("[error] xml parse");
                builder.AppendLine(GetExMsg(e));
                ApendLineLog(builder.ToString());
            }
            return poses;
        }


        private bool HasStringValueContentField(IResource res)
        {
            if (!res.ContentFields.Contains("Value"))
            {
                return false;
            }

            Type t = AApiVersionedFields.GetContentFieldTypes(0, res.GetType())["Value"];
            if (typeof(string).IsAssignableFrom(t))
            {
                return true;
            }

            return false;
        }
        public string GetExMsg(Exception e)
        {
            StringBuilder builder = new StringBuilder();
            Exception tempEx = e;
            if (tempEx == null)
            {
                return "null parameter e!!!!";
            }
            builder.AppendLine("[error] msg: " + tempEx.Message);
            builder.AppendLine("detail: " + tempEx.StackTrace);
            while (tempEx.InnerException != null)
            {
                tempEx = tempEx.InnerException;
                builder.AppendLine("--causedBy: " + tempEx.Message);
                builder.AppendLine("--innerEx: " + tempEx.StackTrace);
            }
            return builder.ToString();
        }

    }



    public class HLogHandler
    {
        public BufferedStream bs;
        public int bufSize = 1024 * 10;
        public HLogHandler(string filePath)
        {
            try
            {
                DirectoryInfo mDinfo=Directory.GetParent(filePath);
                if (mDinfo != null) {
                    if (!mDinfo.Exists)
                    {
                        mDinfo.Create();
                    }
                }
                Stream outStream = File.Open(filePath, FileMode.Create);
                bs = new BufferedStream(outStream, bufSize);
            }
            catch (Exception e) {
                Console.Error.WriteLine($"创建log文件失败,路径{filePath},"+e.Message + ",stack=" + e.StackTrace);
            }
           

        }
        public void InfoNewLine(string info)
        {
            if (bs != null)
            {
                byte[] b = PackageHandler.mCoding.GetBytes(  $"{newNowStr()} {info}\n");
                bs.Write(b, 0, b.Length);
            }
        }

        public static string newNowStr() {
            string res =DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff");
            return res;
        }

        public void Close()
        {
            if (bs != null)
            {
                bs.Flush();
                bs.Close();
                bs = null;
            }
        }

    }
    public class CASRecorder : HLogHandler
    {
        public CASRecorder(string filePath) : base(filePath)
        {
            bs.Close();
            bufSize = 10 * 1024;
            bs = new BufferedStream(new FileStream(filePath, FileMode.OpenOrCreate));
        }

        public new void InfoNewLine(string info)
        {
            if (bs != null)
            {
                byte[] b =PackageHandler.mCoding.GetBytes(info + "\n");
                bs.Write(b, 0, b.Length);
            }
        }
    }

    public class TihuanModRecoder : HLogHandler
    {
        public TihuanModRecoder(string filePath) : base(filePath)
        {
            bs.Close();
            bufSize = 10 * 1024;
            bs = new BufferedStream(new FileStream(filePath, FileMode.OpenOrCreate));
        }

        public new void InfoNewLine(string info)
        {
            if (bs != null)
            {
                byte[] b = PackageHandler.mCoding.GetBytes(info + "\n");
                bs.Write(b, 0, b.Length);
            }
        }
    }
    public class CasTypeRecorder : HLogHandler
    {
        int fileCounts = 0;
        Dictionary<string, int> dict = new Dictionary<string, int>();
        public CasTypeRecorder(string filePath) : base(filePath)
        {
        }
        public void putType(string typeKey)
        {
            if (!dict.ContainsKey(typeKey))
            {
                dict.Add(typeKey, 1);
            }
            else
            {
                int counts = dict[typeKey];
                counts = counts + 1;
                dict[typeKey] = counts;
            }

            addFile();
        }
        public void addFile()
        {
            fileCounts++;
        }
        public int GetCasFileCounts() {
            int count = 0;
            foreach (var item in dict)
            {
                count += item.Value;
            }
            return count;
        }
        public new void Close()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("类别\t\t个数");
            int totalCount = 0;
            foreach (var item in dict)
            {
                sb.AppendLine(item.Key + "\t\t" + item.Value);
                totalCount += item.Value;
            }
            sb.AppendLine("=======结果=======");
            sb.AppendLine("所有分类过的CasMod数量为：" + totalCount);
            byte[] resultBytes = PackageHandler.mCoding.GetBytes(sb.ToString());
            bs.Write(resultBytes, 0, resultBytes.Length);
            base.Close();
        }
    }
}
