using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace llcom.Tools
{
    class Logger
    {
        //显示日志数据的回调函数
        public static event EventHandler<DataShowPara> DataShowEvent;
        //清空显示的回调函数
        public static event EventHandler DataClearEvent;
        //清空日志显示
        public static void ClearData()
        {
            DataClearEvent?.Invoke(null,null);
        }
        //显示日志数据
        public static void ShowData(byte[] data, bool send)
        {
            DataShowEvent?.Invoke(null, new DataShowPara
            {
                data = data,
                send = send
            });
        }


        private static string uartLogFile = "";
        private static string luaLogFile = "";

        /// <summary>
        /// 初始化串口日志文件
        /// </summary>
        public static void InitUartLog()
        {
            uartLogFile = Tools.Global.ProfilePath + "logs/" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".log";
            AddUartLog("[INFO]Logs by LLCOM. https://github.com/chenxuuu/llcom");
        }

        /// <summary>
        /// 写入一条串口日志
        /// </summary>
        /// <param name="l"></param>
        public static void AddUartLog(string l)
        {
            if (uartLogFile == "")
                InitUartLog();
            try
            {
                File.AppendAllText(uartLogFile, DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss:ffff]") + l + "\r\n");
            }
            catch { }
        }


        /// <summary>
        /// 初始化lua日志文件
        /// </summary>
        public static void InitLuaLog()
        {
            luaLogFile = Tools.Global.ProfilePath + "user_script_run/logs/" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".log";
        }

        /// <summary>
        /// 写入一条lua日志
        /// </summary>
        /// <param name="l"></param>
        public static void AddLuaLog(string l)
        {
            if (luaLogFile == "")
                InitLuaLog();
            try
            {
                File.AppendAllText(luaLogFile, DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss:ffff]") + l + "\r\n");
            }
            catch { }
        }
    }

    /// <summary>
    /// 显示到日志显示页面的类
    /// </summary>
    class DataShowPara
    {
        public byte[] data;
        public bool send;
    }
}
