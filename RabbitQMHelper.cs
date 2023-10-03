namespace RabbitQM.Helper
{
    using System;
    using System.Text;
    using Newtonsoft.Json;
    using Polly;
    using Polly.Retry;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using RabbitMQ.Client.Exceptions;
    using RabbitMQ.Client.Helpers;
    using RabbitQM.Helper.Models;

    public class RabbitQMHelper
    {
        private readonly IConnection _currentConnection;

        #region .ctor
        public RabbitQMHelper(RabbitQMConnectionConfig rabbitQMConnectionConfig)
        {
            _currentConnection = CreateNewConnection(rabbitQMConnectionConfig);
        }
        #endregion

        #region CreateNewConnection(rabbitQMConnectionConfig)

        private IConnection CreateNewConnection(RabbitQMConnectionConfig rabbitQMConnectionConfig)
        {
            if (string.IsNullOrEmpty(rabbitQMConnectionConfig.HostName) || !rabbitQMConnectionConfig.Port.HasValue)
            {
                throw new Exception("Не переданы обязательные параметры HostName или Port");
            }

            ConnectionFactory factory = new ()
            {
                HostName = rabbitQMConnectionConfig.HostName,
                Port = rabbitQMConnectionConfig.Port.Value,
                AutomaticRecoveryEnabled = true,
                UserName = rabbitQMConnectionConfig.UserName,
                Password = rabbitQMConnectionConfig.Password,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            };

            return factory.CreateConnection();
        }
        #endregion

        #region GetOrCreateChannel(exchangeConfiguration)

        /// <summary>
        /// Создаёт новый Exchange/Queue или подключается к существующим.
        /// </summary>
        /// <param name="exchangeConfiguration">Настройки Rabbit.</param>
        /// <returns>IModel.</returns>
        public IModel GetOrCreateChannel(ExchangeConfiguration exchangeConfiguration)
        {
            if (exchangeConfiguration == null)
            {
                throw new Exception("Не передана конфигурация подключения к RabbitMQ");
            }

            if (exchangeConfiguration.Queue == null || string.IsNullOrEmpty(exchangeConfiguration.Queue.QueueName))
            {
                throw new Exception("Не передана очередь для RabbitMQ");
            }

            IModel channel = _currentConnection.CreateModel();
            try
            {
                channel.ExchangeDeclarePassive(exchangeConfiguration.ExchangeName);
            }
            catch (Exception ex)
            {
                GrafanaLogHelper.WriteLog($"GetOrCreateChannel() ExchangeDeclarePassive error: {ex.Message}, exchangeName: {exchangeConfiguration.ExchangeName}", GrafanaLogLevel.ERROR);
                GrafanaLogHelper.WriteLog($"GetOrCreateChannel() trying ExchangeDeclare, exchangeName: {exchangeConfiguration.ExchangeName}", GrafanaLogLevel.ERROR);
                channel.ExchangeDeclare(
                    exchange: exchangeConfiguration.ExchangeName,
                    type: exchangeConfiguration.TypeOfExchange,
                    durable: exchangeConfiguration.Durable,
                    autoDelete: exchangeConfiguration.AutoDelete,
                    arguments: exchangeConfiguration.Arguments);
                GrafanaLogHelper.WriteLog($"GetOrCreateChannel(), ExchangeDeclare succesfull, exchangeName: {exchangeConfiguration.ExchangeName}", GrafanaLogLevel.INFO);
            }

            try
            {
                string queueName = channel.QueueDeclarePassive(exchangeConfiguration.Queue.QueueName).QueueName;
                channel.QueueBind(exchangeConfiguration.Queue.QueueName, exchangeConfiguration.ExchangeName, exchangeConfiguration.Queue.RoutingKey, null);
            }
            catch (Exception ex)
            {
                GrafanaLogHelper.WriteLog($"GetOrCreateChannel() QueueDeclarePassive error: {ex.Message}, queueName: {exchangeConfiguration.Queue.QueueName}", GrafanaLogLevel.ERROR);
                GrafanaLogHelper.WriteLog($"GetOrCreateChannel() trying QueueDeclare, queueName: {exchangeConfiguration.Queue.QueueName}", GrafanaLogLevel.ERROR);

                string queueName = channel.QueueDeclare(
                    queue: exchangeConfiguration.Queue.QueueName,
                    durable: exchangeConfiguration.Durable,
                    exclusive: exchangeConfiguration.Queue.Exclusive,
                    autoDelete: exchangeConfiguration.AutoDelete,
                    arguments: exchangeConfiguration.Queue.Arguments).QueueName;
                channel.QueueBind(exchangeConfiguration.Queue.QueueName, exchange: exchangeConfiguration.ExchangeName, routingKey: exchangeConfiguration.Queue.RoutingKey, null);
                GrafanaLogHelper.WriteLog($"GetOrCreateChannel(), QueueDeclare succesfull, queueName: {exchangeConfiguration.ExchangeName}", GrafanaLogLevel.INFO);
            }

            try
            {
                channel.BasicQos(exchangeConfiguration.PrefetchSize, exchangeConfiguration.PrefetchCount, exchangeConfiguration.Global);
            }
            catch (Exception ex)
            {
                GrafanaLogHelper.WriteLog($"GetOrCreateChannel(), BasicQos error: {ex.Message}", GrafanaLogLevel.ERROR);
            }

            return channel;
        }
        #endregion

        #region SendDataInBatchToRabbit(dataToSend, configuration)

        /// <summary>
        /// Метод отправки данных в очередь.
        /// </summary>
        /// <typeparam name="T">Тип данных.</typeparam>
        /// <param name="dataToSend">Данные.</param>
        /// <param name="configuration">Настройки Rabbit.</param>
        /// <returns>Статус отправки.</returns>
        public Response<bool> SendDataInBatchToRabbit<T>(T dataToSend, ExchangeConfiguration configuration) =>
            Response<bool>.DoMethod(response =>
            {
                response.ThrowIfNull(response, -1, $"{DateTime.Now} | SendDataInBatchToRabbit() | Не переданы данные в RabbitMQ");

                response.ThrowIfEmptyString(configuration.ApplicationId, -2, $"{DateTime.Now} | SendDataInBatchToRabbit() | Не передан ApplicationId для отправки в RabbitMQ");

                response.ThrowIfEmptyString(configuration.ContentType, -3, $"{DateTime.Now} | SendDataInBatchToRabbit() | Не передан тип данных ContentType для отправки в RabbitMQ");

                try
                {
                    IModel channel = GetOrCreateChannel(configuration);
                    string body = JsonConvert.SerializeObject(dataToSend);
                    IBasicProperties msgTypes = channel.CreateBasicProperties();
                    msgTypes.ContentType = configuration.ContentType;
                    msgTypes.AppId = configuration.ApplicationId;
                    msgTypes.CorrelationId = Guid.NewGuid().ToString();
                    channel.BasicPublish(
                        exchange: configuration.ExchangeName,
                        routingKey: configuration.Queue.RoutingKey,
                        basicProperties: msgTypes,
                        body: Encoding.UTF8.GetBytes(body));
                    response.Data = true;
                }
                catch (BrokerUnreachableException ex)
                {
                    response.Code = -100;
                    response.Message = ex.Message;
                    GrafanaLogHelper.WriteLog($"{DateTime.Now} | SendDataInBatchToRabbit(BrokerUnreachableException), Ошибка при отправке данных в очередь RabbitMQ: {ex.Message}", GrafanaLogLevel.ERROR);
                    Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    response.Code = -200;
                    response.Message = ex.Message;
                    GrafanaLogHelper.WriteLog($"{DateTime.Now} | SendDataInBatchToRabbit(Exception), Ошибка при отправке данных в очередь RabbitMQ: {ex.Message}", GrafanaLogLevel.ERROR);
                    Thread.Sleep(5000);
                }
            });
        #endregion

        #region StartExchange(configuration, methodToInvoke, errorHandlerToInvoke)

        /// <summary>
        /// Старт обмена сообщениями.
        /// </summary>
        /// <param name="configuration">Настройки Rabbit.</param>
        /// <param name="methodToInvoke">Обработчик сообщения.</param>
        /// <param name="errorHandlerToInvoke">Обработчик ошибок.</param>
        public void StartExchange(ExchangeConfiguration configuration, Action<string>? methodToInvoke, Action<string, string>? errorHandlerToInvoke)
        {
            string queueName = configuration.Queue.QueueName;
            string routingKey = configuration.Queue.RoutingKey;
            string logPrefix = $"ExchangeName: {configuration.ExchangeName}, queueName: {queueName}";
            IModel channel = GetOrCreateChannel(configuration);
            EventingBasicConsumer consumer = new (channel);
            RetryPolicy retry = Policy.Handle<Exception>()
            .WaitAndRetry(
                configuration.RetryAttempts,
                retryAttempt => TimeSpan.FromMilliseconds(configuration.RetryTimeout * retryAttempt),
                onRetry: (exception, sleepDuration, attemptNumber, context) =>
                {
                    if (!configuration.DisableLogsDLX)
                    {
                        GrafanaLogHelper.WriteLog($"{logPrefix} | retry exception: {exception.Message}, retry attempt: {attemptNumber}", GrafanaLogLevel.WARNING);
                    }

                    if (configuration.RetryAttempts < attemptNumber)
                    {
                        ulong deliveryTag = (ulong)context["deliveryTag"];
                        string message = (string)context["message"];
                        channel.BasicNack(deliveryTag, false, false); // Если настроено в RMQ, то сообщение улетит в DLX очередь.
                        if (errorHandlerToInvoke != null)
                        {
                            if (!configuration.DisableLogsDLX)
                            {
                                GrafanaLogHelper.WriteLog($"{logPrefix}, errors delegate invoking...", GrafanaLogLevel.WARNING);
                            }

                            errorHandlerToInvoke?.Invoke(exception.Message, message);

                            if (!configuration.DisableLogsDLX)
                            {
                                GrafanaLogHelper.WriteLog($"{logPrefix}, errors delegate invoked...", GrafanaLogLevel.WARNING);
                            }
                        }

                        return;
                    }
                });
            consumer.Received += (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                try
                {
                    if (ea.BasicProperties.AppId.Equals(configuration.ApplicationId) && ea.BasicProperties.ContentType.Equals(configuration.ContentType))
                    {
                        if (!string.IsNullOrEmpty(message))
                        {
                            Action<Context> act = (_) =>
                            {
                                methodToInvoke?.Invoke(message);
                                channel.BasicAck(ea.DeliveryTag, false);
                            };
                            if (configuration.UseRetry)
                            {
                                Dictionary<string, object> contextData = new ()
                                {
                                    { "message", message },
                                    { "message", message },
                                };
                                retry.Execute(act, contextData);
                                return;
                            }
                            else
                            {
                                methodToInvoke?.Invoke(message);
                                channel.BasicAck(ea.DeliveryTag, false);
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (!configuration.DisableLogsDLX)
                        {
                            GrafanaLogHelper.WriteLog($"{logPrefix} | received empty message", GrafanaLogLevel.WARNING);
                        }

                        channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    if (!configuration.DisableLogsDLX)
                    {
                        GrafanaLogHelper.WriteLog($"{logPrefix} | received: {message}, exception: {ex.Message}", GrafanaLogLevel.ERROR);
                        GrafanaLogHelper.WriteLog($"{logPrefix} | errors delegate invoking...", GrafanaLogLevel.WARNING);
                    }

                    channel.BasicNack(ea.DeliveryTag, false, false);
                    errorHandlerToInvoke?.Invoke(ex.Message, message);

                    if (!configuration.DisableLogsDLX)
                    {
                        GrafanaLogHelper.WriteLog($"{logPrefix} | errors delegate invoked", GrafanaLogLevel.WARNING);
                    }
                }
            };
            string consumerTag = channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        }
        #endregion
    }
}
