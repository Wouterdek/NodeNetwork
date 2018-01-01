@echo off
echo Preparing for packaging...
mkdir NodeNetwork-Examples
del examples.zip
echo Copying files...
cd NodeNetwork-Examples
xcopy /Y "..\..\ExampleCalculatorApp\bin\Release\*.exe" "."
xcopy /Y "..\..\ExampleCalculatorApp\bin\Release\*.exe.config" "."
xcopy /Y "..\..\ExampleCalculatorApp\bin\Release\*.dll" "."
xcopy /Y "..\..\ExampleShaderEditorApp\bin\Release\*.exe" "."
xcopy /Y "..\..\ExampleShaderEditorApp\bin\Release\*.exe.config" "."
xcopy /Y "..\..\ExampleShaderEditorApp\bin\Release\*.dll" "."
cd ..
echo Packaging examples...
.\7zip\7z.exe a -tzip examples.zip NodeNetwork-Examples
echo Cleaning up...
rmdir NodeNetwork-Examples /s /q