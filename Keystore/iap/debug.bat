@echo off
set KEYTOOL="C:\Program Files (x86)\Java\jdk1.8.0_101\bin\keytool.exe"

copy /y iap.keystore debug.keystore
%KEYTOOL% -changealias -keystore debug.keystore -alias iap -destalias androiddebugkey
%KEYTOOL% -keypasswd -keystore debug.keystore -alias androiddebugkey -new android
%KEYTOOL% -storepasswd -keystore debug.keystore -new android