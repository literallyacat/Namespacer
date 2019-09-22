using System;
using System.IO;
using CommandLine;

namespace namespacer {
    public class Options {
        [Option('n', "namespace", Required = true, HelpText = "The root namespace to use when replacing.")]
        public string RootNamespace { get; set; }

        [Option('s', "searchPath", Required = true, HelpText = "The folder to start the search in.")]
        public string SearchPath { get; set; }
    }
    class Program {
        private const string INDENT = "    ";
        private static string _assetPath;
        static string AssetsPath
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_assetPath))
                {
                    var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    var fullpath = Path.GetFullPath(path);

                    var rel = Path.GetRelativePath("../../", fullpath);
                    var assetsPath = fullpath.Replace(rel, string.Empty);
                    _assetPath = Path.Join(assetsPath, "Assets/Modules");
                }
                return _assetPath;
            }
        }

        static void Main(string[] args)
        {

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                   {
                       ReplaceNamespace(o.SearchPath, o.RootNamespace);
                       Console.Write("All set!");
                   });
        }

        static string ComputeNamespace(string filepath, string rootNamespace)
        {
            var trim = filepath.Replace(AssetsPath, string.Empty);
            var directories = trim.Split(Path.DirectorySeparatorChar);
            var nspc = rootNamespace;
            foreach (var dir in directories)
            {
                if (dir == "Scripts") continue;
                if (string.IsNullOrWhiteSpace(dir)) continue;
                if (dir.Contains(".cs")) continue;
                nspc += $".{dir.Replace(".cs", string.Empty)}";
            }
            return nspc;
        }

        static void ApplyNamespace(string file, string nspc)
        {
            var content = File.ReadAllText(file);

            if (content.Contains("namespace "))
            {
                var startIdx = content.IndexOf("namespace") + "namespace".Length;
                var currentNamespace = content.Substring(
                    startIdx,
                    content.IndexOf("{", startIdx) - startIdx
                ).Trim();
                content = content.Replace(currentNamespace, nspc);
            }
            else
            {
                content = content.Replace(Environment.NewLine, $"{Environment.NewLine}{INDENT}")
                                .Replace($"{Environment.NewLine}{INDENT}{Environment.NewLine}", $"{Environment.NewLine}{Environment.NewLine}").TrimEnd();
                content = $@"namespace {nspc} {{
{content}
}}";
            }

            File.WriteAllText(file, content);
        }

        static void ReplaceNamespace(string startingDirectory, string rootNamespace)
        {
            try
            {
                foreach (string dir in Directory.GetDirectories(startingDirectory))
                {
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        if (file.EndsWith(".cs"))
                        {
                            Console.WriteLine(file);
                            var currentNamespace = ComputeNamespace(file, rootNamespace);
                            ApplyNamespace(file, currentNamespace);
                        }
                    }
                    ReplaceNamespace(dir, rootNamespace);
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
