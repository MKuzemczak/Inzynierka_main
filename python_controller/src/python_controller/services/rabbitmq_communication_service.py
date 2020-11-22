import pika

from python_controller.messages import get_message_from_json

class RabbitMQCommunicationService:
    class __RabbitMQCommunicationService:
        _in_queue_name = "requests"
        _out_queue_name = "results"

        def __init__(self):
            self.connection = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
            self.channel = connection.channel()

            self.channel.queue_declare(queue=self._in_queue_name)
            self.channel.queue_declare(queue=self._out_queue_name)
            self.channel.queue_purge(queue=self._in_queue_name)
            self.channel.queue_purge(queue=self._out_queue_name)

            self._message_subscriptions = {}

        def _callback(self, ch, method, properties, body):
            message = get_message_from_json(str(body))

            if message is None:
                return

            subscriptions = self._message_subscriptions.get(message.__class__.__name__)

            if subscriptions is None:
                return

            for subscription in subscriptions:
                subscription(message)

        def start(self):
            self.channel.basic_consume(queue=self._in_queue_name, on_message_callback=self._callback, auto_ack=True)
            self.channel.start_consuming()

        def __str__(self):
            return repr(self) + self.val

    instance = None
    def __init__(self, *args, **kwargs):
        if not RabbitMQCommunicationService.instance:
            RabbitMQCommunicationService.instance = RabbitMQCommunicationService.__RabbitMQCommunicationService(
                args, kwargs)
    def __getattr__(self, name):
        return getattr(self.instance, name)
