import abc
import json

class BaseMessage(abc.ABC):

    @property
    @abc.abstractmethod
    def sender(self):
        """
            Return name of sender (ideally name of rabbitmq queue from which the sender receives messages)
        """

    @property
    @abc.abstractmethod
    def receiver(self):
        """
            Return name of receiver (ideally name of rabbitmq queue from which receiver receives messages)
        """

    @abc.abstractmethod
    def to_json(self) -> str:
        """
            Returns a json representation of the message
        """

    @classmethod
    @abc.abstractmethod
    def get_instance(cls, sender: str, receiver: str, contents: list = []):
        """
            Extracts message contents from the dict and returns an instance of the class
        """

    def _prepare_json(self, contents: dict) -> str:
        return json.dumps({"name": self.__class__.__name__, "contents": contents})
