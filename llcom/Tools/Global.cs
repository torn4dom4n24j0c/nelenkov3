using LibUsbDotNet;
using LibUsbDotNet.Main;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace llcom.Tools
{
    class Global
    {
        //api接口文档网址
        public static string apiDocumentUrl = "https://github.com/chenxuuu/llcom/blob/master/LuaApi.md";
        //主窗口是否被关闭？
        public static bool isMainWindowsClosed = false;
        //给全局使用的设置参数项
        public static Model.Settings setting;
        public static Model.Uart uart = new Model.Uart();

        //软件根目录
        public static readonly string AppPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName);
        //配置文件路径
        public static readonly string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\llcom\";

        /// <summary>
        /// 是否为应用商店版本？
        /// </summary>
        /// <returns></returns>
        public static bool IsMSIX()
        {
            return AppPath.ToUpper().Contains(@"C:\PROGRAM FILES\WINDOWSAPPS");
        }

        /// <summary>
        /// 软件打开后，所有东西的初始化流程
        /// </summary>
        public static void Initial()
        {
            //C:\Users\chenx\AppData\Local\Temp\7zO05433053\user_script_run
            if(AppPath.ToUpper().IndexOf(@"C:\USERS\") == 0 &&
                AppPath.ToUpper().Contains(@"\APPDATA\LOCAL\TEMP\"))
            {
                System.Windows.MessageBox.Show("请勿在压缩包内直接打开本软件。");
                Environment.Exit(1);
            }
            
            try
            {
                if (!Directory.Exists("core_script"))
                {
                    Directory.CreateDirectory("core_script");
                }
                CreateFile("DefaultFiles/core_script/head.lua", "core_script/head.lua", false);
                CreateFile("DefaultFiles/core_script/JSON.lua", "core_script/JSON.lua", false);
                CreateFile("DefaultFiles/core_script/log.lua", "core_script/log.lua", false);
                CreateFile("DefaultFiles/core_script/once.lua", "core_script/once.lua", false);
                CreateFile("DefaultFiles/core_script/strings.lua", "core_script/strings.lua", false);
                CreateFile("DefaultFiles/core_script/sys.lua", "core_script/sys.lua", false);

                if (!Directory.Exists("logs"))
                    Directory.CreateDirectory("logs");
                if (!Directory.Exists("user_script_run"))
                {
                    Directory.CreateDirectory("user_script_run");
                    CreateFile("DefaultFiles/user_script_run/AT控制TCP连接-快发模式.lua", "user_script_run/AT控制TCP连接-快发模式.lua");
                    CreateFile("DefaultFiles/user_script_run/AT控制TCP连接-慢发模式.lua", "user_script_run/AT控制TCP连接-慢发模式.lua");
                    CreateFile("DefaultFiles/user_script_run/example.lua", "user_script_run/example.lua");
                    CreateFile("DefaultFiles/user_script_run/循环发送快捷发送区数据.lua", "user_script_run/循环发送快捷发送区数据.lua");
                }
                if (!Directory.Exists("user_script_run/requires"))
                    Directory.CreateDirectory("user_script_run/requires");
                if (!Directory.Exists("user_script_run/logs"))
                    Directory.CreateDirectory("user_script_run/logs");

                if (!Directory.Exists("user_script_send_convert"))
                {
                    Directory.CreateDirectory("user_script_send_convert");
                    CreateFile("DefaultFiles/user_script_send_convert/16进制数据.lua", "user_script_send_convert/16进制数据.lua");
                    CreateFile("DefaultFiles/user_script_send_convert/GPS NMEA.lua", "user_script_send_convert/GPS NMEA.lua");
                    CreateFile("DefaultFiles/user_script_send_convert/加上换行回车.lua", "user_script_send_convert/加上换行回车.lua");
                    CreateFile("DefaultFiles/user_script_send_convert/解析换行回车的转义字符.lua", "user_script_send_convert/解析换行回车的转义字符.lua");
                    CreateFile("DefaultFiles/user_script_send_convert/默认.lua", "user_script_send_convert/默认.lua");
                }

                CreateFile("DefaultFiles/LICENSE", "LICENSE", false);
                CreateFile("DefaultFiles/反馈网址.txt", "反馈网址.txt", false);
            }
            catch(Exception e)
            {
                System.Windows.MessageBox.Show("生成文件结构失败，请确保本软件处于有读写权限的目录下再打开。\r\n错误信息："+e.Message);
                Environment.Exit(1);
            }

            //配置文件
            if(File.Exists("settings.json"))
            {
                setting = JsonConvert.DeserializeObject<Model.Settings>(File.ReadAllText("settings.json"));
                setting.SentCount = 0;
                setting.ReceivedCount = 0;
            }
            else
            {
                //导入之前的配置文件
                if (Properties.Settings.Default.UpgradeRequired)
                {
                    Properties.Settings.Default.Upgrade();
                    //Properties.Settings.Default.UpgradeRequired = false;
                    //Properties.Settings.Default.Save();
                }
                setting = new Model.Settings();
                if(Properties.Settings.Default.quickData != "done" &&
                    Properties.Settings.Default.dataToSend != 
                    "uart dataplVIzj85gvLDrDqtVxftzTb78")//不是第一次用
                {
                    setting.dataToSend = Properties.Settings.Default.dataToSend;
                    setting.baudRate = Properties.Settings.Default.BaudRate;
                    setting.autoReconnect = Properties.Settings.Default.autoReconnect;
                    setting.autoSaveLog = Properties.Settings.Default.autoSaveLog;
                    setting.showHex = Properties.Settings.Default.showHex;
                    setting.parity = Properties.Settings.Default.parity;
                    setting.timeout = Properties.Settings.Default.timeout;
                    setting.dataBits = Properties.Settings.Default.dataBits;
                    setting.stopBit = Properties.Settings.Default.stopBit;
                    setting.sendScript = Properties.Settings.Default.sendScript;
                    setting.runScript = Properties.Settings.Default.runScript;
                    setting.topmost = Properties.Settings.Default.topmost;
                    setting.quickData = Properties.Settings.Default.quickData;
                    setting.bitDelay = Properties.Settings.Default.bitDelay;
                    setting.autoUpdate = Properties.Settings.Default.autoUpdate;
                    setting.maxLength = Properties.Settings.Default.maxLength;
                    Properties.Settings.Default.quickData = "done";
                }
            }

            setting.UpdateQuickSend();



            uart.serial.BaudRate = setting.baudRate;
            uart.serial.Parity = (Parity)setting.parity;
            uart.serial.DataBits = setting.dataBits;
            uart.serial.StopBits = (StopBits)setting.stopBit;
            uart.UartDataRecived += Uart_UartDataRecived;
            uart.UartDataSent += Uart_UartDataSent;
            LuaEnv.LuaRunEnv.init();
        }

        /// <summary>
        /// 已发送记录到日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Uart_UartDataSent(object sender, EventArgs e)
        {
            Logger.AddUartLog($"[INFO]==>{Byte2String((byte[])sender)}");
            Logger.AddUartLog($"[DEBUG][HEX]\"{Byte2Hex((byte[])sender, " ")}\"");
        }

        /// <summary>
        /// 收到的数据记录到日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Uart_UartDataRecived(object sender, EventArgs e)
        {
            Logger.AddUartLog($"[INFO]<=={Byte2String((byte[])sender)}");
            Logger.AddUartLog($"[DEBUG][HEX]\"{Byte2Hex((byte[])sender, " ")}\"");
        }

        public static Encoding GetEncoding() => Encoding.GetEncoding(setting.encoding);

        /// <summary>
        /// 字符串转hex值
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="space">间隔符号</param>
        /// <returns>结果</returns>
        public static string String2Hex(string str, string space)
        {
             return BitConverter.ToString(GetEncoding().GetBytes(str)).Replace("-", space);
        }


        /// <summary>
        /// hex值转字符串
        /// </summary>
        /// <param name="mHex">hex值</param>
        /// <returns>原始字符串</returns>
        public static string Hex2String(string mHex)
        {
            mHex = Regex.Replace(mHex, "[^0-9A-Fa-f]", "");
            if (mHex.Length % 2 != 0)
                mHex = mHex.Remove(mHex.Length - 1, 1);
            if (mHex.Length <= 0) return "";
            byte[] vBytes = new byte[mHex.Length / 2];
            for (int i = 0; i < mHex.Length; i += 2)
                if (!byte.TryParse(mHex.Substring(i, 2), NumberStyles.HexNumber, null, out vBytes[i / 2]))
                    vBytes[i / 2] = 0;
            return GetEncoding().GetString(vBytes);
        }


        /// <summary>
        /// byte转string
        /// </summary>
        /// <param name="mHex"></param>
        /// <returns></returns>
        public static string Byte2String(byte[] vBytes)
        {
            var br = from e in vBytes
                     where e != 0
                     select e;
            return GetEncoding().GetString(br.ToArray());
        }

        /// <summary>
        /// hex转byte
        /// </summary>
        /// <param name="mHex">hex值</param>
        /// <returns>原始字符串</returns>
        public static byte[] Hex2Byte(string mHex)
        {
            mHex = Regex.Replace(mHex, "[^0-9A-Fa-f]", "");
            if (mHex.Length % 2 != 0)
                mHex = mHex.Remove(mHex.Length - 1, 1);
            if (mHex.Length <= 0) return new byte[0];
            byte[] vBytes = new byte[mHex.Length / 2];
            for (int i = 0; i < mHex.Length; i += 2)
                if (!byte.TryParse(mHex.Substring(i, 2), NumberStyles.HexNumber, null, out vBytes[i / 2]))
                    vBytes[i / 2] = 0;
            return vBytes;
        }


        public static string Byte2Hex(byte[] d, string s = "")
        {
            return BitConverter.ToString(d).Replace("-", s);
        }


        /// <summary>
        /// 导入SSCOM配置文件数据
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<Model.ToSendData> ImportFromSSCOM(string path)
        {
            var lines = File.ReadAllLines(path, Encoding.GetEncoding("GB2312"));
            var r = new List<Model.ToSendData>();
            Regex title = new Regex(@"N1\d\d=\d*,");
            for (int i = 0; i < lines.Length; i++)
            {
                try
                {
                    var temp = new Model.ToSendData();
                    //Console.WriteLine(lines[i]);
                    if (title.IsMatch(lines[i]))//匹配上了
                    {
                        var strs = lines[i].Split(",".ToCharArray()[0]);
                        temp.commit = strs[1].Replace(((char)2).ToString(), ",");
                        if (string.IsNullOrWhiteSpace(temp.commit))
                            temp.commit = "发送";
                        //Console.WriteLine(temp.commit);

                        int dot = lines[i + 1].IndexOf(",");
                        temp.hex = lines[i + 1].Substring(dot - 1, 1) == "H";
                        //Console.WriteLine(strs[0].Substring(strs[0].Length - 1));

                        string text = lines[i + 1].Substring(dot + 1);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            temp.text = text.Replace(((char)2).ToString(), ",");
                            r.Add(temp);
                        }
                    }
                }
                catch
                {
                    //先不处理
                }
            }
            return r;
        }

        /// <summary>
        /// 读取软件资源文件内容
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>内容字节数组</returns>
        public static byte[] GetAssetsFileContent(string path)
        {
            Uri uri = new Uri(path, UriKind.Relative);
            var source = System.Windows.Application.GetResourceStream(uri).Stream;
            byte[] f = new byte[source.Length];
            source.Read(f, 0, (int)source.Length);
            return f;
        }

        /// <summary>
        /// 取出文件
        /// </summary>
        /// <param name="insidePath">软件内部的路径</param>
        /// <param name="outPath">需要释放到的路径</param>
        /// <param name="d">是否覆盖</param>
        public static void CreateFile(string insidePath, string outPath, bool d = true)
        {
            if(!File.Exists(outPath) || d)
                File.WriteAllBytes(outPath, GetAssetsFileContent(insidePath));
        }

        /// <summary>
        /// 更换语言文件
        /// </summary>
        /// <param name="languagefileName"></param>
        public static void LoadLanguageFile(string languagefileName)
        {
            try
            {
                System.Windows.Application.Current.Resources.MergedDictionaries[0] = new System.Windows.ResourceDictionary()
                {
                    Source = new Uri($"pack://application:,,,/languages/{languagefileName}.xaml", UriKind.RelativeOrAbsolute)
                };
            }
            catch
            {
                System.Windows.Application.Current.Resources.MergedDictionaries[0] = new System.Windows.ResourceDictionary()
                {
                    Source = new Uri("pack://application:,,,/languages/en-US.xaml", UriKind.RelativeOrAbsolute)
                };
            }

        }

    }
}
