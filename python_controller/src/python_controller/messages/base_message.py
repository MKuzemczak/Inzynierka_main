import abc
import json

class BaseMessage(abc.ABC):

    @abc.abstractmethod
    def to_json(self) -> str:
        """
            Returns a json representation of the message
        """

    @classmethod
    @abc.abstractmethod
    def get_instance_from_message_contents(cls, contents: list = []):
        """
            Extracts message contents from the dict and returns an instance of the class
        """

    def _prepare_json(self, contents: dict) -> str:
        return json.dumps({"name": self.__class__.__name__, "contents": contents})
