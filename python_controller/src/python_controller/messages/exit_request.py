from . import BaseMessage

class ExitRequest(BaseMessage):

    def __init__(self):
        pass
    
    def to_json(self):
        return self._prepare_json([])

    @classmethod
    def get_instance_from_message_contents(cls, contents: list = []):
        return ExitRequest()
