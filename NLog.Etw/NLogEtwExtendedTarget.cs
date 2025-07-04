﻿using System.Diagnostics.Tracing;
using NLog.Targets;

namespace NLog.Etw
{
    /// <summary>
    /// A NLog target with support for channel-enabled ETW tracing. When using perfview or wpr to record the events use *LowLevelDesign-NLogEtwSource
    /// to enable the NLog provider.
    /// 
    /// Sample configuration and usage sample can be found on my blog: http://lowleveldesign.wordpress.com/2014/04/18/etw-providers-for-nlog/
    /// 
    /// Channel alignment based on best practices documented here: https://blogs.msdn.microsoft.com/vancem/2012/08/14/etw-in-c-controlling-which-events-get-logged-in-an-system-diagnostics-tracing-eventsource/ 
    /// </summary>
    [Target("ExtendedEventTracing")]
    public sealed class NLogEtwExtendedTarget : TargetWithLayout
    {
        [EventSource(Name = "LowLevelDesign-NLogEtwSource")]
        private sealed class EtwLogger : EventSource, INLogEventSource
        {
            [Event(1, Level = EventLevel.Verbose, Message = "{0}: {1}", Channel = EventChannel.Debug)]
            public void Verbose(string LoggerName, string Message)
            {
                WriteEvent(1, LoggerName, Message);
            }

            [Event(2, Level = EventLevel.Informational, Message = "{0}: {1}", Channel = EventChannel.Operational)]
            public void Info(string LoggerName, string Message)
            {
                WriteEvent(2, LoggerName, Message);
            }

            [Event(3, Level = EventLevel.Warning, Message = "{0}: {1}", Channel = EventChannel.Admin)]
            public void Warn(string LoggerName, string Message)
            {
                WriteEvent(3, LoggerName, Message);
            }

            [Event(4, Level = EventLevel.Error, Message = "{0}: {1}", Channel = EventChannel.Admin)]
            public void Error(string LoggerName, string Message)
            {
                WriteEvent(4, LoggerName, Message);
            }

            [Event(5, Level = EventLevel.Critical, Message = "{0}: {1}", Channel = EventChannel.Admin)]
            public void Critical(string LoggerName, string Message)
            {
                WriteEvent(5, LoggerName, Message);
            }

            internal readonly static EtwLogger Log = new EtwLogger();

            [NonEvent]
            void INLogEventSource.Write(EventLevel eventLevel, string layoutMessage, LogEventInfo logEvent)
            {
                switch (eventLevel)
                {
                    case EventLevel.Verbose:
                        Verbose(logEvent.LoggerName, layoutMessage);
                        break;
                    case EventLevel.Informational:
                        Info(logEvent.LoggerName, layoutMessage);
                        break;
                    case EventLevel.Warning:
                        Warn(logEvent.LoggerName, layoutMessage);
                        break;
                    case EventLevel.Error:
                        Error(logEvent.LoggerName, layoutMessage);
                        break;
                    default:
                        Critical(logEvent.LoggerName, layoutMessage);
                        break;
                }
            }

            EventSource INLogEventSource.EventSource => this;
        }

        /// <summary>
        /// Public property that allows the NLog Configuration Engine to recognize any NLog Layout on custom EventSource-implementation
        /// </summary>
        public INLogEventSource EventSource { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EtwEventSourceTarget"/> class.
        /// </summary>
        public NLogEtwExtendedTarget()
        {
            EventSource = EtwLogger.Log;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EtwEventSourceTarget"/> class.
        /// </summary>
        public NLogEtwExtendedTarget(INLogEventSource eventSource)
        {
            EventSource = eventSource;
        }

        /// <summary>
        /// Write to event to ETW.
        /// </summary>
        /// <param name="logEvent">event to be written.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            if (EventSource?.EventSource?.IsEnabled() == true)
            {
                if (logEvent.Level == LogLevel.Debug || logEvent.Level == LogLevel.Trace)
                {
                    WriteEvent(logEvent, EventLevel.Verbose);
                }
                else if (logEvent.Level == LogLevel.Info)
                {
                    WriteEvent(logEvent, EventLevel.Informational);
                }
                else if (logEvent.Level == LogLevel.Warn)
                {
                    WriteEvent(logEvent, EventLevel.Warning);
                }
                else if (logEvent.Level == LogLevel.Error)
                {
                    WriteEvent(logEvent, EventLevel.Error);
                }
                else //if (logEvent.Level == LogLevel.Fatal)
                {
                    WriteEvent(logEvent, EventLevel.Critical);
                }
            }
        }

        /// <summary>
        /// Write to event source, if enabled for that level
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="level"></param>
        private void WriteEvent(LogEventInfo logEvent, EventLevel level)
        {
            if (EventSource.EventSource.IsEnabled(level, EventKeywords.None))
            {
                var message = RenderLogEvent(Layout, logEvent);
                EventSource.Write(level, message, logEvent);
            }
        }
    }
}