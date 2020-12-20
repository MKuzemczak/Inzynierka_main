import json

from . import ExitRequest, FindBonesRequest, FindBonesRequestResult, SetupFinishedIndication


message_name_to_cls = {
    ExitRequest.__name__: ExitRequest,
    FindBonesRequest.__name__: FindBonesRequest,
    FindBonesRequestResult.__name__: FindBonesRequestResult,
    SetupFinishedIndication.__name__: SetupFinishedIndication
}


def get_message_from_json(message_class_name: str, message_body_json_str: str):
    """
        Parses the json and returns an instance of an appropriate class
    """
    if not message_class_name or message_class_name not in message_name_to_cls:
        print("ERROR: get_message_from_json: invalid message_class_name")

    data = json.loads(message_body_json_str)
    # sender = data.get("sender")
    # receiver = data.get("receiver")
    # message_id = data.get("message_id")

    # if not (sender is not None
    #         and receiver is not None
    #         and message_id is not None):
    #     print("ERROR: get_message_from_json: Invalid message structure")
    #     return None

    return message_name_to_cls[message_class_name](**data)
