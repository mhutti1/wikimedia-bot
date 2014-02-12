//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

// Created by Petr Bena benapetr@gmail.com

using System.Text;
using System;
using System.Collections.Generic;
using System.IO;

namespace wmib
{
    /// <summary>
    /// Configuration
    /// </summary>
    public partial class Configuration
    {
        public class Paths
        {
            /// <summary>
            /// Dump
            /// </summary>
            public static string DumpDir = "dump";

            /// <summary>
            /// This is a log where network log is dumped to
            /// </summary>
            public static string TransactionLog = "transaction.dat";

            public static string ConfigFile = "wmib.conf";

            public static string ChannelFile = "channels.conf";

            public static string GetChannelFile()
            {
                return Variables.ConfigurationDirectory + Path.DirectorySeparatorChar + Configuration.Paths.ChannelFile;
            }
        }

        public class WebPages
        {
            /// <summary>
            /// Path to html which is generated by this process
            /// </summary>
            public static string HtmlPath = "html";

            /// <summary>
            /// The webpages url
            /// </summary>
            public static string WebpageURL = "";
            public static string Css = "";
        }

        public class IRC
        {
            /// <summary>
            /// Network the bot is connecting to
            /// </summary>
            public static string NetworkHost = "irc.freenode.net";

            /// <summary>
            /// Nick name
            /// </summary>
            public static string NickName = "wm-bot";

            /// <summary>
            /// Login name
            /// </summary>
            public static string LoginNick = null;

            /// <summary>
            /// Login pw
            /// </summary>
            public static string LoginPw = "";

            /// <summary>
            /// Whether the bot is using external network module
            /// </summary>
            public static bool UsingBouncer = false;

            /// <summary>
            /// User name
            /// </summary>
            public static string Username = "wm-bot";

            /// <summary>
            /// Interval between messages are sent to server
            /// </summary>
            public static int Interval = 800;
        }
        
        public class System
        {
            /// <summary>
            /// Uptime
            /// </summary>
            public static DateTime UpTime;

            /// <summary>
            /// Debug channel (doesn't need to exist)
            /// </summary>
            public static string DebugChan = null;

            /// <summary>
            /// Separator for system db
            /// </summary>
            public static string Separator = "|";

            /// <summary>
            /// This is a string which commands are prefixed with
            /// </summary>
            public static string CommandPrefix
            {
                get
                {
                    return prefix;
                }
            }

            public static string prefix = "@";

            /// <summary>
            /// If colors are in terminal
            /// </summary>
            public static bool Colors = true;

            /// <summary>
            /// How verbose the debugging is
            /// </summary>
            public static int SelectedVerbosity = 0;

            /// <summary>
            /// Version
            /// </summary>
            public static string Version = "wikimedia bot v. 2.0.0.0";
        }

        public class MySQL
        {
            /// <summary>
            /// Mysql user
            /// </summary>
            public static string MysqlUser = null;

            /// <summary>
            /// Mysql pw
            /// </summary>
            public static string MysqlPw = null;

            /// <summary>
            /// Mysql host
            /// </summary>
            public static string MysqlHost = null;

            /// <summary>
            /// Mysql db
            /// </summary>
            public static string Mysqldb = null;

            /// <summary>
            /// Mysql port
            /// </summary>
            public static int MysqlPort = 3306;
        }
        
        public class Network
        {
            /// <summary>
            /// Network traffic is logged
            /// </summary>
            public static bool Logging = false;

            /// <summary>
            /// This is a port for default network bouncer
            /// 
            /// This is needed basically for single instance use only
            /// </summary>
            public static int BouncerPort = 6667;

            /// <summary>
            /// This is a port which system console listen on
            /// </summary>
            public static int SystemPort = 2020;
        }

        /// <summary>
        /// List of channels the bot is in, you should never need to use this, use ChannelList instead
        /// </summary>
        public static List<Channel> Channels = new List<Channel>();

        /// <summary>
        /// List of all channels the bot is in, thread safe
        /// </summary>
        public static List<Channel> ChannelList
        {
            get
            {
                List<Channel> list = new List<Channel>();
                lock (Channels)
                {
                    list.AddRange(Channels);
                }
                return list;
            }
        }

