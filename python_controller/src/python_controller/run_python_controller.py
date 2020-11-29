import pathlib

from python_controller.messages import *
from python_controller.structures import BoneSearchResult
from python_controller.services import RabbitMQCommunicationService, XRayImagesScanner

def run_python_controller():
    yolo_files_path = pathlib.Path('D:/Dane/MichalKuzemczak/Projects/Inzynierka_main/data/yolo_files')

    com_service = RabbitMQCommunicationService()
    scanner = XRayImagesScanner(
        com_service,
        str(yolo_files_path.joinpath('yolov3-custom.cfg')),
        str(yolo_files_path.joinpath('yolov3-custom_final.weights')),
        str(yolo_files_path.joinpath('class_names.names')))

    def callback(message: ExitRequest):
        exit()

    com_service.subscribe(ExitRequest, callback)

    def setup_finished_callback():
        com_service.publish(SetupFinishedIndication(com_service.in_queue_name, com_service.launcher_queue_name))

    com_service.start(setup_finished_callback)

if __name__ == '__main__':
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