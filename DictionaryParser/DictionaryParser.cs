/*
 *   Shikai Jin 
 *   sjin02@syr.edu
 *   SUID 844973756
 */
/*
 *   Build command:
 *   devenv ./DependencyAnalyzer.sln /rebuild debug
 *   
 *   Maintenance History:
 *   Ver 1.0  Nov. 14  2014 created by Shikai Jin 
 */
/*
 * 
 * Public Interface
 * //convert a dictionary type to a xml
 *  public static XElement Dict2XML(Dictionary<string,List<string>> dict,
            string itemtag="item",string key="key",string value="value",string root="root")
 * //conert a xml string to a dictionary 
 * public static Dictionary<string, List<string>> XML2Dict(string doc)
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
namespace Util
{
    /// <summary>
    /// serialize dict type
    /// </summary>
    public class DictionaryParser
    {
        /// <summary>
        /// get the list that mapped by the key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        static List<T> GetEntry<T>(Dictionary<string, List<T>> dict, string key)
        {
            List<T> entry = null;
            if (!dict.ContainsKey(key))
            {
                entry = new List<T>();
                dict[key] = entry;
            }
            else
            {
                entry = dict[key];
            }

            return entry;
        }
        /// <summary>
        /// convert the dict to xml 
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="itemtag"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        public static XElement Dict2XML(Dictionary<string,List<string>> dict,
            string itemtag="item",string key="key",string value="value",string root="root")
        {
            XElement ele=new XElement(itemtag);
            return ele;
        }
        /// <summary>
        /// convert xml strng to dict
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static Dictionary<string, List<string>> XML2Dict(string doc)
        {
            try
            {
                Dictionary<string, List<string>> ret = XML2Dict(XDocument.Parse(doc));
                return ret;
            }
            catch (Exception e)
            {
                DebugLog.Instance.Write(e);
            }
            return null;
        }
        /// <summary>
        /// convert xml as Xdocument to xml
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static Dictionary<string,List<string>> XML2Dict(XDocument doc)
        {
            
            try
            {
                Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
                foreach (var item in doc.Root.Elements())
                {
                    List<XElement> keyvalue2=item.Elements().ToList();
                    string key="";
                    List<string> value=new List<string>();
                    if (keyvalue2.Count >= 2)
                    {
                        key = keyvalue2[0].Value;
                        for (int i = 1; i < keyvalue2.Count; i++)
                        {
                            value.Add(keyvalue2[i].Value);
                        }
                        dict[key] = value;
                    }
                }
                return dict;
            }
            catch (Exception e)
            {
                DebugLog.Instance.Write(e);
            }
            return null;
        }
    }
}


//test stub
#if(III)
    class Test
    {
        static void Main(string[] args)
        {
            XDocument doc=new XDocument("root",
                new XElement("name","AAA"),
                new XElement("file","BBB"),
                new XElement("file","CCC"));
            
            var r=Util.DictionaryParser.XML2Dict(doc.ToString());
            foreach(var pair in r)
            {
                System.Console.WriteLine("key:{0} values:{1}",
                    pair.Key,String.Join(",",pair.Value));
            }
        }
    }
#endif