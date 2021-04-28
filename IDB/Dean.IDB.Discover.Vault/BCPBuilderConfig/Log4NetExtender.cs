using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Core;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;


namespace BCPBuilderConfig
{

    // define the delegate so they can be passed
    // delegate to handle the update to a delegate receiving the message
    public delegate void UpdateTextDelegate(string text);

    public interface ILog4NetExtender : ILog
    {
        void SetUpdateTextDelegate(UpdateTextDelegate updateTextDelegate);
    }


    public class Log4NetExtender : ILog4NetExtender
    {
        public ILog m_ILog;

        // if this is threaded, use a delegate to update the TextBox text
        static protected UpdateTextDelegate mUpdateTextDelegate;
        public void SetUpdateTextDelegate(UpdateTextDelegate updateTextDelegate)
        {
            mUpdateTextDelegate = updateTextDelegate;
        }


        // method to check if a delegate is given, send the same string to that for processing
        protected void CallUpdateTextDelegate(object message)
        {
            if (mUpdateTextDelegate != null)
            {
                mUpdateTextDelegate((string)message + Environment.NewLine);
            }
        }

        // method to check if a delegate is given, send the same string and exception to that for processing
        protected void CallUpdateTextDelegate(object message, Exception exception)
        {
            if (mUpdateTextDelegate != null)
            {
                mUpdateTextDelegate((string)message + Environment.NewLine);
                mUpdateTextDelegate(exception.Message + Environment.NewLine);
                mUpdateTextDelegate(exception.ToString() + Environment.NewLine);
            }
        }

        // method to check if a delegate is given, send the same string to that for processing
        protected void CallUpdateTextDelegate(string format, params object[] args)
        {
            if (mUpdateTextDelegate != null)
            {
                mUpdateTextDelegate(string.Format(format, args) + Environment.NewLine);
            }
        }

        // method to check if a delegate is given, send the same string to that for processing
        protected void CallUpdateTextDelegate(string format, object arg0)
        {
            if (mUpdateTextDelegate != null)
            {
                mUpdateTextDelegate(string.Format(format, arg0) + Environment.NewLine);
            }
        }


        // method to check if a delegate is given, send the same string to that for processing
        protected void CallUpdateTextDelegate(string format, object arg0, object arg1)
        {
            if (mUpdateTextDelegate != null)
            {
                mUpdateTextDelegate(string.Format(format, arg0, arg1) + Environment.NewLine);
            }
        }

        // method to check if a delegate is given, send the same string to that for processing
        protected void CallUpdateTextDelegate(string format, object arg0, object arg1, object arg2)
        {
            if (mUpdateTextDelegate != null)
            {
                mUpdateTextDelegate(string.Format(format, arg0, arg1, arg2) + Environment.NewLine);
            }
        }


        public Log4NetExtender(ILog iLog)
        {
            m_ILog = iLog;
        }

        // implement parameters
        public bool IsDebugEnabled
        { get { return (m_ILog.IsDebugEnabled); } }

        public bool IsInfoEnabled
        { get { return (m_ILog.IsInfoEnabled); } }


        public bool IsWarnEnabled
        { get { return (m_ILog.IsWarnEnabled); } }


        public bool IsErrorEnabled
        { get { return (m_ILog.IsErrorEnabled); } }


        public bool IsFatalEnabled
        { get { return (m_ILog.IsFatalEnabled); } }


        public ILogger Logger
        { get { return (m_ILog.Logger); } }





        // implement methods
        public void Debug(object message)
        {
            m_ILog.Debug(message);

            CallUpdateTextDelegate(message);
        }

        // method to get the "RollingLogFileAppenderDebug" appender
        // so that we can get the name
        public log4net.Appender.RollingFileAppender GetRollingLogFileAppenderDebug()
        {
            log4net.Repository.ILoggerRepository repository = LogManager.GetRepository();
            List<log4net.Appender.IAppender> appenders = repository.GetAppenders().ToList();
            log4net.Appender.IAppender appender = appenders.Where(ap => string.Compare(ap.Name, "RollingLogFileAppenderDebug", true) == 0).FirstOrDefault();

            log4net.Appender.RollingFileAppender rollingFileAppender = null;
            if (appender != null)
            {
                rollingFileAppender = (log4net.Appender.RollingFileAppender)appender;
            }

            return (rollingFileAppender);
        }



