from .run_python_controller import run_python_controller 
# from .structures import BoneSearchResult, ImageBoneSearchResults
# from .messages import FindBonesRequestResult

# r = ImageBoneSearchResults("path", [BoneSearchResult(1, 2, 3, 4, 5, "name1"), BoneSearchResult(1, 2, 3, 4, 5, "name2")])
# m = FindBonesRequestResult("hehe", "hoho", [r, r])

# print(m.to_json())

run_python_controller()

# json = "{\"name\":\"FindBonesRequest\",\"contents\":[{}]}"

# test message:
# {"class_name":"FindBonesRequest", "body": "{\"sender\":\"inzynierka_app\",\"receiver\":\"inzynierka_python\",\"request_id\":1111,\"contents\":[\"D:/Dane/MichalKuzemczak/Projects/Inzynierka_main/data/yolo_files/16.png\"]}"}

# exit message:
# {"name":"ExitRequest","sender":"inzynierka_launcher","receiver":"inzynierka_python","contents":[]}

# print(m)

# d = {"x": 1, "w": 3, "y": 2, "h": 4, "confidence": 5, "detected_class_name": "hehe"}

# b = BoneSearchResult(**d)

# print(b.__dict__)