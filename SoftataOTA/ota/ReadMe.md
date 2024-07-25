## OTA Folder

- Can upload OTA directly using batch file without Arduino IDE _(2 options)_
- Can upload using USB Serial directly using batch file without IDE
- Can copy the built files to the OTA folder using batch file without IDE

Several batch files have been added to the repository at ```<Repository>\SoftataOTA\ota```. 
- [OTA-generic.bat](https://github.com/djaus2/Soft-ata/blob/master/SoftataOTA/ota/OTA-generic.bat)   _... view on GitHub._
  - This runs using an explicit path to python3.exe such as:
  ```C:\Users\user\AppData\Local\arduino15\packages\rp2040\tools\pqt-python3\1.0.1-base-3a57aed```
    - The path used here actually as determined by ```where python3``` is  
    ```C:\Users\%USERNAME%\AppData\Local\Microsoft\WindowsApps```
  - An explicit path to the bin namely  ```<Repository>\SoftataOTA\build\rp2040.rp2040.rpipicow```).
  - An explicit path to ```espota.py```, such as ```C:\Users\%USERNAME%\AppData\Local\arduino15\packages\rp2040\hardware\rp2040\3.9.3\tools```
- [OTA-pythoninpath.bat](https://github.com/djaus2/Soft-ata/blob/master/SoftataOTA/ota/OTA-pythoninpath.bat)  _... view on GitHub._
  - This assumes that the system ```Path``` includes ```python3.exe```
  - It also requires ```espota.py``` has been copied to the ota folder.
  - And the bin file needs to be copied to here as well.
- [USBSerial-pythoninpath.bat]https://github.com/djaus2/Soft-ata/tree/master/SoftataOTA/ota/(USBSerial-pythoninpath.bat)  _... view on GitHub._
  - This batch file is used to upload using USB Serial.
  - It assumes that the system ```Path``` includes ```python3.exe```
  - Expects ```u2conv.py``` has been copied to the ota folder from tools.
  -  Also expects ```pyserial`` folder has been copied from tools.
  - eg tools folder: ```C:\Users\%USERNAME%\AppData\Local\arduino15\packages\rp2040\tools```
  - And the uf2 file needs to be copied to in ota as well. See next batch file:
- [UpdateOTAFolder.bat](https://github.com/djaus2/Soft-ata/tree/master/SoftataOTA/ota/UpdateOTAFolder.bat)  _... view on GitHub._
  - This batch file copies the IDE->Sketch->Export Compiled Library built files to the OTA folder.
  - ... From <Sketch folder>\build\rp2040.rp2040.rpipicow

> Nb: The Repository\SoftataOTA\ota folder and contents have been included in the repository and  
so can be used directly without the Arduino IDE. In that case ignore the second batch file.