import datetime
import time
import json
import AzureDeviceClient


DEVICE_KEY = "DEVICE_KEY";
IOT_HUB_NAME = "IOT_HUB_NAME";
DEVICE_NAME = "DEVICE_NAME";

device = AzureDeviceClient.AzureDeviceClient(IOT_HUB_NAME, DEVICE_NAME, DEVICE_KEY)

device.create_sas(600)

dt = datetime.datetime.now().isoformat()
payload = {
    "sensorId": "home",
    "temperature": 21,
    "humidity": 30,
    "date": dt
}

# Device to Cloud
print(device.send(json.dumps(payload)))

# Cloud to Device
message = device.read_message()
print(message['body'])

device.complete_message(message['etag'])