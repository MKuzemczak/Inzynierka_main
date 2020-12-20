import abc
import json

class BaseIndication(abc.ABC):

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
    
    @property
    @abc.abstractmethod
    def message_id(self):
        """
            Return id of thte request (request and results messages share the same id)
        """

    @abc.abstractmethod
    def to_json(self) -> str:
        """
            Returns a json representation of the message
        """

    @classmethod
    @abc.abstractmethod
    def get_instance(cls, sender: str, receiver: str, message_id: int):
        """
            Returns an instance of the class
        """

    def _prepare_json(self) -> str:
        return str({
            "sender": self.sender,
            "receiver": self.receiver,
            "message_id": self.message_id}).replace("'", "\"")
