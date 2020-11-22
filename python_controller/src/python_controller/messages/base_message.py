import abc
import json

class BaseMessage(abc.ABC):
    @abc.abstractmethod
    def to_json(self) -> str:
        """
            Returns a json representation of the message
        """
    
    @property
    @abc.abstractmethod
    def name(self):
        """
            Return name of the message used as its identifier
        """

    @classmethod
    @abc.abstractmethod
    def get_instance_from_message_contents(cls, contents: list):
        """
            Extracts message contents from the dict and returns an instance of the class
        """

    @classmethod
    def from_json(cls, json_str: str):
        """
            Parses the json and returns an instance of the class
        """

        data = json.loads(json_str)
        name_field = data.get("name")
        contents_field = data.get("contents")

        if not (name_field is not None
                and name_field == cls._name
                and contents_field is not None):
            return None

        return cls.get_instance_from_message_contents(contents_field)

    def _prepare_json(self, contents: dict) -> str:
        return json.dumps({"name": self.name, "contents": contents})
