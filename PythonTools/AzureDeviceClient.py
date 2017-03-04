import hmac
import base64
import urllib
import requests
import time
import hashlib

class AzureDeviceClient():
    """ 
        Client for Azure IoT Hub REST API
        Based on https://github.com/bechynsky/AzureIoTDeviceClientPY
        Tested with Python 2.7.*
        https://msdn.microsoft.com/en-us/library/mt590785.aspx
    """
    _API_VERSION = 'api-version=2015-08-15-preview'
    _HEADER_AUTHORIZATION = 'Authorization'

    """
        iot_hub_name - name of your Azure IoT Hub
        device_name - name of your device
        key - security key for your device
    """
    def __init__(self, iot_hub_name, device_name, key):
        self._iot_hub_name = iot_hub_name.lower()
        self._device_name = device_name.lower()
        self._key = key

        self._base_url = 'https://' + \
                        self._iot_hub_name + \
                        '.azure-devices.net/devices/' + \
                        self._device_name + \
                        '/messages/'

        self._url_to_sign = self._iot_hub_name + \
                        '.azure-devices.net/devices/' + \
                        self._device_name

    """
        Creates Shared Access Signature. Run before another funstions
        timeout - expiration in seconds
    """
    def create_sas(self, timeout):
        urlToSign = urllib.pathname2url(self._url_to_sign) 
        
        # current time +10 minutes
        timestamp = int(time.time()) + timeout

        h = hmac.new(base64.b64decode(self._key), 
                    msg = "{0}\n{1}".format(urlToSign, timestamp).encode('utf-8'),
                    digestmod=hashlib.sha256)

        self._sas = "SharedAccessSignature sr={0}&sig={1}&se={2}".format(urlToSign, 
                    urllib.pathname2url(base64.b64encode(h.digest())),
                    timestamp)

        return self._sas

    """
        Sends message
        message - message to be send

        Returns HTTP response code. 204 is OK.
        https://msdn.microsoft.com/en-us/library/mt590784.aspx
    """
    def send(self, message):
        headers = {
            self._HEADER_AUTHORIZATION : self._sas,
            'Content-Type' : 'application/json'
            }

        resp = requests.post(self._base_url + 'events?' + self._API_VERSION, headers=headers, data=message)
        return resp.status_code

    """
        Reads first message in queue

        Returns:
        message['headers'] - all response headers
        message['etag'] - message id, you need this for complete, reject and abandon
        message['body'] - message content
        message['response_code'] - HTTP response code

        https://msdn.microsoft.com/en-us/library/mt590786.aspx
    """

    def read_message(self):
        headers = {
            self._HEADER_AUTHORIZATION : self._sas,
            }

        resp = requests.get(self._base_url + 'devicebound?' + self._API_VERSION, headers=headers)

        message = {
            "headers": resp.headers,
            "body": resp.text.decode('utf-8'),
            "response_code": resp.status_code
        }

        etag = message["headers"].get("ETag", None)
        if etag != None:
            message["etag"] = etag.strip('"')

        return message

    """
        Completes a cloud-to-device message.
        id - use message['etag'] from read_message function
        
        Returns HTTP response code. 204 is OK.
        https://msdn.microsoft.com/en-us/library/mt605155.aspx
    """
    def complete_message(self, id):
        headers = {
            self._HEADER_AUTHORIZATION : self._sas,
            }

        resp = requests.delete(self._base_url + 'devicebound/' + id + '?' + self._API_VERSION, headers=headers)
        return resp.status_code
    
    """
        Completes a cloud-to-device message.
        id - use message['etag'] from read_message function
        
        Returns HTTP response code. 204 is OK.

        https://msdn.microsoft.com/en-us/library/mt590787.aspx
    """
    def reject_message(self, id):
        headers = {
                        self._HEADER_AUTHORIZATION : self._sas,
                    }

        resp = requests.delete(self._base_url + 'devicebound/' + id + '?reject&' + self._API_VERSION, headers=headers)
        return resp.status_code

    """
        Abandon a cloud-to-device message
        id - use message['etag'] from read_message function
        
        Returns HTTP response code. 204 is OK.

        https://msdn.microsoft.com/en-us/library/mt590788.aspx
    """
    def abandon_message(self, id):
        headers = {
                        self._HEADER_AUTHORIZATION : self._sas,
                    }

        resp = requests.post(self._base_url + 'devicebound/' + id + '/abandon?' + self._API_VERSION, headers=headers, data="")
        return resp.status_code
