namespace RabbitMQ.Client.Helpers
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    /// <summary>
    /// Уровни логирования в Grafana.
    /// </summary>
    public enum GrafanaLogLevel
    {
        /// <summary>
        /// Самое критичное, что могло произойти и возможно повлекло падение приложения
        /// </summary>
        [EnumMember(Value = "critical")]
        CRITICAL = 1,

        /// <summary>
        /// Ошибка в ПО
        /// </summary>
        [EnumMember(Value = "error")]
        ERROR = 2,

        /// <summary>
        /// Предупреждение по событии, которые не должно было случиться
        /// </summary>
        [EnumMember(Value = "warning")]
        WARNING = 3,

        /// <summary>
        /// Информационное сообщение
        /// </summary>
        [EnumMember(Value = "info")]
        INFO = 4,

        /// <summary>
        /// Информация для отладки
        /// </summary>
        [EnumMember(Value = "debug")]
        DEBUG = 5,

        /// <summary>
        /// Содержимое стэка
        /// </summary>
        [EnumMember(Value = "trace")]
        TRACE = 6,

        /// <summary>
        /// Неизвестный или еще неопределенный уроверь логирование
        /// </summary>
        [EnumMember(Value = "unknown")]
        UNKNOWN = 0,
    }

    public static class GrafanaLogHelper
    {
        public static void WriteLog(string message, GrafanaLogLevel logLevel = GrafanaLogLevel.INFO, long? duration = null, string? remoteIP = null, [CallerMemberName] string methodName = "", string? transactionId = null, int? errorCode = null, int? httpStatus = null, [CallerLineNumber] int sourceLineNumber = 0)
        {
            string message2log = $"message=\"{message}\" dt=\"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture)}\" level={logLevel}";

            if (!string.IsNullOrEmpty(methodName))
            {
                message2log += $" methodName={methodName}";
            }

            if (sourceLineNumber != 0)
            {
                message2log += $" lineNumber={sourceLineNumber}";
            }

            if (duration.HasValue)
            {
                message2log += $" duration={duration}ms";
            }

            if (!string.IsNullOrEmpty(remoteIP))
            {
                message2log += $" remoteIP={remoteIP}";
            }

            if (!string.IsNullOrEmpty(transactionId))
            {
                message2log += $" transactionId={transactionId}";
            }

            if (errorCode.HasValue)
            {
                message2log += $" errorCode={errorCode}";
            }

            if (httpStatus.HasValue)
            {
                message2log += $" httpStatus={httpStatus}";
            }

            Console.WriteLine(message2log);
        }
    }
}