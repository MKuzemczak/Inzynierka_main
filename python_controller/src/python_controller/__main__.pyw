from .run_python_controller import run_python_controller 

run_python_controller()

# json = "{\"name\":\"FindBonesRequest\",\"contents\":[{}]}"

# test message:
# {"name":"FindBonesRequest","sender":"inzynierka_app","receiver":"inzynierka_python","contents":["D:/Dane/MichalKuzemczak/Projects/Inzynierka_main/data/yolo_files/16.png"]}

# exit message:
# {"name":"ExitRequest","sender":"inzynierka_launcher","receiver":"inzynierka_python","contents":[]}

# print(m)

# d = {"x": 1, "w": 3, "y": 2, "h": 4, "confidence": 5, "detected_class_name": "hehe"}

# b = BoneSearchResult(**d)

# print(b.__dict__)