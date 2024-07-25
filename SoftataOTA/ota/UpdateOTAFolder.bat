@echo off 
cls
@echo Update sketch export built files in ota folder from main
@echo Expecting IDE->Sketch->Export Compiled Binary to have been run.
@echo Note: Copy uf2conv.py and espota.py copied from tools:
@echo eg. C:\Users\%USER%\AppData\Local\arduino15\packages\rp2040\hardware\rp2040\3.9.3\tools
@echo ===================================================

set sketchName=SoftataOTA
set sketchFolderLocation=C:\Users\%USERNAME%\source\Softata
set builtfolder=%sketchFolderLocation%\%sketchName%\build\rp2040.rp2040.rpipicow
set targetFolder=%sketchFolderLocation%\%sketchName%\ota

@echo Sketch:          %sketchName%
@echo Sketch Folder:   %sketchFolderLocation%\%sketchName%
@echo SourceFolder     %builtfolder%
@echo TargetFolder	   %targetFolder%
@echo ===================================================



@echo copy %builtfolder%\*.* %targetFolder% /Y
dir %targetFolder%
pause

