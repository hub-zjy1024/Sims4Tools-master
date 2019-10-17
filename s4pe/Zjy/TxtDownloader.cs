using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace S4PIDemoFE.Zjy
{
   public class TxtDownloader

    {
        private int delay = 300;
       public class Chapter {
            public  string name;
            public  string url;
        }
        class Adata
        {
            public string href;
            public string text;
        }
        public static string mainUrl="https://www.booktxt.net";
        string getID = "";
        /// <summary>
        /// 
        ///   //string novelId = "/2_2082/";
        ///      string startCap = "第7400章";
        /// </summary>
        /// <param name="novId"></param>
        /// <param name="startCapter"></param>
        public void downLoadBy(string novId, string startCapter) {
            getContent(novId, startCapter);
        }
        public void downLoadBy(string novId)
        {
            getContent(novId, null);
        }
        public void getContent(string novId,string startCName) {
          
            string contentPath = "/" + novId + "/";
            string finalContentUri = mainUrl + contentPath;
            List<Chapter> chapterLists = new List<Chapter>();
            string body = null;
            try
            {
                //                :authority: www.booktxt.net
                //:method: GET
                // : path:/ 2_2082 /
                //:scheme: https
                // accept:text / html,application / xhtml + xml,application / xml; q = 0.9,image / webp,image / apng,*/*;q=0.8
                //accept-encoding:gzip, deflate, br
                //accept-language:zh-CN,zh;q=0.9
                //cache-control:max-age=0
                //cookie:Hm_lvt_3a0ea2f51f8d9b11a51868e48314bf4d=1553406432; Hm_lpvt_3a0ea2f51f8d9b11a51868e48314bf4d=1553415347
                //if-modified-since:Sat, 23 Mar 2019 12:49:38 GMT
                //if-none-match:W/"c0f1f1df76e1d41:0"
                //referer:https://www.booktxt.net/2_2082/4870742.html
                //upgrade-insecure-requests:1
                //user-agent:Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.26 Safari/537.36 Core/1.63.6788.400 QQBrowser/10.3.2714.400
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(finalContentUri);
                //httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";
                httpWebRequest.Timeout = 30 * 1000;
                httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.26 Safari/537.36 Core/1.63.6788.400 QQBrowser/10.3.2714.400";

                if (body != null) {
                     byte[] btBodys = Encoding.UTF8.GetBytes(body);
                    httpWebRequest.ContentLength = btBodys.Length;
                    httpWebRequest.GetRequestStream().Write(btBodys, 0, btBodys.Length);
                }
              
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                string charset = "gbk";
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(),Encoding.GetEncoding(charset));
                string responseContent = streamReader.ReadToEnd();
                streamReader.Close();
                string tag = "id=\"list\"";
                string endTag = "</div>";

                int index = responseContent.IndexOf(tag);
                if (index > 0)
                {
                    int endIndx = responseContent.IndexOf(endTag, index);
                    if (endIndx > 0)
                    {
                        string mContent = responseContent.Substring(index + tag.Length, endIndx - (index + tag.Length));
                        string[] links = mContent.Split(new string[] { "<dd>" }, StringSplitOptions.RemoveEmptyEntries);
                        Regex regLink = new Regex("<a.+</a>");
                        foreach (string tems in links)
                        {
                            Match m = regLink.Match(tems);

                            if (m.Success)
                            {
                                Console.WriteLine("getA=" + m.Value);
                                Adata mData = getAtagValue(m.Value);
                                Chapter chapter = new Chapter();
                                chapter.name = mData.text;
                                chapter.url = mData.href;
                                chapterLists.Add(chapter);
                            }
                        }
                    }
                }
                else {
                    string msg = string.Format("找不到 起始标签 '{0}',html=\n{1}" ,tag, responseContent); 
                    Console.WriteLine(msg);
                }

                string line = "ADDR=1234;NAME=ZHANG;PHONE=6789";

                //Regex reg = new Regex("NAME=(.+);");
                ////例如我想提取记录中的NAME值
                //Match match = reg.Match(line);
                //string value = match.Groups[1].Value;
                //Console.WriteLine("value的值为：{0}", value);

            
             
                try {
                   List< Chapter> chapters = chapterLists;
                    string chapterName = startCName;
                    bool isFound = false;
                    if (chapterName == null || "".Equals(chapterName))
                    {
                        isFound = true;
                    }
                    Encoding utf8;
                    Encoding to;
                    utf8 = Encoding.GetEncoding("UTF-8");
                    to = Encoding.GetEncoding("GBK");
                    FileStream fs = File.Open("d:/test.txt", FileMode.OpenOrCreate);
                    StreamWriter writer = new StreamWriter(fs, Encoding.UTF8);
                    for (int i = 0; i < chapters.Count; i++)
                    {
                        Chapter nowCap = chapters[i];
                        string name = nowCap.name;
                        string dUrl = nowCap.url;
                        Console.WriteLine(string.Format("nowCap={0}", LanChange(name, utf8, to)));
                        if (name.Equals(chapterName))
                        {
                            isFound = true;
                        }
                        string mData = "获取失败";
                        if (isFound)
                        {
                            try
                            {
                                string detailUrl = finalContentUri + dUrl;
                                mData = downloadContent(detailUrl, nowCap.name);
                                writer.WriteLine(name);
                                writer.WriteLine(mData);
                            }
                            catch (Exception e)
                            {

                                string msg = e.Message + "\n" + e.StackTrace;
                                Console.WriteLine(name + " read error=" + msg);
                            }
                            Console.WriteLine(string.Format("getDataLength={0}", mData.Length));
                            Thread.Sleep(delay);
                        }

                    }
                    writer.Close();
                  
                }
                catch (Exception e) {
                    string msg = e.Message+"\n"+ e.StackTrace;
                    Console.WriteLine("下载章节出错,msg=" + msg);
                }
                
                string msg2 = finalContentUri +" finished";
                Console.WriteLine(msg2);
                return;
            }
            catch (Exception ex)
            {
                string msg = ex.Message + "\n" + ex.StackTrace;
                Console.WriteLine("获取目录异常,msg="+msg);
                //return null;
            }
        }
        public void downLoadFrom(string chapterName,List<Chapter> chapters,string finalContentUri) {
           

            //第7400章
        }
        string LanChange(string str, Encoding from, Encoding to)
        {
          
            byte[] gb = from.GetBytes(str);
            gb = Encoding.Convert(from, to, gb);
            return to.GetString(gb);
        }
        string downloadContent(string finalContentUri,string href) {

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(finalContentUri);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
            httpWebRequest.Timeout = 30 * 1000;
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.26 Safari/537.36 Core/1.63.6788.400 QQBrowser/10.3.2714.400";
            //byte[] btBodys = Encoding.UTF8.GetBytes(body);
            //httpWebRequest.ContentLength = btBodys.Length;
            //httpWebRequest.GetRequestStream().Write(btBodys, 0, btBodys.Length);
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            string charset = "gbk";
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding(charset));
            string responseContent = streamReader.ReadToEnd();
            streamReader.Close();
            string tag = "id=\"content\"";
            string ednTag = "</div>";
            string content = getStringFrom(responseContent, tag, ednTag);
            if ("".Equals(content)) {
                string msg = string.Format("明细页找不到 起始标签 '{0}',html=\n{1}", tag, responseContent);
                Console.WriteLine(msg);
            }
            string mData = content.Replace("<br />", "\r\n").Replace("&nbsp;", "  ");
            return mData;
        }

        Adata getAtagValue(string astr) {
            Adata adata = new Adata();

            string tag = "\"";
            string endTag = "\"";
            string href = getStringFrom(astr, tag, endTag);
            string tag2 = ">";
            string endTag2 = "<";
            string text = getStringFrom(astr,tag2, endTag2);
            adata.text = text;
            adata.href = href;
            return adata;
        }
      string   getStringFrom(string data,string startStr,string endStr) {
            string result = "";
              string tag = startStr;
            string endTag = endStr;
            int index = data.IndexOf(tag);
            if (index > 0) {
                int index2 = data.IndexOf(endTag,index+1);
                if (index2 > 0) {
                    int start = index + tag.Length;
                    int end = index2;
                    result = data.Substring(start, end - start);
                }
            }
            return result;

        }

    }
}
