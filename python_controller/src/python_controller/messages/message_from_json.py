import json

from . import ExitRequest, FindBonesRequest, FindBonesRequestResult

message_name_to_cls = {
    ExitRequest.__name__: ExitRequest,
    FindBonesRequest.__name__: FindBonesRequest,
    FindBonesRequestResult.__name__: FindBonesRequestResult
}


def get_message_from_json(json_str: str):
    """
        Parses the json and returns an instance of an appropriate class
    """
    data = json.loads(json_str)
    name = data.get("name")
    contents = data.get("contents")

    if not (name is not None
            and name in message_name_to_cls
            and contents is not None):
        return None

    return message_name_to_cls[name].get_instance_from_message_contents(contents)