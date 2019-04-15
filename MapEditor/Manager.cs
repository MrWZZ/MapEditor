using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Newtonsoft.Json;

namespace MapEditor
{
    public static class Manager
    {
        //用户数据
        public static UserData userData = new UserData();
        //用户数据保存路径
        private static string configPath = @".\MapEditorUserData.json";

        //保存用户数据
        public static void SaveUserData()
        {
            string jsonString = JsonConvert.SerializeObject(userData,Formatting.Indented);
            using (StreamWriter sw = new StreamWriter(configPath, false, Encoding.UTF8))
            {
                sw.Write(jsonString);
            }
        }

        //读取用户数据
        public static void LoadUserData()
        {
            if(File.Exists(configPath))
            {
                using (StreamReader sr = new StreamReader(configPath, Encoding.UTF8))
                {
                    string file = sr.ReadToEnd();
                    userData = JsonConvert.DeserializeObject<UserData>(file);
                }
            }
        }
    }

    [System.Serializable]
    public class UserData
    {
        //游戏总地图路径
        public string mapTotalPath;
        //游戏总地图文件绝对路径
        public string mapFileName;

        //上次保存文件的路径
        public string path;
        //网格默认类型
        public int gridDefaultType = 0;
        //网格透明度
        public float gridAlpha = 0.2f;
        //网格默认高宽
        public int gridWidth = 256;
        public int gridHeight = 256;
        //图片导出的前缀
        public string frontName = "map_";
    }

}
