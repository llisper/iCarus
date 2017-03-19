
namespace Experimental
{
    public interface IInputProvider
    {
        void Get(InputParameters parameters);
    }

    public interface IRecorder
    {
        bool recording { get; set; }

        void Subscribe(object obj);
        void Unsubscribe(object obj);
        void Clear();
    }

    public interface IFrameData
    {
        IFrameData Clone();
        bool Compare(IFrameData o);
    }

    public interface IFrame
    {
        string identity { get; }
        IFrameData Save();
        void Load(IFrameData frameData);
    }

    public interface ITrail
    {
        bool recordTrail { get; }
    }
}
