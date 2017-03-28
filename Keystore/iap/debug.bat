@echo off

rem Change these variables for your keystore
set KEYTOOL="C:\Program Files (x86)\Java\jdk1.8.0_101\bin\keytool.exe"
set KEYSTORE="iap.keystore"
set ALIAS="iap"
set PASSWORD="YOUR_KEYSTORE_PASSWORD"

rem These are variables that Xamarin.Android expects
set OUT_KEYSTORE="debug.keystore"
set OUT_ALIAS="androiddebugkey"
set OUT_PASSWORD="android"

copy /y %KEYSTORE% %OUT_KEYSTORE%
%KEYTOOL% -changealias -keystore %OUT_KEYSTORE% -alias %ALIAS% -destalias %OUT_ALIAS% -storepass %PASSWORD%
%KEYTOOL% -keypasswd -keystore %OUT_KEYSTORE% -alias %OUT_ALIAS% -new %OUT_PASSWORD% -storepass %PASSWORD%
%KEYTOOL% -storepasswd -keystore %OUT_KEYSTORE% -new %OUT_PASSWORD% -storepass %PASSWORD%