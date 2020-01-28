@echo off
echo Preparing for packaging...
mkdir NodeNetwork-Examples
del examples.zip
echo Copying files...
cd NodeNetwork-Examples
xcopy /Y "..\..\ExampleCalculatorApp\bin\Release\netcoreapp3.1\*.exe" "."
xcopy /Y "..\..\ExampleCalculatorApp\bin\Release\netcoreapp3.1\*.json" "."
xcopy /Y "..\..\ExampleCalculatorApp\bin\Release\netcoreapp3.1\*.exe.config" "."
xcopy /Y "..\..\ExampleCalculatorApp\bin\Release\netcoreapp3.1\*.dll" "."
xcopy /Y "..\..\ExampleShaderEditorApp\bin\Release\netcoreapp3.1\*.exe" "."
xcopy /Y "..\..\ExampleShaderEditorApp\bin\Release\netcoreapp3.1\*.json" "."
xcopy /Y "..\..\ExampleShaderEditorApp\bin\Release\netcoreapp3.1\*.exe.config" "."
xcopy /Y "..\..\ExampleShaderEditorApp\bin\Release\netcoreapp3.1\*.dll" "."
xcopy /Y "..\..\ExampleCodeGenApp\bin\Release\netcoreapp3.1\*.exe" "."
xcopy /Y "..\..\ExampleCodeGenApp\bin\Release\netcoreapp3.1\*.json" "."
xcopy /Y "..\..\ExampleCodeGenApp\bin\Release\netcoreapp3.1\*.exe.config" "."
xcopy /Y "..\..\ExampleCodeGenApp\bin\Release\netcoreapp3.1\*.dll" "."
cd ..
echo Packaging examples...
.\7zip\7z.exe a -tzip examples.zip NodeNetwork-Examples
echo Cleaning up...
rmdir NodeNetwork-Examples /s /q