using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Windows.UI.Core;
using Windows.UI.Xaml;

using Inzynierka.CommunicationService.Messages;
using Inzynierka.Exceptions;
using Inzynierka.MainThreadDispatcher;
using Inzynierka.StateMessaging;

namespace Inzynierka.CommunicationService
{
    public sealed class RabbitMQCommunicationService
    {
        public static string PythonQueueName = "inzynierka_python";
        public static string IncomingQueueName = "inzynierka_app";
        public static string LauncherQueueName = "inzynierka_launcher";

        public bool Initialized = false;

        private static RabbitMQCommunicationService m_oInstance = null;
        private static readonly object m_oPadLock = new object();

        private ConnectionFactory Factory { get; set; }
        private IConnection Connection { get; set; }
        private IModel ConnectionModel { get; set; }

        private List<string> Queues = new List<string>();

        private string CurrentReceivedQueue { get; set; }
        private string CurrentReceivedMessage { get; set; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public static RabbitMQCommunicationService Instance
        {
            get
            {
                lock (m_oPadLock)
                {
                    if (m_oInstance == null)
                    {
                        m_oInstance = new RabbitMQCommunicationService();
                    }
                    return m_oInstance;
                }
            }
        }

        private RabbitMQCommunicationService()
        {
        }

        public void Initialize()
        {
            Factory = new ConnectionFactory() { HostName = "localhost" };
            Connection = Factory.CreateConnection();
            ConnectionModel = Connection.CreateModel();
            DeclareOutgoingQueue(PythonQueueName);
            DeclareOutgoingQueue(LauncherQueueName);
            DeclareIncomingQueue(IncomingQueueName);
            Initialized = true;
        }

        private void DeclareOutgoingQueue(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Queue name should consist of non-whitespace characters", nameof(name));
            }

            if (Queues.Contains(name))
            {
                throw new QueueAlreadyExistsException(name);
            }

            ConnectionModel.QueueDeclare(queue: name,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            Queues.Add(name);
            CleanQueue(name);
        }

        private void DeclareIncomingQueue(string name)
        {
            if (Queues.Contains(name))
            {
                throw new QueueAlreadyExistsException(name);
            }

            ConnectionModel.QueueDeclare(queue: name,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            Queues.Add(name);
            CleanQueue(name);

            var consumer = new EventingBasicConsumer(ConnectionModel);
            consumer.Received += Receiver;
            ConnectionModel.BasicConsume(queue: name,
                autoAck: true,
                consumer: consumer);

        }

        public void Send(BaseIndication message)
        {
            if (!Initialized)
            {
                throw new NotInitializedException();
            }

            if (!Queues.Contains(message.Receiver))
            {
                throw new QueueDoesntExistException();
            }

            var messageBodyJson = message.ToJson();
            var wrappingMessage = new WrappingMessage()
            {
                ClassName = message.GetType().Name,
                Body = messageBodyJson
            };
            var wrappingMessageJson = JsonSerializer.Serialize(wrappingMessage);
            var body = Encoding.UTF8.GetBytes(wrappingMessageJson);

            ConnectionModel.BasicPublish(exchange: "",
                routingKey: message.Receiver,
                basicProperties: null,
                body: body);
        }

        private async void Receiver(object model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            string wrappingMessageJson = Encoding.ASCII.GetString(body);
            var outerMessage = JsonSerializer.Deserialize<WrappingMessage>(wrappingMessageJson);
            Type messageType;
            try
            {
                messageType = MessageDictionary.GetTypeFromClassName(outerMessage.ClassName);
            }
            catch(MessageTypeNotFoundException e)
            {
                StateMessagingService.Instance.SendInfoMessage(e.Message, 5000);

                return;
            }

            var deserializeMethod = typeof(JsonSerializer).GetMethod("Deserialize", new[] { typeof(string), typeof(JsonSerializerOptions) });
            var deserializeRef = deserializeMethod.MakeGenericMethod(messageType);
            var messageBody = deserializeRef.Invoke(null,
                new object[]
                {
                    outerMessage.Body,
                    new JsonSerializerOptions()
                    {
                        Converters = { new JsonStringEnumConverter() }
                    }
                });

            await MainThreadDispatcherService.MarshalToMainThreadAsync(() => MessageReceived?.Invoke(this, new MessageReceivedEventArgs(messageBody)));
        }

        public void CleanQueue(string queueName)
        {
            ConnectionModel.QueuePurge(queueName);
        }

    }
}
