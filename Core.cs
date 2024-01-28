using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Xml.Linq;
using LitJson;

namespace Xm_Core
{
    public class Download
    {
        public static string HttpGet(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.ContentType = "application/json";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
        public List<string> GetversionsList(out List<string> backurl)
        {
            List<string> versions = new List<string>();
            List<string> url = new List<string>();
            var json = HttpGet("https://launchermeta.mojang.com/mc/game/version_manifest.json");
            JsonData data = JsonMapper.ToObject(json);
            for (int i = 0; i < data["versions"].Count; i++)
            {
                versions.Add(data["versions"][i]["id"].ToString());
            }
            for (int i = 0; i < data["versions"].Count; i++)
            {
                url.Add(data["versions"][i]["url"].ToString());
            }
            backurl = url;
            return versions;
        }
        public void DownloadCore(string version, string MinecraftFolder)
        {
            List<string> versions = new List<string>();
            List<string> urls = new List<string>();
            var json = HttpGet("https://launchermeta.mojang.com/mc/game/version_manifest.json");
            JsonData data = JsonMapper.ToObject(json);
            for (int i = 0; i < data["versions"].Count; i++)
            {
                if (data["versions"][i]["url"].ToString().Contains(version))
                {
                    if (!Directory.Exists($@"{MinecraftFolder}\versions\{version}"))
                        Directory.CreateDirectory($@"{MinecraftFolder}\versions\{version}");
                    Downloader(data["versions"][i]["url"].ToString(), $@"{MinecraftFolder}\versions\{version}\{version}.json");
                    JsonData Jar = JsonMapper.ToObject(File.ReadAllText($@"{MinecraftFolder}\versions\{version}\{version}.json"));
                    Downloader(Jar["downloads"]["client"]["url"].ToString(), $@"{MinecraftFolder}\versions\{version}\{version}.jar");
                    if (!Directory.Exists($@"{MinecraftFolder}\libraries\"))
                        Directory.CreateDirectory($@"{MinecraftFolder}\libraries\");
                    for (int a = 0; a < Jar["libraries"].Count; a++)
                    {
                        try
                        {
                            var path = Jar["libraries"][a]["downloads"]["artifact"]["path"].ToString();
                            if (!Directory.Exists($@"{MinecraftFolder}\libraries\{path}\..\"))
                                Directory.CreateDirectory($@"{MinecraftFolder}\libraries\{path}\..\");
                            Downloader(Jar["libraries"][a]["downloads"]["artifact"]["url"].ToString(), $@"{MinecraftFolder}\libraries\{path}");
                            //尝试用第一种方案下载文件
                        }
                        catch
                        {
                            try
                            {
                                var path = Jar["libraries"][a]["downloads"]["classifiers"]["natives-windows-32"]["path"].ToString();
                                if (!Directory.Exists($@"{MinecraftFolder}\libraries\{path}\..\"))
                                    Directory.CreateDirectory($@"{MinecraftFolder}\libraries\{path}\..\");
                                Downloader(Jar["libraries"][a]["downloads"]["classifiers"]["natives-windows-32"]["url"].ToString(), $@"{MinecraftFolder}\libraries\{path}");
                                //尝试用第二种方案下载第一种下不到文件
                            }
                            catch
                            {
                                try
                                {
                                    var path = Jar["libraries"][a]["downloads"]["classifiers"]["natives-windows"]["path"].ToString();
                                    if (!Directory.Exists($@"{MinecraftFolder}\libraries\{path}\..\"))
                                        Directory.CreateDirectory($@"{MinecraftFolder}\libraries\{path}\..\");
                                    Downloader(Jar["libraries"][a]["downloads"]["classifiers"]["natives-windows"]["url"].ToString(), $@"{MinecraftFolder}\libraries\{path}");
                                    //尝试用第三种方案下载第二种下不到文件
                                }
                                catch { /* 真的会谢，那点文件不下就不下罢，反正不会影响游戏正常启动 */ }
                            }
                        }
                    }
                }
            }
        }
        public void Downloader(string url, string path)
        {
            using (var web = new WebClient())
            {
                try
                {
                    web.DownloadFile(url, path);
                }
                catch { }
            }
        }
        public List<string> DownloadAssets(string version, string MinecraftFolder)
        {
            List<string> list = new List<string>();
            JsonData Jar = JsonMapper.ToObject(File.ReadAllText($@"{MinecraftFolder}\versions\{version}\{version}.json"));
            if (!Directory.Exists($@"{MinecraftFolder}\assets\indexes\"))
                Directory.CreateDirectory($@"{MinecraftFolder}\assets\indexes\");
            //下载Assets文件夹的文件
            Downloader(Jar["assetIndex"]["url"].ToString(), $@"{MinecraftFolder}\assets\indexes\assets.json");
            JsonData assets = JsonMapper.ToObject(File.ReadAllText($@"{MinecraftFolder}\assets\indexes\assets.json"));
            JsonData listdata = assets["objects"];
            foreach (string key in listdata.Keys)
            {
                var hash = assets["objects"][key]["hash"].ToString();
                list.Add(hash);
                if (!Directory.Exists($@"{MinecraftFolder}\assets\objects\{hash.Substring(0, 2)}"))
                    Directory.CreateDirectory($@"{MinecraftFolder}\assets\objects\{hash.Substring(0, 2)}");
                Downloader($"https://resources.download.minecraft.net/{hash.Substring(0, 2)}/{hash}", $@"{MinecraftFolder}\assets\objects\{hash.Substring(0, 2)}\{hash}");
            }
            return list;
        }
    }
    public class Launch
    {
        public string launchgame(string java, string mcfolder, string version, string username)
        {
            var Token = "*****************************";//Token
            else if (!Directory.Exists(mcfolder) || !File.Exists(java))
              return "Java或者Minecraft文件夹不存在，请检查目录";
            else if (username == null || version == null)
            {

                return "用户名是空的，或者版本是空的";
            }
            else
            {
                //所有都是齐全的，开始启动游戏
                var json = $@"{mcfolder}\versions\{version}\{version}.json";
                var jar = $@"{mcfolder}\versions\{version}\{version}.jar";
                StringBuilder Bootup = new StringBuilder();
                var cplist = new List<string>();
                var navilist = new List<string>();
                Bootup.append($"\"{java}\" -XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump -Dos.name=Windows10 -Dos.version=10.0 -Xss1M ");
                if (!Directory.Exists($@"{mcfolder}\versions\{version}\natives\"))
                    Directory.CreateDirectory($@"{mcfolder}\versions\{version}\natives\");
                Bootup.append("-Djava.library.path=" + $@"{mcfolder}\versions\{version}\natives\ ");
                Bootup.append("-Dminecraft.launcher.brand=Launcher -Dminecraft.launcher.version=1.0 ");
                Bootup.append("-cp ");
                //这里给cplist赋值
                cplist.Add(jar);
                JsonData cpdata = JsonMapper.ToObject(File.ReadAllText(json));
                for (int i = 0; i < cpdata["libraries"].Count; i++)
                {
                    try
                    {
                        cplist.Add(mcfolder + @"\libraries\" + cpdata["libraries"][i]["downloads"]["artifact"]["path"].ToString().Replace("/", @"\")/*没用，就为了个好看，统一下左斜杠和右斜杠*/);
                        navilist.Add(mcfolder + @"\libraries\" + cpdata["libraries"][i]["downloads"]["classifiers"]["natives-windows"]["path"].ToString());
                    }
                    catch { }
                }
                //遍历
                foreach (var cp in cplist)
                {
                    Bootup.append(cp + ";");
                }
                //解压必要的DLL
                foreach(var nav in navilist)
                {
                    Process.Start("cmd.exe", "/c start cmd.exe /c 7z.exe x " + nav + " -y -o" + $@"{mcfolder}\versions\{version}\natives");
                }
                Bootup.append("-Xmx2G -XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1NewSizePercent=20 -XX:G1ReservePercent=20 -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=32M net.minecraft.client.main.Main ");
                Bootup.append($@"--username {username} --version {version} --gameDir {mcfolder} --accessToken {Token} --assetsDir {mcfolder}\assets --userType mojang --versionType Core");
                return Bootup.ToString();
            }
        }
    }
}
