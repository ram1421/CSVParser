using Autofac;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVReader
{
    class CSVReader
    {
       static DataTable dtMerged = new DataTable();
        private static IContainer Container { get; set; }


        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<SQLite>().As<ISQLLite>();
       
            Container = builder.Build();

            getCSVFiles();
        }

        public static void getCSVFiles()
        {

            using (var scope = Container.BeginLifetimeScope())
            {
                var writer = scope.Resolve<ISQLLite>();
                dtMerged.TableName = "tblMerged";
                var numberOfSavedRecords = writer.SaveDataTable(dtMerged);
                Console.WriteLine($" ***** CSV READER SUCCESSFULLY SAVED {numberOfSavedRecords} *****");
            }



            string sourceDirectory = @"C:\Learn\Demo\CollegeScorecard_Raw_Data";

            var csvFiles = Directory.EnumerateFiles(sourceDirectory, "*.csv", SearchOption.AllDirectories);

            string str = "**##***** CSV READER  *****##**";
            Console.SetCursorPosition((Console.WindowWidth - str.Length) / 2, Console.CursorTop);
            Console.WriteLine(str);
            int count = 1;
            foreach (string currentFile in csvFiles)
            {
                if (!isFileLocked(currentFile))
                {
                    Console.WriteLine($" Started processing {count}:{currentFile}");
                    drawTextProgressBar(count, csvFiles.Count());
                    loadCSVToDataTable(currentFile);
                    count++;
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.WriteLine($" FILE LOCKED {count}:{currentFile}");
                }

            }
            using (var scope = Container.BeginLifetimeScope())
            {
                var writer = scope.Resolve<ISQLLite>();
                dtMerged.TableName = "tblMerged";
                var numberOfSavedRecords = writer.SaveDataTable(dtMerged);
                Console.WriteLine($" ***** CSV READER SUCCESSFULLY SAVED {numberOfSavedRecords} *****");
            }
            
            Console.WriteLine($" ***** CSV READER PARSING COMPLETED {csvFiles.Count()} *****");
            Console.ReadKey();
        }
        protected static bool isFileLocked(string fileName)
        {
            FileStream stream = null;

            try
            {
                var fileInfo = new FileInfo(fileName);
                stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None);                
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        private static void drawTextProgressBar(int progress, int total)
        {
            //draw empty progress bar
            Console.CursorLeft = 0;
            Console.Write("["); //start
            Console.CursorLeft = 32;
            Console.Write("]"); //end
            Console.CursorLeft = 1;
            float onechunk = 30.0f / total;

            //draw filled part
            int position = 1;
            for (int i = 0; i < onechunk * progress; i++)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw unfilled part
            for (int i = position; i <= 31; i++)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw totals
            Console.CursorLeft = 35;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(progress.ToString() + " of " + total.ToString() + "    "); //blanks at the end remove any excess
        }
       
        public static void loadCSVToDataTable(string filePath)
        {
            
            var dt = new DataTable();
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader))
            {
              
                // Do any configuration to `CsvReader` before creating CsvDataReader.
                using (var dr = new CsvDataReader(csv))
                {
                    //drawTextProgressBar(0, dr.)
                    dtMerged.Load(dr);                   
                }
                Console.WriteLine($" DT Merged item count  {dtMerged.Rows.Count} *****");

            }

        }

    }
}
