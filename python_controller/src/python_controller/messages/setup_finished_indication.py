from . import BaseMessage

class SetupFinishedIndication(BaseMessage):

    def __init__(self, sender: str, receiver: str, request_id: int = 0):
        self._sender = sender
        self._receiver = receiver
        self._request_id = request_id

    @property
    def sender(self) -> str:
        return self._sender
    
    @property
    def receiver(self) -> str:
        return self._receiver

    @property
    def request_id(self) -> int:
        return self._request_id

    def to_json(self):
        return self._prepare_json([])

    @classmethod
    def get_instance(cls, sender: str, receiver: str, request_id: int, contents: list = []):
        return SetupFinishedIndication(sender, receiver, request_id)
