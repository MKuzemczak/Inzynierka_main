import abc

from enum import Enum

from . import BaseRequest

class ResultMessageStatus(Enum):
    Failed = 0
    Success = 1

class BaseResult(BaseRequest):
    @property
    @abc.abstractmethod
    def status(self):
        """
            Return result message status of enum value: Failed, Success
        """

    def _prepare_json(self, contents: dict) -> str:
        return str({
            "sender": self.sender,
            "receiver": self.receiver,
            "message_id": self.message_id,
            "status": self.status.name,
            "contents": contents}).replace("'", "\"")
