using UnityEngine;
using log4net.Core;
using log4net.Appender;

namespace iCarus.Log
{
    class UnityLogAppender : AppenderSkeleton
    {
        protected override void Append(LoggingEvent loggingEvent)
        {
            string message = RenderLoggingEvent(loggingEvent);
            if (Level.Compare(loggingEvent.Level, Level.Error) >= 0)
                Debug.LogError(message);
            else if (Level.Compare(loggingEvent.Level, Level.Warn) >= 0)
                Debug.LogWarning(message);
            else
                Debug.Log(message);
        }
    }
}
