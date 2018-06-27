using s4pi.ImageResource;
using s4pi.Interfaces;
using s4pi.Package;
using s4pi.WrapperDealer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using S4Version = S4PIDemoFE.Settings.Version;
namespace S4PIDemoFE
{
    public class PackageHandler
    {

        public static  string dirMod = "D:/S4pe_Files/";
        HLogHandler hLogHandler = null;
        CASRecorder cRecoder = null;
        CasTypeRecorder coutsRecorder = null;
        int poseCounts = 0;
        int notCASMod= 0;
        string rootDir = "";

        public PackageHandler(string parentDir)
        {
            this.rootDir = parentDir;
        }

        public CASRecorder CRecoder { get => cRecoder; set => cRecoder = value; }
        public HLogHandler HLogHandler { get => hLogHandler; set => hLogHandler = value; }
        public CasTypeRecorder CoutsRecorder { get => coutsRecorder; set => coutsRecorder = value; }

        public void ApendLineLog(string info) {
            if (hLogHandler != null) {
                hLogHandler.InfoNewLine(info);
            }
        }
        public void ApendLineRecordCas(string s) {
            if (CRecoder != null) {
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
        public void AddPoseCounts() {
            poseCounts++;
        }
        public void AddNotCASCounts()
        {
            notCASMod++;
        }
        public void addCasCounts() {
            if (CoutsRecorder != null) {
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
        public void Release ()
        {
            if (CRecoder != null) {
                cRecoder.Close();
            }
            if (HLogHandler != null) {
                HLogHandler.Close();
            }
            if (CoutsRecorder != null)
            {
                CoutsRecorder.Close();
            }
            //throw new Exception();
        }
      
        public static string[] GetFileNames(string directoryPath, string searchPattern, bool isSearchChild)
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


        public string exportAll(string dirPath,MainForm form)
        {
          
           
            if (!Directory.Exists(dirMod))
            {
                Directory.CreateDirectory(dirMod);
            }
            string[] files = null;
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
            DateTime t1 = DateTime.Now;
            int len = files.Length;
            try
            {

                for (int i = 0; i < len; i++)
            {
                string s = files[i];
                int persent = i * 100 / len;
                ReadPackageAndExport(s, null);

                ApendLineLog("next----------");
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
            ApendLineLog("动作mod数量:"+poseCounts);
            ApendLineLog("所有mod数量:" + len);
            ApendLineLog("非CAS mod数量:" + notCASMod);
            int duration = (int)((DateTime.Now - t1).TotalMilliseconds);
            int minute = duration / 1000 / 60;
            int second = duration / 1000;
            string timeLong = "";
            if (minute == 0)
            {
                timeLong = second + "毫秒";
            }
            else
            {
                timeLong = minute + "分" + second + "毫秒";
            }
            return "导出完成，总共找到模组文件：" + len + ",耗时：" + timeLong;
        }

        public bool ReadPackageAndExport(string filePath, StringBuilder builder2)
        {
            StringBuilder builder = new StringBuilder();
            IPackage tempPackage = null;
            try
            {
                tempPackage = Package.OpenPackage(0, filePath);
            }
            catch (Exception e)
            {
                builder.AppendLine("[OpenError]" + filePath);
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
            List<IResourceIndexEntry> tempListEntry = tempPackage.GetResourceList;
            string strEntry = "";
            string casTypes = "";
            StringBuilder casBuilder = new StringBuilder();
            List<S4PoseInfo> poseList = new List<S4PoseInfo>();
            string poseDir = dirMod + "Pose/";
            if (!Directory.Exists(poseDir))
            {
                Directory.CreateDirectory(poseDir);
            }
            CasModInfo casINFO = new CasModInfo();
            Dictionary<string, string> dicStrEntry = new Dictionary<string, string>();
            IResourceIndexEntry debugEntry = null;
            int poseCC = 0;
            int dataCounts = 0;
            bool isPose = false;
            ObjectMod objMod = null;
            Dictionary<string, int> unHandledTagAndCounts = new Dictionary<string, int>();
            List<IResourceIndexEntry> poseEntrys = new List<IResourceIndexEntry>();
            for (int i = 0; i < tempListEntry.Count; i++)
            {
                IResourceIndexEntry entry = tempListEntry[i];
                uint type = entry.ResourceType;
                int reqApi = entry.RequestedApiVersion;
                uint group = entry.ResourceGroup;
                int recommendedApi = entry.RecommendedApiVersion;
                if (reqApi != 0 || recommendedApi != 2)
                {
                    builder.AppendLine(String.Format("reqApi:{0} recommendedApi:{1}", reqApi, recommendedApi));
                }
                //builder.AppendLine(string.Format("[info]type:{0},group:{1}", type.ToString("X8"), group.ToString("X8")));
                //if (type== 55242443&&group== 2147483648) {
                //builder.AppendLine(string.Format("[info]type:{0},group:{1}", type.ToString("X8"), group.ToString("X8")));
                //}
                //can't deal with:
                //RMAP   2887187436
                //if (type == 55242443 && group == 2147483648)
                //{
                //    continue;
                //}
                IResource res = null;
                if (type == 0xBC4A5044 || type == 0x6B20C4F3 || type == 0xBC4A5044 || type == 0x220557DA || type == 0x015A1849
                       || type == 0x034AEECB || type == 0xAC16FBEC)
                {
                    //if(group)
                }
                else
                {
                    poseEntrys.Add(entry);
                }
                bool useDefHandler = false;
                if (type == 0x015A1849)
                {
                    useDefHandler = true;
                }
                string unHanldedKey = string.Format("0x{0:X8}-0x{1:X8}-{2}", type,group,useDefHandler);
                try
                {
                    res = WrapperDealer.GetResource(0, tempPackage, entry, useDefHandler);
                    if (type == 0xBC4A5044 || type == 0x6B20C4F3 || type == 0xBC4A5044 || type == 0x220557DA || type == 0x015A1849
                        || type == 0x034AEECB || type == 0xAC16FBEC)
                    {
                        if (group != 0x80000000 && group != 0x00000000)
                        {
                            //034AEECB
                            //AC16FBEC
                            builder.AppendLine(string.Format("[OkRead]reading resource 0x{0:X8}-0x{1:X8}-0x{2:X8},group Not format", type, group, entry.Instance));
                        }
                        else
                        {
                            builder.AppendLine(string.Format("[OkRead]reading resource 0x{0:X8}-0x{1:X8}-0x{2:X8}", type, group, entry.Instance));
                        }
                    }
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

                    //无法在流的结尾之外进行读取
                    //string resError=e.StackTrace;
                    //int fIndex = resError.IndexOf("PackageHandler");
                    //string errrStr = "";
                    //if (fIndex == -1)
                    //{
                    //    errrStr = "not have PackageHandler";
                    //}
                    //else {
                    //    errrStr = resError.Substring(fIndex, resError.Length - fIndex);
                    //}
                    //builder.AppendLine("msg:"+e.Message);
                    //builder.AppendLine("at:"+ errrStr);
                    //Exception child = e.InnerException;
                    //if (child != null) {
                    //    builder.AppendLine("detail:" + child.StackTrace);
                    //}
                }
                if (res == null)
                {
                    continue;
                }
                //11720834为图片    2113017500为xml文件，解析xml文件得到图片和动作代码，文件的映射
                //_xml
                if (type == 2113017500)
                {
                    string result = "";
                    try
                    {
                        if (this.HasStringValueContentField(res))
                        {
                            result = res["Value"];
                            builder.AppendLine("[debug] XML get result By :HasStringValueContentField:");
                        }
                        else
                        {
                            result = new StreamReader(res.Stream).ReadToEnd();
                            builder.AppendLine("[debug] XML get result By :StreamReader.ReadToEnd:");
                        }
                        poseList = ParseXmlTolist(result, builder);
                        poseCC++;
                        if (poseList == null || poseList.Count == 0)
                        {
                            builder.AppendLine("[bug] No pose at :" + filePath);
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
                }
                else if (type == 11720834)
                {
                    //poseEntrys.Add(entry);
                }

                //STBL
                else if (type == 0x220557DA)
                {
                    if (strEntry.Equals(""))
                    {
                        if (this.HasStringValueContentField(res))
                        {
                            strEntry = res["Value"];
                            builder.AppendLine("[debug] get STBL result By  value");
                        }
                        else
                        {
                            strEntry = new StreamReader(res.Stream).ReadToEnd();
                            builder.AppendLine("[debug] get STBL result By  readToEnd");
                        }
                        int start = 0;
                        builder.AppendLine("STBL　Key-value ");
                        while ((start = strEntry.IndexOf("Key", start + 1)) != -1)
                        {
                            try
                            {
                                int maohaoIndex = strEntry.IndexOf(":", start);
                                int dotIndex = strEntry.IndexOf(",", start);
                                int endIndex = strEntry.IndexOf("\n", start);
                                string key = strEntry.Substring(start + 4, dotIndex - start - 4);
                                string value = strEntry.Substring(maohaoIndex + 2, endIndex - maohaoIndex - 2);
                                if (dicStrEntry.ContainsKey(key))
                                {
                                    builder.AppendLine("[bug] STBL key 多次重复");
                                    builder.AppendLine("content:\n" + strEntry);
                                }
                                else
                                {
                                    dicStrEntry.Add(key, value);
                                }
                                //builder.AppendLine("id:" + key + "\tvalue:" + value);
                            }
                            catch (Exception e)
                            {
                                builder.AppendLine("[error] stbl not format");
                                builder.AppendLine("STBL:" + strEntry);
                                builder.AppendLine(GetExMsg(e));
                            }

                            //string name=strEntry.Substring()
                        }
                        if (dicStrEntry.Count == 0)
                        {
                            builder.AppendLine(String.Format("bug STBL:{0}", strEntry));
                        }
                    }
                }
                //导出CAS服装信息：
                //BodyType: 0x00000001(Hat)
                //BodySubType: 0x00000001
                //AgeGender: 0x00002078(Teen, YoungAdult, Adult, Elder, Female)
                else if (type == 0x034AEECB)
                {
                    if ("".Equals(casTypes))
                    {
                        casTypes += "";
                        casBuilder.AppendLine("文件名:" + filePath);
                        string casInfo = "";
                        if (this.HasStringValueContentField(res))
                        {
                            casInfo = res["Value"];
                            casBuilder.AppendLine("[debug] get result By  value");
                        }
                        else
                        {
                            casInfo = new StreamReader(res.Stream).ReadToEnd();
                            casBuilder.AppendLine("[debug] get result By  readToEnd");
                        }
                        string errorIndex = "";
                        try
                        {
                            int index1 = casInfo.IndexOf("BodyType:");
                            int index2 = casInfo.IndexOf("BodySubType");
                            int index3 = casInfo.IndexOf("AgeGender:");
                            int end1 = casInfo.IndexOf("\n", index1);
                            int end2 = casInfo.IndexOf("\n", index2);
                            int end3 = casInfo.IndexOf("\n", index3);
                            if (index1 >= 0 && end1 >= 0)
                            {
                                errorIndex += "BodyType:" + index1;
                                int dot1 = casInfo.IndexOf("(", index1);
                                int dot2 = casInfo.IndexOf(")", index1);
                                //casBuilder.AppendLine("类别：" + casInfo.Substring(index1 + 8, end1 - index1 - 8));
                                casBuilder.AppendLine("类别：" + casInfo.Substring(dot1 + 1, dot2 - dot1 - 1));
                                casINFO.modPath = filePath;
                                casINFO.modType = casInfo.Substring(dot1 + 1, dot2 - dot1 - 1);
                                CoutsRecorder.putType(casInfo.Substring(dot1 + 1, dot2 - dot1 - 1));
                            }
                            if (index2 >= 0 && end2 >= 0)
                            {
                                casBuilder.AppendLine("子类别：" + casInfo.Substring(index2 + 11, end2 - index2 - 11));

                            }
                            if (index3 >= 0 && end3 >= 0)
                            {
                                errorIndex += "AgeGender:" + index3;
                                int dot1 = casInfo.IndexOf("(", index3);
                                int dot2 = casInfo.IndexOf(")", index3);
                                casINFO.ageSuitable = casInfo.Substring(dot1 + 1, dot2 - dot1 - 1);
                                casBuilder.AppendLine("适用年龄：" + casInfo.Substring(dot1 + 1, dot2 - dot1 - 1));
                            }
                            //FileStream casFis = new FileStream(filePath + "cas位置.txt", FileMode.OpenOrCreate);
                            //string wStr = casBuilder.ToString();
                            //byte[] bt = Encoding.UTF8.GetBytes(wStr);
                            //casFis.Write(bt, 0, bt.Length);
                            //casFis.Close();
                        }
                        catch (Exception casEx)
                        {
                            casBuilder.AppendLine("[bug] casTypeInfo is not found at " + filePath);
                            casBuilder.AppendLine("indexs :" + errorIndex);
                            casBuilder.AppendLine("CASInfo:" + casInfo);
                            casBuilder.AppendLine(GetExMsg(casEx));
                        }
                    }
                    casTypes = casBuilder.ToString();

                }
                //家具OBJD 0xC0DB5AE7
                else if (type == 0xC0DB5AE7)
                {
                    if (objMod == null)
                    {
                        objMod = new ObjectMod();
                        builder.AppendLine("OBJ文件名:" + filePath);
                        string resStr = "";
                        if (this.HasStringValueContentField(res))
                        {
                            resStr = res["Value"];
                            builder.AppendLine("[debug] get Object result By  value");
                        }
                        else
                        {
                            resStr = new StreamReader(res.Stream).ReadToEnd();
                            builder.AppendLine("[debug] get Object result By  ReadToEnd");

                        }
                        string errorIndex = "";
                        try
                        {
                            //SimoleonPrice: 0x00000032 
                            //        PositiveEnvironmentScore: 1.0000
                            int index1 = resStr.IndexOf("Name");
                            int index2 = resStr.IndexOf("Tuning");
                            int index3 = resStr.IndexOf("SimoleonPrice");
                            int index4 = resStr.IndexOf("PositiveEnvironmentScore");
                            errorIndex += "1:" + index1;
                            errorIndex += "2:" + index2;
                            errorIndex += "3:" + index3;
                            errorIndex += "4:" + index4;
                            int dot1 = resStr.IndexOf(":", index1);
                            int dot2 = resStr.IndexOf(":", index2);
                            int dot3 = resStr.IndexOf(":", index3);

                            objMod.name = resStr.Substring(dot1 + 2, resStr.IndexOf("\n", index1) - dot1 - 2);
                            objMod.tuning = resStr.Substring(dot2 + 2, resStr.IndexOf("\n", index2) - dot2 - 2);
                            objMod.price = resStr.Substring(dot3 + 2, resStr.IndexOf("\n", index3) - dot3 - 2);
                            if (index4 != -1)
                            {
                                int dot4 = resStr.IndexOf(":", index4);
                                objMod.score = resStr.Substring(dot4 + 2, resStr.IndexOf("\n", index4) - dot4 - 2);
                            }
                            else
                            {
                                objMod.score = "0";
                            }
                            errorIndex += "name：" + objMod.name + "\n";
                            errorIndex += "Tuning：" + objMod.tuning + "\n";
                            errorIndex += "SimoleonPrice：" + objMod.price + "\n";
                            errorIndex += "PositiveEnvironmentScore：" + objMod.score + "\n";
                            builder.AppendLine(errorIndex);
                        }
                        catch (Exception casEx)
                        {

                            builder.AppendLine("[bug] ObjMod is not found at " + filePath);
                            builder.AppendLine("indexs :" + errorIndex);
                            builder.AppendLine("CASInfo:" + resStr);
                            builder.AppendLine(GetExMsg(casEx));
                        }
                    }
                }
                //功能性
                else if (type == 0x545AC67A) {
                    if (dataCounts == 0) {
                        builder.AppendLine("[ability]" + filePath);
                    }
                    dataCounts++;
                }
                //替换动作信息
                else if (type == 0x6B20C4F3)
                {
                    string resStr = "";
                    if (this.HasStringValueContentField(res))
                    {
                        resStr = res["Value"];
                        builder.AppendLine("[debug] get RelpacePose result By  value");
                    }
                    else
                    {
                        resStr = new StreamReader(res.Stream).ReadToEnd();
                        builder.AppendLine("[debug] get RelpacePose result By  ReadToEnd");
                    }
                    int indexName = resStr.IndexOf("ClipName:");
                    if (indexName != -1)
                    {
                        int indexDot = resStr.IndexOf("\n", indexName);
                        string clipName = resStr.Substring(indexName, indexDot - indexName);
                        isPose = true;
                    }
                }
            }
            //记录getResource失败的entry信息
            if (unHandledTagAndCounts.Count > 0) {
                builder.AppendLine("currentFile:" + filePath);
                foreach (string key in unHandledTagAndCounts.Keys) {
                    builder.AppendLine(string.Format("[bug]reading resource {0},Counts={1}", key,unHandledTagAndCounts[key]));
                }
            }
            //记录替换动作
            if (poseList.Count == 0)
            {
                if (isPose)
                {
                    //既不是CAS mod也不是物品Object mod
                    if ("".Equals(casTypes) && objMod == null)
                    {
                        string desPath = dirMod + "替換mod/";
                        //string newPath = desPath + pkName + ".package";
                        FileCopyKeepFilePath(filePath, desPath);

                        builder.AppendLine("[debug] " + filePath + " 是替换动作mod");
                    }
                }
            }
            if (dataCounts != 0) {
                if ("".Equals(casTypes))
                {
                    string desPath = dirMod + "功能mod/";
                    //string newPath = desPath + pkName + ".package";
                    FileCopyKeepFilePath(filePath, desPath);
                  
                    if (!Directory.Exists(desPath))
                    {
                        Directory.CreateDirectory(desPath);
                    }
                    string abLog = desPath + "log.txt";
                    FileStream abStream = new FileStream(abLog, FileMode.Append);
                    byte[] b = System.Text.Encoding.UTF8.GetBytes(filePath + "\n");
                    abStream.Write(b, 0, b.Length);
                    abStream.Close();
                }
            }
            if (!"".Equals(casTypes))
            {
                string modParent = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                string desTypeFilePath = modParent + pkName + "_cas位置信息.txt";
                if (!File.Exists(desTypeFilePath)) {
                    Stream s = new FileStream(desTypeFilePath,FileMode.OpenOrCreate);
                    byte[] b = Encoding.UTF8.GetBytes(casTypes + "\n");
                    s.Write(b, 0, b.Length);
                    s.Close();
                }
                string type = casINFO.modType;
                string childPath = filePath.Substring(rootDir.Length);
                string newPath = dirMod + "CasMod/" + type + childPath;
                string newPathDir = newPath.Substring(0, newPath.LastIndexOf("\\"));
                if (!Directory.Exists(newPathDir))
                {
                    Directory.CreateDirectory(newPathDir);
                }
                FileInfo casPackInfo = new FileInfo(filePath);
                if (!File.Exists(newPath)) {
                    //casPackInfo.CopyTo(newPath);
                }
                DirectoryInfo casModDir = Directory.GetParent(filePath);
                FileInfo[] casPath = casModDir.GetFiles();
                for (int i = 0; i < casPath.Length; i++) {
                    //File f;
                    FileInfo fi = casPath[i];
                    if (!".package".Equals(fi.Extension)) {
                         childPath = fi.FullName.Substring(rootDir.Length);
                         newPath = dirMod + "CasMod/" + type + childPath;
                         newPathDir = newPath.Substring(0, newPath.LastIndexOf("\\"));
                        if (!Directory.Exists(newPathDir)) {
                            Directory.CreateDirectory(newPathDir);
                        }
                        if (!File.Exists(newPath)) {
                            //fi.CopyTo(newPath);
                        }
                    }
                }
                ApendLineRecordCas(casTypes);
            }
            else {
                AddNotCASCounts();
                builder.AppendLine(string.Format("[debug] file '{0}', 不是CAS mod",filePath));
            }
            if (poseCC > 1)
            {
                builder.AppendLine("[poses] 多个xml" + filePath);
            }
            if (poseList != null && poseList.Count > 0)
            {

                DirectoryInfo fileDir= Directory.GetParent(filePath);
                FileInfo[] tFile = fileDir.GetFiles();
                for (int i = 0; i < tFile.Length; i++)
                {
                    //File f;
                    FileInfo fi = tFile[i];
                    string childPath = "";
                    if (".png".Equals(fi.Extension)|| ".jpeg".Equals(fi.Extension) || ".jpg".Equals(fi.Extension) || ".webp".Equals(fi.Extension) ||
                        ".bmp".Equals(fi.Extension))
                    {
                        childPath = fi.FullName.Substring(rootDir.Length);
                        string srcName=fi.Name;
                        DirectoryInfo grandDir = fileDir.Parent;
                        string tempImageName = pkName+"$"+srcName;
                        string tempDir = poseDir + "动作列表/";
                        string tempImgDir= poseDir + "tempImg/";
                        string tempImgPath = poseDir + "tempImg/"+ fileDir.Name+"@"+srcName;
                        string newPath = tempDir + tempImageName + fi.Extension;
                        if (!Directory.Exists(tempDir))
                        {
                            Directory.CreateDirectory(tempDir);
                        }
                        if (!Directory.Exists(tempImgDir))
                        {
                            Directory.CreateDirectory(tempImgDir);
                        }
                        if (!File.Exists(tempImgPath))
                        {
                            fi.CopyTo(tempImgPath);
                            if (!File.Exists(newPath))
                            {
                                fi.CopyTo(newPath);
                            }
                        }
                       
                    }
                }
                string[] leager = new string[] {"<", ">", "/", "\\", "\"", ":", "*", "?", "|" };
                for (int i = 0; i < poseList.Count; i++)
                {

                    S4PoseInfo info = poseList[i];
                    string dexName = info.mainName;
                    string value = "";
                    if (dicStrEntry.ContainsKey(dexName))
                    {
                        value = dicStrEntry[dexName];
                    }
                    if (!value.Equals(""))
                    {
                        string modParent = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                        foreach (string ts in leager) {
                            if (value.Contains(ts)) {
                                value= value.Replace(ts, "$");
                                builder.AppendLine("[bug] poseListName illeage"+filePath);
                            }
                        };
                        string newPoseName = modParent + string.Format("{0}对应的动作名称：《{1}》.txt", pkName, value);
                        if (!File.Exists(newPoseName))
                        {
                            FileStream fs = new FileStream(newPoseName, FileMode.OpenOrCreate);
                            fs.Close();
                        }
                    }
                    else
                    {
                        builder.AppendLine("no Pose name" + filePath);
                    }
                }
            }
            //只导出pose
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
                    IResource res = null;
                    bool useDefHandler = false;
                    if (type == 0x015A1849)
                    {
                        useDefHandler = true;
                    }
                    //type = '55242443',group = '2147483648'
                    //if (type == 55242443 && group == 2147483648) {
                    //    continue;
                    //}
                    try
                    {
                        res = WrapperDealer.GetResource(recommendedApi, tempPackage, entry, useDefHandler);
                    }
                    catch (Exception e)
                    {
                        builder.AppendLine("PoseFile:" + filePath);
                        builder.AppendLine(string.Format("[bug]PoseList GetResource byDefHandler='{0}',type='{1}',group='{2}'", useDefHandler, type, group));
                        builder.AppendLine(GetExMsg(e));
                    }
                    if (res == null)
                    {
                        builder.AppendLine("");
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
                    string formatPath = filePath.Replace("\\\\", "/");
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
                            string detailPath =poseDir+ "detailImg/";
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
                                    else {
                                        value = dexName;
                                        builder.AppendLine("[error]not found value matched" + dexName + "\t@" + pkName);
                                    }
                                    string newName = string.Format("{0}@{1}({2}).jpg", pkName, value, info.PoseCmd.Replace(":", "%"));
                                    path = detailPath + newName;
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
                                builder.AppendLine(String.Format("[bug]当前图片与动作列表中的图片不相关 instance16:{0}", str16));
                            }
                            builder.AppendLine("[debug]:use imgPath " + path);
                            try
                            {
                                if (File.Exists(path)) {
                                    continue;
                                }
                                fs = File.Open(path, FileMode.Create);
                                Stream resStream = data;
                                if (data == null || data.Length == 0)
                                {
                                    builder.AppendLine(string.Format("[bug] imgData is null, type={0},group={1}",type,group));
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
                                builder.AppendLine("[atFile]:" + filePath);
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
                            builder.AppendLine(String.Format("[error] at file:{0},\ndeTail:{1}", filePath, s));
                        }
                    }

                }
            }
            try
            {
                if (tempPackage != null) {
                    tempPackage.Dispose();
                }
            }
            catch (Exception closeEx){
                builder.AppendLine("[error] Dispose Package Error");
                builder.AppendLine(GetExMsg(closeEx));
            }
            ApendLineLog(builder.ToString());
            return true;
        }
        public void FileMoveKeepFilePath(string filePath,string newDir) {
            string newPath = newDir+ filePath.Substring(rootDir.Length);
            string newPathDir = newPath.Substring(0, newPath.LastIndexOf("\\"));
            FileInfo dataFile = new FileInfo(filePath);
            if (!Directory.Exists(newPathDir))
            {
                Directory.CreateDirectory(newPathDir);
            }
            if (!File.Exists(newPath))
            {
                dataFile.MoveTo(newPath);
            }
        }
        public void FileCopyKeepFilePath(string filePath, string newDir)
            {
            string newPath = newDir+filePath.Substring(rootDir.Length);
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
                XmlNodeList  rList=doc.LastChild.ChildNodes;
                foreach( XmlNode node in rList){
                    string rName=node.Attributes[0].Value;
                    if ("icon".Equals(rName)) {
                        mainIcon = node.InnerText.Substring(node.InnerText.LastIndexOf(":") + 1);
                        builder.AppendLine("MainIcon:" + mainIcon);
                    } else if ("display_name".Equals(rName)) {
                        listName = node.InnerText;
                    }
                }
                if (mainIcon.Equals("")) {
                    return poses;
                }
                XmlNodeList listNode = doc.GetElementsByTagName("L");
                if (listNode.Count == 0) {
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
                    if (poseInfo.PoseName == null) {
                        builder.AppendLine("[bug] PoseName null ,XML=" + xmlStr);
                    }
                    if (poseInfo.PoseCmd != null) {
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
                return poses;
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
        public  string GetExMsg(Exception e)
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



    public class HLogHandler{
        public BufferedStream bs;
        public  int bufSize = 1024 * 100;
        public HLogHandler(string filePath)
        {
            Stream outStream = File.Open(filePath, FileMode.Create);
             bs = new BufferedStream(outStream, bufSize);

        }
        public void InfoNewLine(string info) {
            if (bs != null)
            {
                byte[] b = Encoding.UTF8.GetBytes(info + "\n");
                bs.Write(b, 0, b.Length);
            }
        }
        public void Close()
        {
            if (bs != null)
            {
                bs.Flush();
                bs.Close();
            }
        }

    }
    public class CASRecorder:HLogHandler
    {
        public CASRecorder(string filePath) : base(filePath)
        {
            bs.Close();
            bufSize = 10 * 1024;
            bs = new BufferedStream(new FileStream(filePath,FileMode.OpenOrCreate));
        }

        public new void InfoNewLine(string info)
        {
            if (bs != null) {
                byte[] b = Encoding.UTF8.GetBytes(info + "\n");
                bs.Write(b, 0, b.Length);
            }
        }
    }
    public class CasTypeRecorder : HLogHandler
    {
        int fileCounts=0;
        Dictionary<string, int> dict = new Dictionary<string, int> ();
        public CasTypeRecorder(string filePath) : base(filePath)
        {
        }
        public void putType(string typeKey) {
            if (!dict.ContainsKey(typeKey))
            {
                dict.Add(typeKey, 1);
            }
            else {
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
        public new void Close() {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("类别\t\t个数");
            int totalCount = 0;
            foreach (var item in dict)
            {
                sb.AppendLine(item.Key +"\t\t"+ item.Value);
                totalCount += item.Value;
            }
            sb.AppendLine("=======结果=======");
            sb.AppendLine("所有分类过的CasMod数量为："+totalCount);
            byte[] resultBytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            bs.Write(resultBytes, 0, resultBytes.Length);
            base.Close();
        }
    }
}
