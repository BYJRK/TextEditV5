using System;

namespace TextEditV5.PairExchange
{
    public class PairElement : IComparable
    {
        public string Left { get; set; }
        public string Right { get; set; }
        string Comparator { get { return Left + Right; } }

        public PairElement()
        {
            Left = Right = string.Empty;
        }
        public PairElement(string left, string right)
        {
            Left = left;
            Right = right;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                throw new NullReferenceException();
            return Comparator.CompareTo(((PairElement)obj).Comparator);
        }
    }
}
