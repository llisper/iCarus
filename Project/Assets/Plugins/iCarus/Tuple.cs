
namespace iCarus
{
    public static class Tuple
    {
        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2) { return new Tuple<T1, T2>(item1, item2); }
        public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3) { return new Tuple<T1, T2, T3>(item1, item2, item3); }
    }

    public interface ITuple
    {
        int size { get; }
    }

    public class Tuple<T1, T2> : ITuple
    {
        public Tuple(T1 item1, T2 item2)
        {
            this.item1 = item1;
            this.item2 = item2;
        }

        public T1 item1 { get; private set; }
        public T2 item2 { get; private set; }
        public int size { get { return 2; } }
    }

    public class Tuple<T1, T2, T3> : ITuple
    {
        public Tuple(T1 item1, T2 item2, T3 item3)
        {
            this.item1 = item1;
            this.item2 = item2;
            this.item3 = item3;
        }

        public T1 item1 { get; private set; }
        public T2 item2 { get; private set; }
        public T3 item3 { get; private set; }
        public int size { get { return 3; } }
    }
}
