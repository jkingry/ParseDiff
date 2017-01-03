﻿namespace ParseDiff
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class FileDiff : IEquatable<FileDiff>
    {
        public ICollection<ChunkDiff> Chunks { get; } = new List<ChunkDiff>();

        public int Deletions { get; internal set; }
        public int Additions { get; internal set; }

        public string To { get; internal set; }

        public string From { get; internal set; }

        public FileChangeType Type { get; internal set; }

        public bool Deleted => Type == FileChangeType.Delete;

        public bool Add => Type == FileChangeType.Add;

        public IEnumerable<string> Index { get; internal set; }

        private delegate void ParserAction(string line, Match m);

        public static IEnumerable<FileDiff> Parse(string input, string lineEnding = "\n")
        {
            if (string.IsNullOrWhiteSpace(input)) return Enumerable.Empty<FileDiff>();

            var lines = input.Split(new[] { lineEnding }, StringSplitOptions.None);

            if (lines.Length == 0) return Enumerable.Empty<FileDiff>();

            var files = new List<FileDiff>();
            var in_del = 0;
            var in_add = 0;

            ChunkDiff current = null;
            FileDiff file = null;

            int oldStart, newStart;
            int oldLines, newLines;

            ParserAction start = (line, m) => {
                file = new FileDiff();
                files.Add(file);

                if (file.To == null && file.From == null)
                {
                    var fileNames = parseFile(line);

                    if (fileNames != null)
                    {
                        file.From = fileNames[0];
                        file.To = fileNames[1];
                    }
                }
            };

            ParserAction restart = (line, m) => {
                if (file == null || file.Chunks.Count != 0)
                    start(null, null);
            };

            ParserAction new_file = (line, m) => {
                restart(null, null);
                file.Type = FileChangeType.Add;
                file.From = "/dev/null";
            };

            ParserAction deleted_file = (line, m) => {
                restart(null, null);
                file.Type = FileChangeType.Delete;
                file.To = "/dev/null";
            };

            ParserAction index = (line, m) => {
                restart(null, null);
                file.Index = line.Split(' ').Skip(1);
            };

            ParserAction from_file = (line, m) => {
                restart(null, null);
                file.From = parseFileFallback(line);
            };

            ParserAction to_file = (line, m) => {
                restart(null, null);
                file.To = parseFileFallback(line);
            };

            ParserAction chunk = (line, match) => {
                in_del = oldStart = int.Parse(match.Groups[1].Value);
                oldLines = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
                in_add = newStart = int.Parse(match.Groups[3].Value);
                newLines = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : 0;
                current = new ChunkDiff(
                    content: line,
                    oldStart: oldStart,
                    oldLines: oldLines,
                    newStart: newStart,
                    newLines: newLines
                );
                file.Chunks.Add(current);
            };

            ParserAction del = (line, match) => {
                current.Changes.Add(new LineDiff(type: LineChangeType.Delete, index: in_del++, content: line));
                file.Deletions++;
            };

            ParserAction add = (line, m) => {
                current.Changes.Add(new LineDiff(type: LineChangeType.Add, index: in_add++, content: line));
                file.Additions++;
            };

            const string noeol = "\\ No newline at end of file";

            Action<string> normal = line => {
                if (file == null) return;

                current.Changes.Add(new LineDiff(
                    oldIndex: line == noeol ? 0 : in_del++,
                    newIndex: line == noeol ? 0 : in_add++,
                    content: line));
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

        public bool Equals(FileDiff other)
        {
            return
                Equals(Deletions, other.Deletions) &&
                Equals(Additions, other.Additions) &&
                Equals(To, other.To) &&
                Equals(From, other.From) &&
                Equals(Type, other.Type) &&
                IndexEquals(other.Index) &&
                Enumerable.SequenceEqual(Chunks, other.Chunks);
        }

        private bool IndexEquals(IEnumerable<string> otherIndex)
        {
            return 
                Index == null 
                ? otherIndex == null                 
                : (otherIndex != null && Enumerable.SequenceEqual(Index, otherIndex));
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as FileDiff);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + Deletions;
                hash = hash * 23 + Additions;
                hash = hash * 23 + To?.GetHashCode() ?? 0;
                hash = hash * 23 + From?.GetHashCode() ?? 0;
                hash = hash * 23 + Type.GetHashCode();
                hash = hash * 23 + Index?.GetHashCode() ?? 0;
                hash = hash * 23 + Chunks.GetHashCode();
                return hash;
            }
        }
    }
}
