#!/bin/bash

# Change these variables for your keystore
KEYSTORE="iap.keystore"
ALIAS="iap"
PASSWORD="YOUR_KEYSTORE_PASSWORD"

# These are variables that Xamarin.Android expects
OUT_KEYSTORE="debug.keystore"
OUT_ALIAS="androiddebugkey"
OUT_PASSWORD="android"

cp -f $KEYSTORE $OUT_KEYSTORE
keytool -changealias -keystore $OUT_KEYSTORE -alias $ALIAS -destalias $OUT_ALIAS -storepass $PASSWORD
keytool -keypasswd -keystore $OUT_KEYSTORE -alias $OUT_ALIAS -new $OUT_PASSWORD -storepass $PASSWORD
keytool -storepasswd -keystore $OUT_KEYSTORE -new $OUT_PASSWORD -storepass $PASSWORD