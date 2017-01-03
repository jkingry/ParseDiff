namespace ParseDiff
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ChunkDiff : IEquatable<ChunkDiff>
    {
        public ChunkDiff(string content, int oldStart, int oldLines, int newStart, int newLines)
        {
            Content = content;
            OldStart = oldStart;
            OldLines = oldLines;
            NewStart = newStart;
            NewLines = newLines;
        }

        public ICollection<LineDiff> Changes { get; } = new List<LineDiff>();

        public string Content { get; }

        public int OldStart { get; }

        public int OldLines { get; }

        public int NewStart { get; }

        public int NewLines { get; }

        public bool Equals(ChunkDiff other)
        {
            return
                Equals(Content, other.Content) &&
                Equals(OldStart, other.OldStart) &&
                Equals(OldLines, other.OldLines) &&
                Equals(NewStart, other.NewStart) &&
                Equals(NewLines, other.NewLines) &&
                Enumerable.SequenceEqual(Changes, other.Changes);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ChunkDiff);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + Content?.GetHashCode() ?? 0;
                hash = hash * 23 + OldStart;
                hash = hash * 23 + OldLines;
                hash = hash * 23 + NewStart;
                hash = hash * 23 + NewLines;
                hash = hash * 23 + Changes.GetHashCode();
                return hash;
            }
        }
    }
}
