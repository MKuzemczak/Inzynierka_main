import pika

from python_controller.messages import BaseMessage, get_message_from_json

class RabbitMQCommunicationService:
    class __RabbitMQCommunicationService:
        _in_queue_name = "requests"
        _out_queue_name = "results"

        def __init__(self):
            self.connection = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
            self.channel = self.connection.channel()

            self.channel.queue_declare(queue=self._in_queue_name)
            self.channel.queue_declare(queue=self._out_queue_name)
            self.channel.queue_purge(queue=self._in_queue_name)
            self.channel.queue_purge(queue=self._out_queue_name)

            self._message_subscriptions = {}
        
        def __str__(self):
            return repr(self) + self.val

        def _run_subscriptions(self, message):
            subscriptions = self._message_subscriptions.get(message.__class__.__name__)

            if subscriptions is None:
                return

            for subscription in subscriptions:
                subscription(message)

        def _callback(self, ch, method, properties, body):
            print(body.decode())
            message = get_message_from_json(body.decode())

            if message is None:
                return
            
            self._run_subscriptions(message)

        def start(self):
            self.channel.basic_consume(queue=self._in_queue_name, on_message_callback=self._callback, auto_ack=True)
            self.channel.start_consuming()

        def subscribe(self, message_type: BaseMessage, callback_function):
            message_type_name = message_type.__name__
            subscriptions = self._message_subscriptions.get(message_type_name)

            if subscriptions is None:
                self._message_subscriptions[message_type_name] = [callback_function]
                return
            
            subscriptions.append(callback_function)

        def publish(self, message: BaseMessage):
            json_message = message.to_json()
            print("publishing message: " + json_message)
            self.channel.basic_publish(exchange='', routing_key=self._out_queue_name, body=json_message)

    instance = None
    def __init__(self, *args, **kwargs):
        if not RabbitMQCommunicationService.instance:
            RabbitMQCommunicationService.instance = RabbitMQCommunicationService.__RabbitMQCommunicationService()
    def __getattr__(self, name):
        return getattr(self.instance, name)