        public void Debug(object message, Exception exception)
        {
            m_ILog.Debug(message, exception);

            // display an error message
            log4net.Appender.RollingFileAppender rollingFileAppender = GetRollingLogFileAppenderDebug();
            MessageBox.Show(string.Format("Exception unhandled: View log file for details \"{0}\"", rollingFileAppender != null ? rollingFileAppender.File : ""));

            CallUpdateTextDelegate(message, exception);
        }


        public void DebugFormat(string format, params object[] args)
        {
            m_ILog.DebugFormat(format, args);

            CallUpdateTextDelegate(format, args);
        }

        public void DebugFormat(string format, object arg0)
        {
            m_ILog.DebugFormat(format, arg0);

            CallUpdateTextDelegate(format, arg0);
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            m_ILog.DebugFormat(format, arg0, arg1);

            CallUpdateTextDelegate(format, arg0, arg1);
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            m_ILog.DebugFormat(format, arg0, arg1, arg2);

            CallUpdateTextDelegate(format, arg0, arg1, arg2);
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            m_ILog.DebugFormat(provider, format, args);

            CallUpdateTextDelegate(format, args);
        }

        public void Error(object message)
        {
            m_ILog.Error(message);

            CallUpdateTextDelegate(message);
        }

        public void Error(object message, Exception exception)
        {
            m_ILog.Error(message, exception);

            CallUpdateTextDelegate(message, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            m_ILog.ErrorFormat(format, args);

            CallUpdateTextDelegate(format, args);
        }

        public void ErrorFormat(string format, object arg0)
        {
            m_ILog.ErrorFormat(format, arg0);

            CallUpdateTextDelegate(format, arg0);
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            m_ILog.ErrorFormat(format, arg0, arg1);

            CallUpdateTextDelegate(format, arg0, arg1);
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            m_ILog.ErrorFormat(format, arg0, arg1, arg2);

            CallUpdateTextDelegate(format, arg0, arg1, arg2);
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            m_ILog.ErrorFormat(provider, format, args);

            CallUpdateTextDelegate(format, args);
        }

        public void Fatal(object message)
        {
            m_ILog.Fatal(message);

            CallUpdateTextDelegate(message);
        }

        public void Fatal(object message, Exception exception)
        {
            m_ILog.Fatal(message, exception);

            CallUpdateTextDelegate(message, exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            m_ILog.FatalFormat(format, args);

            CallUpdateTextDelegate(format, args);
        }

        public void FatalFormat(string format, object arg0)
        {
            m_ILog.FatalFormat(format, arg0);
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            m_ILog.FatalFormat(format, arg0, arg1);

            CallUpdateTextDelegate(format, arg0, arg1);
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            m_ILog.FatalFormat(format, arg0, arg1, arg2);

            CallUpdateTextDelegate(format, arg0, arg1, arg2);
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            m_ILog.FatalFormat(provider, format, args);

            CallUpdateTextDelegate(format, args);
        }

        public void Info(object message)
        {
            m_ILog.Info(message);

            CallUpdateTextDelegate(message);
        }

        public void Info(object message, Exception exception)
        {
            m_ILog.Info(message, exception);

            CallUpdateTextDelegate(message, exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            m_ILog.InfoFormat(format, args);

            CallUpdateTextDelegate(format, args);
        }

        public void InfoFormat(string format, object arg0)
        {
            m_ILog.InfoFormat(format, arg0);

            CallUpdateTextDelegate(format, arg0);
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            m_ILog.InfoFormat(format, arg0, arg1);

            CallUpdateTextDelegate(format, arg0, arg1);
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            m_ILog.InfoFormat(format, arg0, arg1, arg2);

            CallUpdateTextDelegate(format, arg0, arg1, arg2);
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            m_ILog.InfoFormat(provider, format, args);

            CallUpdateTextDelegate(format, args);
        }

        public void Warn(object message)
        {
            m_ILog.Warn(message);

            CallUpdateTextDelegate(message);
        }

        public void Warn(object message, Exception exception)
        {
            m_ILog.Warn(message, exception);

            CallUpdateTextDelegate(message, exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            m_ILog.WarnFormat(format, args);

            CallUpdateTextDelegate(format, args);
        }

        public void WarnFormat(string format, object arg0)
        {
            m_ILog.WarnFormat(format, arg0);

            CallUpdateTextDelegate(format, arg0);
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            m_ILog.WarnFormat(format, arg0, arg1);

            CallUpdateTextDelegate(format, arg0, arg1);
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            m_ILog.WarnFormat(format, arg0, arg1, arg2);

            CallUpdateTextDelegate(format, arg0, arg1, arg2);
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            m_ILog.WarnFormat(provider, format, args);

            CallUpdateTextDelegate(format, args);
        }
    }
}
