@echo off
set KEYTOOL="C:\Program Files (x86)\Java\jdk1.8.0_101\bin\keytool.exe"
set KEYSTORE="iap.keystore"
set ALIAS="iap"
set PASSWORD="YOUR_KEYSTORE_PASSWORD"

copy /y %KEYSTORE% debug.keystore
%KEYTOOL% -changealias -keystore debug.keystore -alias %ALIAS% -destalias androiddebugkey -storepass %PASSWORD%
%KEYTOOL% -keypasswd -keystore debug.keystore -alias androiddebugkey -new android -storepass %PASSWORD%
%KEYTOOL% -storepasswd -keystore debug.keystore -new android -storepass %PASSWORD%