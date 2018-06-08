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

        public  string dirMod = "D:/modlogs/";
        HLogHandler hLogHandler = null;
        CASRecorder cRecoder = null;
        CasTypeRecorder coutsRecorder = null;
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
                ApendLineLog("===========分割线===========");
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
                return false;
            }
            int nIndex1 = filePath.LastIndexOf("\\");
            int nIndex2 = filePath.LastIndexOf(".");
            string pkName = filePath.Substring(nIndex1 + 1, nIndex2 - nIndex1 - 1);
            List<IResourceIndexEntry> tempListEntry = tempPackage.GetResourceList;
            string strEntry = "";
            string casTypes = "";
            StringBuilder casBuilder = new StringBuilder();
            List<S4PoseInfo> poseList = null;
            string dirMod = "d:/modlogs/imgs/";
            if (!Directory.Exists(dirMod))
            {
                Directory.CreateDirectory(dirMod);
            }
            Dictionary<string, string> dicStrEntry = new Dictionary<string, string>();
            IResourceIndexEntry debugEntry = null;
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
                //can't deal with:
                //GEOM 0x015A1849 ,55242443
                //RMAP  2887187436
                if (type == 0x015A1849 || type == 2887187436)
                {
                    continue;
                }
                IResource res = null;
                try
                {
                    //Error: 55242443 group: 2147483648
                    //Error:1797309683 group:0
                    if (type == 55242443 && group == 2147483648)
                    {
                        builder.AppendLine("[bug] GetResource will causeError ");
                    } else if (type == 1797309683 && group == 0)
                    {
                        builder.AppendLine("[bug] GetResource will causeError ");

                    }
                    else {
                        res = WrapperDealer.GetResource(recommendedApi, tempPackage, entry, false);

                    }
                    //res = WrapperDealer.GetResource(recommendedApi, tempPackage, entry, true);
                }
                catch (Exception e)
                {
                    builder.AppendLine("currentFile:\n" + filePath );
                    builder.AppendLine("[bug]:GetResource Error:" + type + " group:" + group);
                    builder.AppendLine(GetExMsg(e));
                }
                if (res == null)
                {
                    builder.AppendLine("");
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
                            builder.AppendLine("[debug] get result By :HasStringValueContentField:");
                        }
                        else
                        {
                            result = new StreamReader(res.Stream).ReadToEnd();
                            builder.AppendLine("[debug] get result By :StreamReader.ReadToEnd:");
                        }
                        poseList = ParseXmlTolist(result, builder);
                        if (poseList == null || poseList.Count == 0)
                        {
                            builder.AppendLine("[bug] xml :\n" + result);
                        }
                    }
                    catch (Exception e2)
                    {
                        builder.AppendLine("[error] xml 解析异常");
                        builder.AppendLine(GetExMsg(e2));
                        continue;
                    }
                } //STBL
                else if (type == 0x220557DA)
                {
                    if (strEntry.Equals(""))
                    {
                        if (this.HasStringValueContentField(res))
                        {
                            strEntry = res["Value"];
                            builder.AppendLine("[debug] get result By  value");
                        }
                        else
                        {
                            strEntry = new StreamReader(res.Stream).ReadToEnd();
                            builder.AppendLine("[debug] get result By  readToEnd");
                        }
                        int start = 0;
                        while ((start = strEntry.IndexOf("Key", start + 1)) != -1)
                        {
                            int maohaoIndex = strEntry.IndexOf(":", start);
                            int dotIndex = strEntry.IndexOf(",", start);
                            int endIndex = strEntry.IndexOf("\n", start);
                            string key = strEntry.Substring(start + 4, dotIndex - start - 4);
                            string value = strEntry.Substring(maohaoIndex + 2, endIndex - maohaoIndex - 2);
                            dicStrEntry.Add(key, value);
                            builder.AppendLine("id:" + key + "\tvalue:" + value);
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
                        casBuilder.AppendLine("文件名:" +filePath);
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
                        int index3 = casInfo.IndexOf("AgeGender");
                        int end1 = casInfo.IndexOf("\n", index1);
                        int end2 = casInfo.IndexOf("\n", index2);
                        int end3 = casInfo.IndexOf("\n", index3);
                        if (index1 >= 0 && end1 >= 0)
                        {
                                errorIndex += "BodyType:" + index1;
                                int dot1 = casInfo.IndexOf("(",index1);
                                int dot2 = casInfo.IndexOf(")", index1);

                                //casBuilder.AppendLine("类别：" + casInfo.Substring(index1 + 8, end1 - index1 - 8));
                                casBuilder.AppendLine("类别：" + casInfo.Substring(dot1+1,dot2-dot1-1));
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
                                casBuilder.AppendLine("适用年龄：" + casInfo.Substring(dot1 + 1, dot2 - dot1 - 1));

                                //casBuilder.AppendLine("适用年龄：" + casInfo.Substring(index3 + 9, end3 - index3 - 9));
                            }
                           

                        }
                        catch(Exception casEx )
                        {
                            casBuilder.AppendLine("[bug] casTypeInfo is not found at " + filePath);
                            casBuilder.AppendLine("indexs :"+errorIndex);
                            casBuilder.AppendLine("CASInfo:" + casInfo);
                            casBuilder.AppendLine(GetExMsg(casEx));
                        }
                    }
                    casTypes = casBuilder.ToString();
                }
            }
            if (!"".Equals(casTypes))
            {
                ApendLineRecordCas(casTypes);
                //builder.AppendLine("casTypes:" + casTypes);
            }
            //只导出pose
            
            if (poseList != null && poseList.Count > 0)
            {
                foreach(S4PoseInfo s4 in poseList) {
                }
                for (int i = 0; i < tempListEntry.Count; i++)
                {
                    //图片img资源
                    IResourceIndexEntry entry = tempListEntry[i];
                    uint type = entry.ResourceType;
                    int reqApi = entry.RequestedApiVersion;
                    uint group = entry.ResourceGroup;
                    int recommendedApi = entry.RecommendedApiVersion;
                    IResource res = null;
                    try
                    {
                        res = WrapperDealer.GetResource(recommendedApi, tempPackage, entry, false);
                        //res = WrapperDealer.GetResource(recommendedApi, tempPackage, entry, true);
                    }
                    catch (Exception e)
                    {
                        builder.Append("currentFile:" + filePath + "\n");
                        builder.AppendLine("[error]:GetResource Error:" + type + " group:" + group);
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
                            //conn.GetContextMenuItems
                            string finalPath = dirMod + "Temp_Icon/";
                            if (!Directory.Exists(finalPath))
                            {
                                Directory.CreateDirectory(finalPath);
                            }
                            string detailPath =dirMod+ "detailImg/";
                           
                            if (!Directory.Exists(detailPath))
                            {
                                Directory.CreateDirectory(detailPath);
                            }
                            string otherDir = dirMod + "otherIcon/";
                            if (!Directory.Exists(otherDir))
                            {
                                Directory.CreateDirectory(otherDir);
                            }
                            path = finalPath + pkName + "@_mainIcon_" + str16 + ".jpg";
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
                                        builder.AppendLine("[bug]not found Key " + dexName + "\t@" + pkName);
                                        value = dexName;
                                    }
                                    string newName = string.Format("{0}@{1}({2}).jpg", pkName, value, info.PoseCmd.Replace(":", "%"));
                                    path = detailPath + pkName + "@" + str16 + ".jpg";
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
                                builder.AppendLine(String.Format("[bug] img instance16:{0}", str16));
                            }
                            builder.AppendLine("[debug]:use imgPath " + path);

                            if (!File.Exists(path))
                            {
                                //fs = File.Create(path);
                            }
                            else
                            {
                                builder.AppendLine("[debug]:img " + path + " is exists");
                                continue;
                            }

                            try
                            {
                                fs = File.Open(path, FileMode.Create);
                                Stream resStream = data;
                                if (data == null || data.Length == 0)
                                {
                                    return true;
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
                                builder.Append("[error]:" + we.Message);
                                builder.AppendLine("[error]:" + we.StackTrace);
                                builder.Append("\n");
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
                            builder.AppendLine(String.Format("Error at{0},\nCaused By:{1}", filePath, s));
                        }
                    }

                }
            }
            ApendLineLog(builder.ToString());
            return true;
        }

        public List<S4PoseInfo> ParseXmlTolist(string xmlStr, StringBuilder builder2)

        {
            StringBuilder builder = new StringBuilder();
            List<S4PoseInfo> poses = new List<S4PoseInfo>();
            string mainIcon = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlStr);
                XmlNodeList  rList=doc.LastChild.ChildNodes;
                foreach( XmlNode node in rList){
                    string rName=node.Attributes[0].Value;
                    if ("icon".Equals(rName)) {
                        mainIcon = node.InnerText.Substring(node.InnerText.LastIndexOf(":")+1);
                        builder.AppendLine("MainIcon:"+mainIcon);
                    }
                }
                XmlNodeList listNode = doc.GetElementsByTagName("L");
                XmlNode ln = listNode[0];
                foreach (XmlNode courseNode in ln.ChildNodes)
                {
                    XmlNodeList finalValues = courseNode.ChildNodes;
                    S4PoseInfo poseInfo = new S4PoseInfo();
                    poseInfo.mainIcon = mainIcon;
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
                                builder.AppendLine("pose:" + poseInfo.PoseIcon);
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
                        builder.AppendLine(asName + asValue);
                    }
                    poses.Add(poseInfo);
                    builder.Append("\n");
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
            builder.AppendLine("[error] detail: " + tempEx.StackTrace);
            while (tempEx.InnerException != null)
            {
                tempEx = tempEx.InnerException;
                builder.AppendLine("[error] causedBy: " + tempEx.Message);
                builder.AppendLine("[error] detail: " + tempEx.StackTrace);
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
            int counts = 0;
            if (!dict.ContainsKey(typeKey))
            {
                counts = 0;
                dict.Add(typeKey, counts);
                //dict.
            }
            else {
                counts = dict[typeKey];
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
            foreach (var item in dict)
            {
                sb.AppendLine(item.Key + "\t\t" + item.Value);
            }
            byte[] resultBytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            bs.Write(resultBytes, 0, resultBytes.Length);
            base.Close();
        }
    }
}
