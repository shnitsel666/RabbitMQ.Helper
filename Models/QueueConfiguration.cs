namespace RabbitQM.Helper.Models
{
    public class QueueConfiguration
    {
        public string QueueName { get; set; }

        public bool Exclusive { get; set; } = false;

        public string RoutingKey { get; set; }

        public IDictionary<string, object>? Arguments { get; set; }
    }
}
