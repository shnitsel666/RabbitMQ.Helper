namespace RabbitQM.Helper
{
    public class RabbitQMConnectionConfig
    {
        /// <summary>
        /// Адрес хоста.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Порт.
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// Логин.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Пароль.
        /// </summary>
        public string? Password { get; set; }
    }
}
