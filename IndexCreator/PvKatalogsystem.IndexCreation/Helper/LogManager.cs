using System;
using System.Diagnostics;
using System.IO;
using PvKatalogsystem.IndexCreation.IndexCreators;

namespace PvKatalogsystem.IndexCreation.Helper
{
    public static class LogManager
    {
        private static TextWriter Writer { get; set; }
        private static string Status { get; set; }

        public static void Start(string logPath, bool deleteOldLogs)
        {

            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
                Console.WriteLine(string.Format("Verzeichnis '{0}' wurde erstellt.", logPath));
            }
            var logFilePath = Path.Combine(logPath, "info.log");
            if (deleteOldLogs && File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }
            Writer = TextWriter.Synchronized(new StreamWriter(logFilePath));

            Writer.WriteLine();
            Writer.WriteLine(string.Format("[{0}] ### START INDEXCREATION ###", DateTime.Now));
            Writer.WriteLine();
            Writer.Flush();
        }

        public static void SetStatus(string status)
        {
            for (var i = status.Length; i < Console.WindowWidth; i++)
            {
                status += " ";
            }
            Status = status;
            WriteStatus();
        }

        public static void WriteStatus()
        {
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(Status);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void Info(string message)
        {
            Info(message, null);
        }
        public static void Info(string message, string name)
        {
            if (name != null)
                message = string.Format("{0}: {1}", name, message);

            Writer.WriteLine(string.Format("[{0}] {1}", DateTime.Now, message));
            Writer.Flush();

            Console.WriteLine(message);
        }

        public static void Debug(string message)
        {
            Debug(message, null);
        }
        public static void Debug(string message, string name)
        {
            if (name != null)
                message = String.Format("{0}: {1}", name, message);

            Writer.WriteLine(String.Format("[{0}] {1}", DateTime.Now, message));
            Writer.Flush();

            Console.WriteLine(message);
        }

        public static void Error(string message, Exception ex, string name)
        {
            Error(new Exception(message, ex), name);
        }
        public static void Error(string message, Exception ex)
        {
            Error(message, ex, null);
        }
        public static void Error(string message)
        {
            Error(message, null, null);
        }
        public static void Error(Exception ex)
        {
            Error(ex, null);
        }
        public static void Error(Exception ex, string name)
        {
            var message = ex.Message;
            if (name != null)
                message = string.Format("{0}: {1}", name, message);

            Writer.WriteLine(string.Format("[{0}] {1}", DateTime.Now, message));
            Writer.Flush();

            if (ex != null) Writer.WriteLine(ex);

            Console.WriteLine(ex.Message);
        }

        public static void FatalError(string message, Exception ex)
        {
            var source = new StackTrace().GetFrame(1).GetMethod().Name;

            Writer.WriteLine(string.Format("[{0}] {1}", DateTime.Now, message));
            if (ex != null) Writer.WriteLine(ex);
            Writer.Flush();

            Console.WriteLine(message);
            Console.WriteLine("Anwendung wird beendet. Weitere Informationen entnehmen Sie den aktuellen Log-Files.");

            Environment.Exit(1);

        }

        public static void Delimiter(string title)
        {
            if (title.Length > 60)
                title = title.Substring(0, 60).ToUpper();
            var delimLength = 79 - title.Length - 2;
            var msg = "";
            for (var i = 0; i < Math.Floor((float)delimLength / 2); i++)
                msg += "*";
            msg += String.Format(" {0} ", title);
            for (var i = 0; i < Math.Ceiling((float)delimLength / 2); i++)
                msg += "*";

            Writer.WriteLine();
            Writer.WriteLine(msg);
            Writer.WriteLine();
            Writer.Flush();
        }
    }
}
