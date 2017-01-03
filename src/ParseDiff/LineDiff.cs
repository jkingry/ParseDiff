namespace ParseDiff
{
    using System;

    public class LineDiff : IEquatable<LineDiff>
    {
        public LineDiff(LineChangeType type, int index, string content)
        {
            Type = type;
            Index = index;
            Content = content;
        }

        public LineDiff(int oldIndex, int newIndex, string content)
        {
            OldIndex = oldIndex;
            NewIndex = NewIndex;
            Type = LineChangeType.Normal;
            Content = content;
        }

        public bool Add => Type == LineChangeType.Add;

        public bool Delete => Type == LineChangeType.Delete;

        public bool Normal => Type == LineChangeType.Normal;

        public string Content { get; }

        public int Index { get; }

        public int OldIndex { get; }

        public int NewIndex { get; }

        public LineChangeType Type { get; }

        public bool Equals(LineDiff other)
        {
            return
              Equals(Content, other.Content) &&
              Equals(Index, other.Index) &&
              Equals(OldIndex, other.OldIndex) &&
              Equals(NewIndex, other.NewIndex) &&
              Equals(Type, other.Type);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LineDiff);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + Content?.GetHashCode() ?? 0;
                hash = hash * 23 + Index;
                hash = hash * 23 + OldIndex;
                hash = hash * 23 + NewIndex;
                hash = hash * 23 + Type.GetHashCode();
                return hash;
            }
        }
    }
}
