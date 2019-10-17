using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S4PIDemoFE.Zjy
{
    public class DuplicatItem: IComparable<DuplicatItem>
    {
        public string fileName;
        public string filepath;
        public string modifytime;
        public class Comparator : IComparer<DuplicatItem>
        {
            public int Compare(DuplicatItem x, DuplicatItem y)
            {
                char[] m = x.fileName.ToLower().ToCharArray();
                char[] m2 = y.fileName.ToLower().ToCharArray();
                int len1 = m.Length;
                int len2 = m2.Length;
                if (len1 > len2)
                {
                    len1 = len2;
                }

                for (int i = 0; i < len1; i++)
                {
                    char c1 = m[i];
                    char c2 = m[i];
                    if (c1 == c2)
                    {
                        continue;
                    }
                    else
                    {
                        return c1 - c2;
                    }
                }
                return 0;
            }
        }

        public int CompareTo(DuplicatItem obj)
        {
            char[] m = obj.fileName.ToLower().ToCharArray();
            char[] m2 = this.fileName.ToLower().ToCharArray();
            int len1 = m.Length;
            int len2= m2.Length;
            if (len1 > len2) {
                len1 = len2;
            }

            for (int i = 0; i < len1; i++) {
                char c1 = m[i];
                char c2 = m[i];
                if (c1 == c2)
                {
                    continue;
                }
                else {
                    return c1 - c2;
                }
            }
            //char[] t1 = obj.modifytime.ToLower().ToCharArray();
            //char[] t2 = this.modifytime.ToLower().ToCharArray();
            //int tlen1 = t1.Length;
            //int tlen12 = t2.Length;
            //if (tlen1 > tlen12)
            //{
            //    tlen1 = tlen12;
            //}

            //for (int i = 0; i < tlen1; i++)
            //{
            //    char ct1 = m[i];
            //    char ct2 = m[i];
            //    if (ct1 == ct2)
            //    {
            //        continue;
            //    }
            //    else
            //    {
            //        return ct1 - ct2;
            //    }
            //}
            //Convert.ToDateTime(string, IFormatProvider)
            DateTime dt1 = Convert.ToDateTime(obj.modifytime);
            DateTime dt2 = Convert.ToDateTime(this.modifytime);
            int compNum = DateTime.Compare(dt1, dt2);
            return compNum;
        }
    }
    class t : IFormatProvider
    {
        public object GetFormat(Type formatType)
        {
            throw new NotImplementedException();
        }
    }
}
