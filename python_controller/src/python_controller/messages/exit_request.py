from . import BaseMessage

class ExitRequest(BaseMessage):

    def __init__(self, sender: str, receiver: str):
        self._sender = sender
        self._receiver = receiver

    @property
    def sender(self) -> str:
        return self._sender
    
    @property
    def receiver(self) -> str:
        return self._receiver

    def to_json(self):
        return self._prepare_json([])

    @classmethod
    def get_instance(cls, sender: str, receiver: str, contents: list = []):
        return ExitRequest(sender, receiver)
