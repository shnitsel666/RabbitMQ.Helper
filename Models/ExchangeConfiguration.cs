namespace RabbitQM.Helper.Models
{
    using System.Collections.Generic;
    using RabbitMQ.Client;

    public class ExchangeConfiguration
    {
        public string ExchangeName { get; set; }

        public string TypeOfExchange { get; set; } = ExchangeType.Direct;

        public bool Durable { get; set; } = true;

        public bool AutoDelete { get; set; } = false;

        public IDictionary<string, object>? Arguments { get; set; }

        public QueueConfiguration Queue { get; set; }

        public uint PrefetchSize { get; set; } = 0u;

        public ushort PrefetchCount { get; set; } = 1;

        public bool Global { get; set; } = false;

        public string ApplicationId { get; set; }

        public string ContentType { get; set; } = "application/json";

        /// <summary>
        /// Использовать повторный вызов обработчика.
        /// Использовать с умом, с x-idempotency-key и проверкой на идемпотентность.
        /// </summary>
        public bool UseRetry { get; set; } = false;

        /// <summary>
        /// Количество попыток повторного вызова обработчика.
        /// </summary>
        public ushort RetryAttempts { get; set; } = 1;

        /// <summary>
        /// Таймаут Retry в мс.
        /// </summary>
        public uint RetryTimeout { get; set; } = 100;

        /// <summary>
        /// Включить/выключить логи для DLX очередей.
        /// </summary>
        public bool DisableLogsDLX { get; set; } = true;
    }
}
