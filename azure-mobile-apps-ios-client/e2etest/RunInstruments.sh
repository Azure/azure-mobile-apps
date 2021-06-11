#!/bin/bash -x

# Make sure we are executing in this script's directory
cd "$( cd "$( dirname "$0" )" && pwd )"
rm -R Results
mkdir Results

DIR="$( pwd )"

if [ $# -lt 5 ]
then
  #           $0 $1                  $2         $3                        $4           $5             $6 (optional)
  echo Usage: $0 \<Application URL\> \<device\> \<zumotestuser password\> \<Blob URL\> \<Blob Token\> \<iOSsdkZip\>
  echo Where
  echo   \<Application URL\> - the URL of the Mobile Service
  echo   \<device\> is one of the following:
  echo       - iPad2Sim             - iPadSimAir          - iPadSimAir2
  echo       - iPadSimPro           - iPadSimRetina       - iPhoneSim4s
  echo       - iPhoneSim5           - iPhoneSim5s         - iPhoneSim6
  echo       - iPhoneSim6Plus       - iPhoneSim6s         - iPhoneSim6sWatch
  echo       - iPhone6sPlus         - iPhone6sPlusWatch
  echo   \<zumotestuser password\> - the password to use for log in operations \(for zumotestuser account\)
  echo   \<Blob URL\> - storage url
  echo   \<Blob Token\> - storage token
  echo   \<iOSsdkZip\> - the zip file location of the framework to test against \(optional\)
  exit 1
fi

echo "$2"
export DEVICE_ARG=
export APP_NAME=
export DEVICE_CMD_ARG=$2

echo Device: $DEVICE_CMD_ARG

# Build current app to test with
pushd e2etest

rm -Rf $DIR/ZumoE2ETestApp/MicrosoftAzureMobile.framework
if [ $6 ]
then
  rm -f sdk.zip
  # Copy specified framework
  curl --location --output sdk.zip $6
  unzip -o sdk.zip
else
  # Copy in current version of the framework
  bash $DIR/sdk/build.command
  cp -R ../sdk/MicrosoftAzureMobile.framework .
fi

xcodebuild -sdk iphonesimulator10.2 || exit 1

popd

if [ "$DEVICE_CMD_ARG" == "iPad2Sim" ]; then
  echo Using iPad 2 Simulator
  export DEVICE_ARG=iPad\ 2\ \(10.2\)
  APP_NAME=$DIR/e2etest/build/Release-iphonesimulator/ZumoE2ETestApp.app
fi

if [ "$DEVICE_CMD_ARG" == "iPadSimAir" ]; then
  echo Using iPad Air Simulator
  export DEVICE_ARG=iPad\ Air\ \(10.2\)
  APP_NAME=$DIR/e2etest/build/Release-iphonesimulator/ZumoE2ETestApp.app
fi

if [ "$DEVICE_CMD_ARG" == "iPadSimAir2" ]; then
  echo Using iPad Air 2 Simulator
  export DEVICE_ARG=iPad\ Air\ 2\ \(10.2\)
  APP_NAME=$DIR/e2etest/build/Release-iphonesimulator/ZumoE2ETestApp.app
fi

if [ "$DEVICE_CMD_ARG" == "iPadSimPro" ]; then
  echo Using iPad Pro Simulator
  export DEVICE_ARG=iPad\ Pro\ \(10.2\)
  APP_NAME=$DIR/e2etest/build/Release-iphonesimulator/ZumoE2ETestApp.app
fi

if [ "$DEVICE_CMD_ARG" == "iPadSimRetina" ]; then
  echo Using iPad Retina Simulator
  export DEVICE_ARG=iPad\ Retina\ \(10.2\)
  APP_NAME=$DIR/ZumoE2ETestApp/build/Release-iphonesimulator/ZumoE2ETestApp.app
fi

if [ "$DEVICE_CMD_ARG" == "iPhoneSim4s" ]; then
  echo Using iPhone 4s Simulator
  export DEVICE_ARG=iPhone\ 4s\ \(10.2\)
  APP_NAME=$DIR/ZumoE2ETestApp/build/Release-iphonesimulator/ZumoE2ETestApp.app
fi

if [ "$DEVICE_CMD_ARG" == "iPhoneSim5" ]; then
  echo Using iPhone 5 Simulator
  export DEVICE_ARG=iPhone\ 5\ \(10.2\)
  APP_NAME=$DIR/e2etest/build/Release-iphonesimulator/ZumoE2ETestApp.app
fi

if [ "$DEVICE_CMD_ARG" == "iPhoneSim5s" ]; then
  echo Using iPhone 5s Simulator
  export DEVICE_ARG=iPhone\ 5s\ \(10.2\)
  APP_NAME=$DIR/e2etest/build/Release-iphonesimulator/ZumoE2ETestApp.app
fi

if [ "$DEVICE_CMD_ARG" == "iPhoneSim6" ]; then
  echo Using iPhone 6 Simulator
  export DEVICE_ARG=iPhone\ 6\ \(10.2\)
  APP_NAME=$DIR/e2etest/build/Release-iphonesimulator/ZumoE2ETestApp.app
fi

if [ "$DEVICE_CMD_ARG" == "iPhoneSim6Plus" ]; then
  echo Using iPhone 6 Plus Simulator
  export DEVICE_ARG=iPhone\ 6\ Plus\ \(10.2\)
  APP_NAME=$DIR/e2etest/build/Release-iphonesimulator/ZumoE2ETestApp.app
fi

if [ "$DEVICE_CMD_ARG" == "iPhoneSim6s" ]; then
  echo Using iPhone 6s Simulator
  export DEVICE_ARG=iPhone\ 6s\ \(10.2\)
  APP_NAME=$DIR/ZumoE2ETestApp/build/Release-iphonesimulator/ZumoE2ETestApp.app
fi

if [ "$DEVICE_CMD_ARG" == "iPhoneSim6sWatch" ]; then
  echo Using iPhone 6s Simulator + Apple Watch
  export DEVICE_ARG=iPhone\ 6s\ \(10.2\)\ +\ Apple\ Watch\ -\ 38mm\ \(2.0\)
  APP_NAME=$DIR/ZumoE2ETestApp/build/Release-iphonesimulator/ZumoE2ETestApp.app
fi

if [ "$DEVICE_CMD_ARG" == "iPhoneSim6sPlus" ]; then
  echo Using iPhone 6s Plus Simulator
  export DEVICE_ARG=iPhone\ 6s\ \(10.2\)
  APP_NAME=$DIR/e2etest/build/Release-iphonesimulator/ZumoE2ETestApp.app
fi

if [ "$DEVICE_CMD_ARG" == "iPhoneSim6sPlusWatch" ]; then
  echo Using iPhone 6s Plus Simulator + Apple Watch
  export DEVICE_ARG=iPhone\ 6s\ Plus\ \(10.2\)\ +\ Apple\ Watch\ -\ 42mm\ \(2.0\)
  APP_NAME=$DIR/e2etest/build/Release-iphonesimulator/ZumoE2ETestApp.app
fi

if [ "$APP_NAME" == "" ]
then
  echo Unsupported device: "$2"
  exit 1
fi

echo DEVICE_ARG: $DEVICE_ARG
echo APP_NAME: $APP_NAME
EscapedToken=${5//&/\\&}

sed -e "s|--APPLICATION_URL--|$1|g" ZumoAutomationTemplate.js > ZumoAutomationWithData.js
sed -e "s|--BLOB_URL--|$4|g" -i "" ZumoAutomationWithData.js
sed -e "s|--BLOB_TOKEN--|$EscapedToken|g" -i "" ZumoAutomationWithData.js
sed -e "s|--AUTH_PASSWORD--|$3|g" -i "" ZumoAutomationWithData.js

echo Replaced data on template - now running instruments
echo Args: DEVICE_ARG = $DEVICE_ARG
echo APP_NAME = $APP_NAME

export INSTRUMENT_TEMPLATE=/Applications/Xcode.app/Contents/Applications/Instruments.app/Contents/PlugIns/AutomationInstrument.xrplugin/Contents/Resources/Automation.tracetemplate

echo Running instruments...
instruments -w "$DEVICE_ARG" -t "$INSTRUMENT_TEMPLATE" "$APP_NAME" -e UIASCRIPT "ZumoAutomationWithData.js" -e UIARESULTSPATH "Results" || exit 1

exit 0
