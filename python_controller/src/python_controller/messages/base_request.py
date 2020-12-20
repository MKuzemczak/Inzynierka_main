import abc
import json

from . import BaseIndication

class BaseRequest(BaseIndication):

    @classmethod
    @abc.abstractmethod
    def get_instance(cls, sender: str, receiver: str, message_id: int, contents: list = []):
        """
            Extracts message contents from the dict and returns an instance of the class
        """

    def _prepare_json(self, contents: dict) -> str:
        return str({
            "sender": self.sender,
            "receiver": self.receiver,
            "message_id": self.message_id,
            "contents": contents}).replace("'", "\"")
