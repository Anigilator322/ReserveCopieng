using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
namespace ReserveCopieng
{
    
    internal class SettingsOfCopieng
    {

        private string[] sourceFolder;
        private string destinationFolder;
        private int levelOfLogging;
        public SettingsOfCopieng(string[] SourceFolder, string DestinationFolder, int levelOfLogging)
        {
            this.sourceFolder = SourceFolder;
            this.destinationFolder = DestinationFolder;
            this.levelOfLogging = levelOfLogging;
        }
        public string[] SourceFolder { get { return sourceFolder; } }
        public string DestinationFolder { get { return destinationFolder; } }
        public int LevelOfLogging { get { return levelOfLogging; } }
    }
    public class LogWriter
    {
        public enum TypeOfLog
        {
            Error,
            Info,
            Debug
        }
        private StreamWriter sw;
        private int levelOfLogging;
        private string pathToLog;
        public LogWriter(int levelOfLogging, string pathToLog)
        {
            this.levelOfLogging = levelOfLogging;
            this.pathToLog = Path.Combine(pathToLog, "logs.txt");
            sw = new StreamWriter(this.pathToLog,true, Encoding.UTF8);
        }
        public void WriteLog(TypeOfLog logType,string msg)
        {
            
            switch(levelOfLogging)
            {
                case 1:
                    {
                        if (logType == TypeOfLog.Error) sw.WriteLine("<" + logType.ToString() + ">" + " " + msg);
                        break;
                    }
                case 2:
                    {
                        if (logType == TypeOfLog.Info) sw.WriteLine("<" + logType.ToString() + ">" + " " + msg);
                        break;
                    }
                case 3:
                    {
                        if (logType == TypeOfLog.Debug) sw.WriteLine("<" + logType.ToString() + ">" + " " + msg);
                        break;
                    }
                default:
                    {
                        sw.WriteLine("<" + logType.ToString() + ">" + " " + msg);
                        break;
                    }

            }
     
        }
        public void EndOfLogging()
        {
            sw.Close();
        }

    }
    internal class Program
    {
        
        static void Main(string[] args)
        {
            string time = DateTime.Now.ToString().Replace(':', '_');
            var jsonText = File.ReadAllText("CopySettings.json");
            
            SettingsOfCopieng jsonSettings = JsonConvert.DeserializeObject<SettingsOfCopieng>(jsonText);

            DirectoryInfo newDir = Directory.CreateDirectory(Path.Combine(jsonSettings.DestinationFolder, time));

            LogWriter logWriter = new LogWriter(jsonSettings.LevelOfLogging,newDir.FullName);
            logWriter.WriteLog(LogWriter.TypeOfLog.Info, "Start");
            logWriter.WriteLog(LogWriter.TypeOfLog.Debug, "Создана конечная папка");
            foreach(string sourceFl in jsonSettings.SourceFolder )
            {
                string[] pathsToFiles = Directory.GetFiles(sourceFl);
                foreach (string file in pathsToFiles)
                {
                    try
                    {
                        string fileName = Path.GetFileName(file);
                        string destFile = Path.Combine(newDir.FullName, fileName);
                        File.Copy(file, destFile, true);
                        logWriter.WriteLog(LogWriter.TypeOfLog.Debug, "скопирован 1 файл");
                    }
                    catch (Exception ex)
                    {
                        logWriter.WriteLog(LogWriter.TypeOfLog.Error, ex.Message);
                    }
                }
                logWriter.WriteLog(LogWriter.TypeOfLog.Info, "Исходная папка обработана");
            }
            
            
            logWriter.WriteLog(LogWriter.TypeOfLog.Info, "End");
            logWriter.EndOfLogging();
        }
    }
}

