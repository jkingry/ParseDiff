using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ParseDiff
{
    public class LineChange
    {
        internal bool add;
        internal string content;
        internal bool del;
        internal int @in;
        internal int in1;
        internal int in2;
        internal bool normal;
        internal string type;
    }

    public class Chunk
    {
        public string content;
        public List<LineChange> changes = new List<LineChange>();
        public int oldStart, oldLines, newStart, newLines;
    }

    public class DiffResult
    {
        public List<Chunk> chunks = new List<Chunk>();
        public int deletions = 0;
        public int additions = 0;
        public string to;
        public string from;
        internal bool @new;
        internal bool deleted;
        internal IEnumerable<string> index;
    }

    delegate void ParserAction(string line, Match m);

    class Program
    {
        static void Main(string[] args)
        {
            var text = File.ReadAllText(@"c:\r\shoplogix\diff.txt");
            foreach(var p in Parse(text))
            {
                Console.WriteLine(p.from);
            }
        }

        public static IEnumerable<DiffResult> Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return Enumerable.Empty<DiffResult>();

            var lines = input.Split('\n');

            if (lines.Length == 0) return Enumerable.Empty<DiffResult>();

            var files = new List<DiffResult>();
            var in_del = 0;
            var in_add = 0;

            Chunk current = null;
            DiffResult file = null;

            int oldStart, newStart;
            int oldLines, newLines;

            ParserAction start = (line, m) => {
                file = new DiffResult();
                files.Add(file);

                if (file.to == null && file.from == null)
                {
                    var fileNames = parseFile(line);

                    if (fileNames != null)
                    {
                        file.from = fileNames[0];
                        file.to = fileNames[1];
                    }
                }
            };

            ParserAction restart = (line, m) => {
                if (file == null || file.chunks.Count != 0)
                    start(null, null);
            };

            ParserAction new_file = (line, m) => {
                restart(null, null);
                file.@new = true;
                file.from = "/dev/null";
            };

            ParserAction deleted_file = (line, m) => {
                restart(null, null);
                file.deleted = true;
                file.to = "/dev/null";
            };

            ParserAction index = (line, m) => {
                restart(null, null);
                file.index = line.Split(' ').Skip(1);
            };

            ParserAction from_file = (line, m) => {
                restart(null, null);
                file.from = parseFileFallback(line);
            };

            ParserAction to_file = (line, m) => {
                restart(null, null);
                file.to = parseFileFallback(line);
            };

            ParserAction chunk = (line, match) => {
                in_del = oldStart = int.Parse(match.Groups[1].Value);
                oldLines = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
                in_add = newStart = int.Parse(match.Groups[3].Value);
                newLines = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : 0;
                current = new Chunk {
                    content = line,
                    oldStart = oldStart,
                    oldLines = oldLines,
                    newStart = newStart,
                    newLines = newLines
                };
                file.chunks.Add(current);
            };

            ParserAction del = (line, match) => {
                current.changes.Add(new LineChange { type = "del", del = true, @in = in_del++, content = line });
                file.deletions++;
            };

            ParserAction add = (line, m) => {
                current.changes.Add(new LineChange { type = "add", add = true, @in = in_add++, content = line });
                file.additions++;
            };

            const string noeol = "\\ No newline at end of file";

            Action<string> normal = line => {
                if (file == null) return;

                current.changes.Add(new LineChange {
                    type = "normal",
                    normal = true,
                    in1 = line == noeol ? 0 : in_del++,
                    in2 = line == noeol ? 0 : in_add++,
                    content = line
                });
            };

            var schema = new Dictionary<Regex, ParserAction>
            {
                    { new Regex(@"^diff\s"), start },
                    { new Regex(@"^new file mode \d+$"), new_file },
                    { new Regex(@"^deleted file mode \d+$"), deleted_file },
                    { new Regex(@"^index\s[\da-zA-Z]+\.\.[\da-zA-Z]+(\s(\d+))?$"), index },
                    { new Regex(@"^---\s"), from_file },
                    { new Regex(@"^\+\+\+\s"), to_file },
                    { new Regex(@"^@@\s+\-(\d+),?(\d+)?\s+\+(\d+),?(\d+)?\s@@"), chunk },
                    { new Regex(@"^-"), del },
                    { new Regex(@"^\+"), add }
            };

            Func<string, bool> parse = line => {
                foreach (var p in schema)
                {
                    var m = p.Key.Match(line);
                    if (m.Success)
                    {
                        p.Value(line, m);
                        return true;
                    }
                }

                return false;
            };

            foreach (var line in lines)
                if (!parse(line))
                    normal(line);

            return files;
        }

        private static string[] parseFile(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            return s
                .Split(' ')
                .Reverse().Take(2).Reverse()
                .Select(fileName => Regex.Replace(fileName, @"^(a|b)\/", "")).ToArray();
        }

        private static string parseFileFallback(string s)
        {
	        s = s.TrimStart('-', '+');
            s = s.Trim();

	        // ignore possible time stamp
	        var t = new Regex(@"\t.*|\d{4}-\d\d-\d\d\s\d\d:\d\d:\d\d(.\d+)?\s(\+|-)\d\d\d\d").Match(s);
            if (t.Success)
            {
                s = s.Substring(0, t.Index).Trim();
            }

	        // ignore git prefixes a/ or b/
	        return Regex.IsMatch(s, @"^(a|b)\/") 
                ? s.Substring(2) 
                : s;
        }
    }
}