        /// <summary>
        /// Add line to the config file
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="text"></param>
        private static void AddConfig(string key, string data, StringBuilder text)
        {
            if (data != null)
            {
                text.Append(key + "=" + data + ";\n");
            }
        }

        /// <summary>
        /// Save a wm-bot channel list
        /// </summary>
        public static void Save()
        {
            StringBuilder text = new StringBuilder("");
            lock (Channels)
            {
                foreach (Channel channel in Channels)
                {
                    text.Append(channel.Name + "\n");
                }
            }
            File.WriteAllText(Variables.ConfigurationDirectory + Path.DirectorySeparatorChar + Configuration.Paths.ChannelFile, 
                              text.ToString());
        }

        /// <summary>
        /// Return a temporary name for a file
        /// </summary>
        /// <param name="file">File you need to have temporary name for</param>
        /// <returns></returns>
        public static string TempName(string file)
        {
            return (file + "~");
        }

        private static Dictionary<string, string> File2Dict()
        {
            Dictionary<string, string> Values = new Dictionary<string, string>();
            string[] xx = File.ReadAllLines(Variables.ConfigurationDirectory + Path.DirectorySeparatorChar +
                                            Configuration.Paths.ConfigFile);
            string LastName = null;
            foreach (string line in xx)
            {
                string content = null;
                if (line == "")
                {
                    continue;
                }
                if (line.StartsWith("//"))
                {
                    continue;
                }
                Syslog.DebugWrite("Parsing line: " + line, 8);
                if (LastName == null && line.Contains("="))
                {
                    LastName = line.Substring(0, line.IndexOf("="));
                    if (Values.ContainsKey(LastName))
                    {
                        throw new Exception("You can't redefine same value in configuration multiple times, error reading: " + LastName);
                    }
                    content = line.Substring(line.IndexOf("=") + 1);
                    if (content.Contains(";"))
                    {
                        content = content.Substring(0, content.IndexOf(";"));
                    }
                    Values.Add(LastName, content);
                    Syslog.DebugWrite("Stored config value: " + LastName + ": " + content);
                    if (line.Contains(";"))
                    {
                        LastName = null;
                    }
                    continue;
                }
                if (LastName != null)
                {
                    content = line;
                    if (!content.Contains(";"))
                    {
                        Syslog.DebugWrite("Append config value: " + LastName + ": " + content);
                        Values[LastName] += "\n" + content;
                    }
                    else
                    {
                        content = content.Substring(0, content.IndexOf(";"));
                        Values[LastName] += "\n" + content;
                        Syslog.DebugWrite("Append config value: " + LastName + ": " + content);
                        LastName = null;
                    }
                    continue;
                }
                Syslog.WriteNow("Invalid configuration line: " + line, true);
            }
            return Values;
        }

