import abc

from enum import Enum

from . import BaseMessage

class ResultMessageStatus(Enum):
    Failed = 0
    Success = 0

class BaseResult(BaseMessage):
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
            "request_id": self.request_id,
            "status": self.status.name,
            "contents": contents}).replace("'", "\"")
