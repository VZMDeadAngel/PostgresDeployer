using CommandLine;
using Npgsql;
using PostgresDeployer.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace PostgresDeployer
{
    public class Options
    {
        [Option('c', "config", Required = false, HelpText = "Config full path.")]
        public string ConfigFilePath { get; set; }
    }

    class Program
    {
        static Settings settings = null;

        static Regex jobCreate = new Regex(@"insert( *)into( *)pgagent\.pga_job", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex tablecreate = new Regex(@"^create *table", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex tableName = new Regex("create *table *(if *not *exists *)?([\\w.\"]*)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex tableFK = new Regex("references *([\"\\w]*.?[\\w\"]*[\\w\"])", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex typecreate = new Regex(@"^create *type", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex sequencecreate = new Regex(@"^create *sequence", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex schemas = new Regex(@"^create *schema", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex views = new Regex(@"^create *view", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        static string[] excludefiles = new string[]
        {
        };

        // -c or --config with config file name
        static void Main(string[] args)
        {
            Console.WriteLine(@"     ________  ________  ________  _________  ________  ________  _______   ________         ");
            Console.WriteLine(@"    |\   __  \|\   __  \|\   ____\|\___   ___\\   ____\|\   __  \|\  ___ \ |\   ____\        ");
            Console.WriteLine(@"    \ \  \|\  \ \  \|\  \ \  \___|\|___ \  \_\ \  \___|\ \  \|\  \ \   __/|\ \  \___|_       ");
            Console.WriteLine(@"     \ \   ____\ \  \\\  \ \_____  \   \ \  \ \ \  \  __\ \   _  _\ \  \_|/_\ \_____  \      ");
            Console.WriteLine(@"      \ \  \___|\ \  \\\  \|____|\  \   \ \  \ \ \  \|\  \ \  \\  \\ \  \_|\ \|____|\  \     ");
            Console.WriteLine(@"       \ \__\    \ \_______\____\_\  \   \ \__\ \ \_______\ \__\\ _\\ \_______\____\_\  \    ");
            Console.WriteLine(@"        \|__|     \|_______|\_________\   \|__|  \|_______|\|__|\|__|\|_______|\_________\   ");
            Console.WriteLine(@"                           \|_________|                                       \|_________|   ");
            Console.WriteLine(@"     ________  _______   ________  ___       ________      ___    ___ _______   ________     ");
            Console.WriteLine(@"    |\   ___ \|\  ___ \ |\   __  \|\  \     |\   __  \    |\  \  /  /|\  ___ \ |\   __  \    ");
            Console.WriteLine(@"    \ \  \_|\ \ \   __/|\ \  \|\  \ \  \    \ \  \|\  \   \ \  \/  / | \   __/|\ \  \|\  \   ");
            Console.WriteLine(@"     \ \  \ \\ \ \  \_|/_\ \   ____\ \  \    \ \  \\\  \   \ \    / / \ \  \_|/_\ \   _  _\  ");
            Console.WriteLine(@"      \ \  \_\\ \ \  \_|\ \ \  \___|\ \  \____\ \  \\\  \   \/  /  /   \ \  \_|\ \ \  \\  \| ");
            Console.WriteLine(@"       \ \_______\ \_______\ \__\    \ \_______\ \_______\__/  / /      \ \_______\ \__\\ _\ ");
            Console.WriteLine(@"        \|_______|\|_______|\|__|     \|_______|\|_______|\___/ /        \|_______|\|__|\|__|");
            Console.WriteLine(@"                                                         \|___|/                             ");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Parsing command line parameters
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       string configFileName;
                       if (string.IsNullOrEmpty(o.ConfigFilePath))
                       {
                           configFileName = File.Exists($"{Environment.MachineName}.xml") ? $"{Environment.MachineName}.xml" : $"Default.xml";
                       }
                       else
                       {
                           configFileName = o.ConfigFilePath;
                       }

                       // Open a configuration file
                       Console.Write("Opening a settings file");
                       Console.WriteLine("\t....ok");
                       Console.WriteLine();

                       XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                       StreamReader reader = new StreamReader(configFileName);
                       settings = (Settings)serializer.Deserialize(reader);
                       reader.Close();
                   });

            Console.Write("Reading and sorting all objects in the database");
            List<ScriptObjectFile> DBObjects = new List<ScriptObjectFile>();

            // Getting the whole list of files
            DirSearch(settings.Common.ScriptsFilesPath, ref DBObjects);

            var tableNodes = new HashSet<string>(DBObjects.Where(t => t.scriptType == ScriptType.Table).Select(t => t.Name.Replace("\"", "")).ToArray());
            var tableEdges = new HashSet<Tuple<string, string>>();
            foreach (ScriptObjectFile t in DBObjects.Where(t => t.scriptType == ScriptType.Table))
            {
                foreach (string To in t.FK)
                {
                    // There should be no references to ourselves
                    if (To != t.Name)
                    {
                        tableEdges.Add(Tuple.Create(t.Name.Replace("\"", ""), To.Replace("\"", "")));
                    }
                }
            }

            var ret = TopologicalSort(tableNodes, tableEdges);

            // Be sure to do the reverse, because... have to go from the end of the connection
            ret.Reverse();
            Console.WriteLine("\t....ok");

            // Recreating the entire database
            string tempDBConnectionstring = $"Server={settings.Common.TargetTempDataBase.Host};Port={settings.Common.TargetTempDataBase.Port};Database={settings.Common.TargetTempDataBase.DatabaseName};User Id={settings.Common.TargetTempDataBase.User};Password={settings.Common.TargetTempDataBase.Password};";
            string postgresConnectionstring = $"Server={settings.Common.TargetTempDataBase.Host};Port={settings.Common.TargetTempDataBase.Port};Database=postgres;User Id={settings.Common.TargetTempDataBase.User};Password={settings.Common.TargetTempDataBase.Password};";
            using (var conn = new NpgsqlConnection(postgresConnectionstring))
            {
                conn.Open();
                Console.Write($"Rebuild the database \"{settings.Common.TargetTempDataBase.DatabaseName}\"");

                dropDatabase(conn, settings.Common.TargetTempDataBase.DatabaseName);
                createDatabase(conn, settings.Common.TargetTempDataBase.DatabaseName);
            }
            Console.WriteLine("\t....ok");
            Console.WriteLine();

            using (var conn = new NpgsqlConnection(tempDBConnectionstring))
            {
                conn.Open();

                // Creating schemas
                Console.WriteLine($"\nCreating schemas");
                foreach (ScriptObjectFile f in DBObjects.Where(t => t.scriptType == ScriptType.Schema))
                {
                    Console.Write($"File {f.FileName.Split('\\').Last()}.");
                    using (var cmd = new NpgsqlCommand(File.ReadAllText(f.FileName), conn))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("\t....ok");
                    }
                }

                Console.WriteLine($"\nCreating sequences");
                foreach (ScriptObjectFile f in DBObjects.Where(t => t.scriptType == ScriptType.Sequence))
                {
                    Console.Write($"File {f.FileName.Split('\\').Last()}.");
                    using (var cmd = new NpgsqlCommand(File.ReadAllText(f.FileName), conn))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("\t....ok");
                    }
                }

                // Creating tables
                Console.WriteLine($"\nCreating tables");
                foreach (string tableName in ret)
                {
                    ScriptObjectFile currentTable = DBObjects.FirstOrDefault(t => (t.Name != null) && (t.Name.Replace("\"", "") == tableName.Replace("\"", "")));
                    Console.Write($"Table {currentTable.Name}.");
                    using (var cmd = new NpgsqlCommand(File.ReadAllText(currentTable.FileName), conn))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("\t....ok");
                    }
                }

                // Creating views
                Console.WriteLine($"\nCreating views");
                foreach (ScriptObjectFile f in DBObjects.Where(t => t.scriptType == ScriptType.View))
                {
                    Console.Write($"File {f.FileName.Split('\\').Last()}.");
                    using (var cmd = new NpgsqlCommand(File.ReadAllText(f.FileName), conn))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("\t....ok");
                    }
                }

                Console.WriteLine($"\nCreating types");
                foreach (ScriptObjectFile f in DBObjects.Where(t => t.scriptType == ScriptType.CustomType))
                {
                    Console.Write($"File {f.FileName.Split('\\').Last()}.");
                    using (var cmd = new NpgsqlCommand(File.ReadAllText(f.FileName), conn))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("\t....ok");
                    }
                }

                Console.WriteLine($"\nProcessing of other objects");
                foreach (ScriptObjectFile f in DBObjects.Where(t => t.scriptType == ScriptType.Another).OrderBy(t => t.FileName))
                {
                    Console.Write($"File {f.FileName.Split('\\').Last()}.");
                    using (var cmd = new NpgsqlCommand(File.ReadAllText(f.FileName), conn))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("\t....ok");
                    }
                }

                Console.WriteLine($"\nPost deployment scripts");
                foreach (ScriptObjectFile f in DBObjects.Where(t => t.scriptType == ScriptType.PostDeploymentScript).OrderBy(t => Path.GetFileName(t.FileName)))
                {
                    Console.Write($"File {f.FileName.Split('\\').Last()}.");
                    using (var cmd = new NpgsqlCommand(File.ReadAllText(f.FileName), conn))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("\t....ok");
                    }
                }
            }

            // Handle each environment separately
            foreach (var env in settings.Enviroments)
            {
                string targetDBConnectionstring = $"Server={env.TargetDataBase.Host};Port={env.TargetDataBase.Port};Database={env.TargetDataBase.DatabaseName};User Id={env.TargetDataBase.User};Password={env.TargetDataBase.Password};";
                string targetDBPostgresConnectionstring = $"Server={env.TargetDataBase.Host};Port={env.TargetDataBase.Port};Database=postgres;User Id={env.TargetDataBase.User};Password={env.TargetDataBase.Password};";

                // Assume there is a target database and try to connect to it
                using (var conn = new NpgsqlConnection(targetDBConnectionstring))
                {
                    try
                    {
                        conn.Open();
                    }
                    catch
                    {
                        // It's not possible to connect to the database. It must be created by connecting to the postgres database
                        using (var pconn = new NpgsqlConnection(targetDBPostgresConnectionstring))
                        {
                            pconn.Open();
                            createDatabase(pconn, env.TargetDataBase.DatabaseName);
                        }
                    }
                }

                // Create a directory for scripts
                DateTime n = DateTime.Now;
                string filePath = $"{env.Name}/{n.Year}-{n.Month.ToString("00")}-{n.Day.ToString("00")}_{n.Hour.ToString("00")}-{n.Minute.ToString("00")}-{n.Second.ToString("00")}_Script.sql";
                Directory.CreateDirectory($"{env.Name}");
                File.WriteAllText(filePath, $"-- Deployment script for database '{env.TargetDataBase.DatabaseName}'\n\n");

                foreach (string method in new string[] { "SCHEMA", "SEQUENCE", "TABLE", "COLUMN", "INDEX", "VIEW", "FOREIGN_KEY", "FUNCTION" })
                {
                    Process compiler = new Process();
                    compiler.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "pgdiff.exe");
                    compiler.StartInfo.Arguments = $"-U \"{settings.Common.TargetTempDataBase.User}\" -W \"{settings.Common.TargetTempDataBase.Password}\" -H \"{settings.Common.TargetTempDataBase.Host}\" -P {settings.Common.TargetTempDataBase.Port} -D \"{settings.Common.TargetTempDataBase.DatabaseName}\" -u \"{env.TargetDataBase.User}\" -w \"{env.TargetDataBase.Password}\" -h \"{env.TargetDataBase.Host}\" -p {env.TargetDataBase.Port} -d \"{env.TargetDataBase.DatabaseName}\" -O \"sslmode=disable\" -o \"sslmode=disable\" {method}";
                    compiler.StartInfo.UseShellExecute = false;
                    compiler.StartInfo.RedirectStandardOutput = true;
                    compiler.Start();

                    string rez = compiler.StandardOutput.ReadToEnd();

                    rez = convert(Encoding.GetEncoding("utf-8"), Encoding.GetEncoding("cp866"), rez);

                    File.AppendAllText(filePath, $"{rez}\n");
                    compiler.WaitForExit();
                }

                // Add to the common file
                WritePostDeploymentScriptFile(DBObjects, filePath);

                if (env.ExecuteOnTarget)
                {
                    using (var conn = new NpgsqlConnection(targetDBConnectionstring))
                    {
                        conn.Open();

                        NpgsqlTransaction transaction;
                        transaction = conn.BeginTransaction();
                        using (NpgsqlCommand cmd = new NpgsqlCommand(File.ReadAllText(filePath), conn, transaction))
                        {
                            try
                            {
                                cmd.ExecuteNonQuery();
                                transaction.Commit();
                            }
                            catch (PostgresException e)
                            {
                                Console.WriteLine($"\n\nERROR: {e.Message}\n{e.MessageText}\n{e.Hint}\n{e.Where}\n\n");
                                transaction.Rollback();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"\n\nERROR: {e.Message}\n\n");
                                transaction.Rollback();
                            }
                        }
                    }
                }
            }

            // Deleting the temporary database
            using (var conn = new NpgsqlConnection(postgresConnectionstring))
            {
                Console.WriteLine($"Deleting the temporary database '{settings.Common.TargetTempDataBase.DatabaseName}'");
                conn.Open();
                dropDatabase(conn, settings.Common.TargetTempDataBase.DatabaseName);
            }

            Console.WriteLine("\nCompleted");
        }

        static string convert(Encoding srcEnc, Encoding dstEnc, string src)
        {
            byte[] srcBytes = srcEnc.GetBytes(src);
            byte[] dstBytes = Encoding.Convert(srcEnc, dstEnc, srcBytes);
            return srcEnc.GetString(dstBytes);
        }

        static void dropDatabase(NpgsqlConnection connection, string DatabaseName)
        {
            try
            {
                using (var cmd = new NpgsqlCommand($"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE pid <> pg_backend_pid() AND datname = '{DatabaseName}';drop database \"{DatabaseName}\";", connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {

            }
        }

        static void createDatabase(NpgsqlConnection connection, string DatabaseName)
        {
            using (var cmd = new NpgsqlCommand($"create database \"{DatabaseName}\";", connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        static void WritePostDeploymentScriptFile(List<ScriptObjectFile> DBObjects, string filePath)
        {
            foreach (ScriptObjectFile f in DBObjects.Where(t => t.scriptType == ScriptType.PostDeploymentScript).OrderBy(t => Path.GetFileName(t.FileName)))
            {
                File.AppendAllText(filePath, File.ReadAllText(f.FileName));
                File.AppendAllText(filePath, "\n");
            }
        }

        // Topological sorting function of the graph
        static List<T> TopologicalSort<T>(HashSet<T> nodes, HashSet<Tuple<T, T>> edges) where T : IEquatable<T>
        {
            // Empty list that will contain the sorted elements
            var L = new List<T>();

            // Set of all nodes with no incoming edges
            var S = new HashSet<T>(nodes.Where(n => edges.All(e => e.Item2.Equals(n) == false)));

            // while S is non-empty do
            while (S.Any())
            {

                //  remove a node n from S
                var n = S.First();
                S.Remove(n);

                // add n to tail of L
                L.Add(n);

                // for each node m with an edge e from n to m do
                foreach (var e in edges.Where(e => e.Item1.Equals(n)).ToList())
                {
                    var m = e.Item2;

                    // remove edge e from the graph
                    edges.Remove(e);

                    // if m has no other incoming edges then
                    if (edges.All(me => me.Item2.Equals(m) == false))
                    {
                        // insert m into S
                        S.Add(m);
                    }
                }
            }

            // if graph has edges then
            if (edges.Any())
            {
                // return error (graph has at least one cycle)
                return null;
            }
            else
            {
                // return L (a topologically sorted order)
                return L;
            }
        }

        static void DirSearch(string sDir, ref List<ScriptObjectFile> files)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d))
                    {
                        if ((f.Split('.').Last().ToLower() == "sql")
                            &&
                           (!excludefiles.Contains(f.ToLower())))
                        {
                            ScriptObjectFile file = new ScriptObjectFile() { FileName = f };
                            ProcessFile(ref file);
                            files.Add(file);
                        }
                    }
                    DirSearch(d, ref files);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        static void ProcessFile(ref ScriptObjectFile objectFile)
        {
            objectFile.FK = new List<string>();
            string scriptText = File.ReadAllText(objectFile.FileName);
            if (jobCreate.IsMatch(scriptText))
            {
                objectFile.scriptType = ScriptType.Jobs;
            }
            else if ( Path.GetFullPath(objectFile.FileName).Contains(Path.GetFullPath(settings.Common.PostDeploymentScriptsFilesPath)))
            {
                objectFile.scriptType = ScriptType.PostDeploymentScript;
            }
            else if (tablecreate.IsMatch(scriptText))
            {
                // We declare that this table
                objectFile.scriptType = ScriptType.Table;
                objectFile.Name = tableName.Matches(scriptText)[0].Groups[2].Value;
                objectFile.Name = objectFile.Name.Contains(".") ? objectFile.Name.Split('.')[1] : objectFile.Name;

                // Looking for the keys fk
                foreach (Match m in tableFK.Matches(scriptText))
                {
                    objectFile.FK.Add(m.Groups[1].Value.Contains(".") ? m.Groups[1].Value.Split('.')[1] : m.Groups[1].Value);
                }
            }
            else if (typecreate.IsMatch(scriptText))
            {
                objectFile.scriptType = ScriptType.CustomType;
            }
            else if (sequencecreate.IsMatch(scriptText))
            {
                objectFile.scriptType = ScriptType.Sequence;
            }
            else if (schemas.IsMatch(scriptText))
            {
                objectFile.scriptType = ScriptType.Schema;
            }
            else if (views.IsMatch(scriptText))
            {
                objectFile.scriptType = ScriptType.View;
            }
            else
            {
                objectFile.scriptType = ScriptType.Another;
            }
        }
    }
}
