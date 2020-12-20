from . import BaseIndication

class SetupFinishedIndication(BaseIndication):

    def __init__(self, sender: str, receiver: str, message_id: int = 0):
        self._sender = sender
        self._receiver = receiver
        self._message_id = message_id

    @property
    def sender(self) -> str:
        return self._sender
    
    @property
    def receiver(self) -> str:
        return self._receiver

    @property
    def message_id(self) -> int:
        return self._message_id

    def to_json(self):
        return self._prepare_json()

    @classmethod
    def get_instance(cls, sender: str, receiver: str, message_id: int):
        return SetupFinishedIndication(sender, receiver, message_id)