        /// <summary>
        /// Load config of bot
        /// </summary>
        public static int Load()
        {
            if (Directory.Exists(Variables.ConfigurationDirectory) == false)
            {
                Directory.CreateDirectory(Variables.ConfigurationDirectory);
            }
            if (!File.Exists(Variables.ConfigurationDirectory + Path.DirectorySeparatorChar + Configuration.Paths.ConfigFile))
            {
                Console.WriteLine("Error: unable to find config file in configuration/" 
                    + Configuration.Paths.ConfigFile
                );
                return 2;
            }
            Dictionary<string, string> Data = File2Dict();
            if (Data.ContainsKey("username"))
            {
                Configuration.IRC.Username = Data["username"];
            }
            if (Data.ContainsKey("network"))
            {
                Configuration.IRC.NetworkHost = Data["network"];
            }
            if (Data.ContainsKey("nick"))
            {
                Configuration.IRC.NickName = Data["nick"];
                Configuration.IRC.LoginNick = Data["nick"];
            }
            if (Data.ContainsKey("debug"))
            {
                Configuration.System.DebugChan = Data["debug"];
            }
            if (Data.ContainsKey("bouncerp"))
            {
                Configuration.Network.BouncerPort = int.Parse(Data["bouncerp"]);
            }
            if (Data.ContainsKey("web"))
            {
                Configuration.WebPages.WebpageURL = Data["web"];
            }
            if (Data.ContainsKey("password"))
            {
                Configuration.IRC.LoginPw = Data["password"];
            }
            if (Data.ContainsKey("mysql_pw"))
            {
                Configuration.MySQL.MysqlPw = Data["mysql_pw"];
            }
            if (Data.ContainsKey("mysql_db"))
            {
                Configuration.MySQL.Mysqldb = Data["mysql_db"];
            }
            if (Data.ContainsKey("mysql_user"))
            {
                Configuration.MySQL.MysqlUser = Data["mysql_user"];
            }
            if (Data.ContainsKey("interval"))
            {
                Configuration.IRC.Interval = int.Parse(Data["interval"]);
            }
            if (Data.ContainsKey("mysql_port"))
            {
                Configuration.MySQL.MysqlPort = int.Parse(Data["mysql_port"]);
            }
            if (Data.ContainsKey("mysql_host"))
            {
                Configuration.MySQL.MysqlHost = Data["mysql_host"];
            }
            if (Data.ContainsKey("style_html_file"))
            {
                Configuration.WebPages.Css = Data["style_html_file"];
            }
            if (Data.ContainsKey("system_port"))
            {
                Configuration.Network.SystemPort = int.Parse(Data["system_port"]);
            }
            if (string.IsNullOrEmpty(Configuration.IRC.LoginNick))
            {
                Console.WriteLine("Error there is no login for bot");
                return 1;
            }
            if (string.IsNullOrEmpty(Configuration.IRC.NetworkHost))
            {
                Console.WriteLine("Error irc server is wrong");
                return 4;
            }
            if (string.IsNullOrEmpty(Configuration.IRC.NickName))
            {
                Console.WriteLine("Error there is no username for bot");
                return 6;
            }
            if (Data.ContainsKey("system_prefix"))
            {
                Configuration.System.prefix = Data["system_prefix"];
            }
            if (Data.ContainsKey("serverIO"))
            {
                Configuration.IRC.UsingBouncer = bool.Parse(Data["serverIO"]);
            }
            Syslog.Log("Loading instances");
            Core.CreateInstance(Configuration.IRC.NickName, Configuration.Network.BouncerPort); // primary instance
            int CurrentInstance = 0;
            while (CurrentInstance < 20)
            {
                if (!Data.ContainsKey("instancename" + CurrentInstance.ToString()))
                {
                    break;
                }
                string InstanceName = Data["instancename" + CurrentInstance.ToString()];
                Syslog.DebugLog("Instance found: " + InstanceName);
                if (Configuration.IRC.UsingBouncer)
                {
                    Syslog.DebugLog("Using bouncer, looking for instance port");
                    if (!Data.ContainsKey("instanceport" + CurrentInstance.ToString()))
                    {
                        Syslog.WriteNow("Instance " + InstanceName + " has invalid port, not using", true);
                        continue;
                    }
                    string InstancePort = Data["instanceport" + CurrentInstance.ToString()];
                    int port = int.Parse(InstancePort);
                    Core.CreateInstance(InstanceName, port);
                } else
                {
                    Core.CreateInstance(InstanceName);
                }
                CurrentInstance++;
            }
            if (!File.Exists(Configuration.Paths.GetChannelFile()))
            {
                Console.WriteLine("Error there is no channel file (" + Configuration.Paths.GetChannelFile() + ") to load channels from");
                return 20;
            }
            foreach (string x in File.ReadAllLines(Configuration.Paths.GetChannelFile()))
            {
                string name = x.Replace(" ", "");
                if (name != "")
                {
                    lock(Channels)
                    {
                        Channels.Add(new Channel(name));
                    }
                }
            }
            Syslog.Log("Channels were all loaded");

            // Now when all chans are loaded let's link them together
            lock(Channels)
            {
                foreach (Channel channel in Channels)
                {
                    channel.SharesNo();
                }
            }

            Syslog.Log("Channel db's working");
            if (!Directory.Exists(Configuration.Paths.DumpDir))
            {
                Directory.CreateDirectory(Configuration.Paths.DumpDir);
            }
            return 0;
        }
    }
}