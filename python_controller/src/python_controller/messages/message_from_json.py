import json

from . import ExitRequest, FindBonesRequest, FindBonesRequestResult, SetupFinishedIndication


message_name_to_cls = {
    ExitRequest.__name__: ExitRequest,
    FindBonesRequest.__name__: FindBonesRequest,
    FindBonesRequestResult.__name__: FindBonesRequestResult,
    SetupFinishedIndication.__name__: SetupFinishedIndication
}


def get_message_from_json(json_str: str):
    """
        Parses the json and returns an instance of an appropriate class
    """
    data = json.loads(json_str)
    name = data.get("name")
    sender = data.get("sender")
    receiver = data.get("receiver")
    request_id = data.get("request_id")
    contents = data.get("contents")

    if not (name is not None
            and name in message_name_to_cls
            and sender is not None
            and receiver is not None
            and request_id is not None
            and contents is not None):
        print("Error: Invalid message structure")
        return None

    return message_name_to_cls[name].get_instance(sender, receiver, request_id, contents)
