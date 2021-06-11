#!/bin/bash -x

# Make sure we are executing in this script's directory
cd `dirname $0`
rm -Rf Results
mkdir Results

DIR="$( pwd )"

if [ $# -lt 5 ]
then
#           $0 $1                  $2         $3                        $4           $5
echo Usage: $0 \<Application URL\> \<device\> \<zumotestuser password\> \<Blob URL\> \<Blob Token\>
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
exit 1
fi

echo "$2"
export DEVICE_ARG=
export APP_NAME=
export DEVICE_CMD_ARG=$2

echo Device: $DEVICE_CMD_ARG

#Delete if E2EAPP exists
rm -f ZumoE2ETestApp.app.zip
rm -f -r ZumoE2ETestApp.app

# Copy the E2EAPP from Build share into this directory
rsync -rlK $BUILD_SHARE_PATH/ZumoE2ETestApp.app.zip .

#Unzip
unzip ZumoE2ETestApp.app.zip


if [ "$DEVICE_CMD_ARG" == "iPad2Sim" ]; then
echo Using iPad 2 Simulator
export DEVICE_ARG=iPad\ 2\ \(10.2\)
fi

if [ "$DEVICE_CMD_ARG" == "iPadSimAir" ]; then
echo Using iPad Air Simulator
export DEVICE_ARG=iPad\ Air\ \(10.2\)
fi

if [ "$DEVICE_CMD_ARG" == "iPadSimAir2" ]; then
echo Using iPad Air 2 Simulator
export DEVICE_ARG=iPad\ Air\ 2\ \(10.2\)
fi

if [ "$DEVICE_CMD_ARG" == "iPadSimPro" ]; then
echo Using iPad Pro Simulator
export DEVICE_ARG=iPad\ Pro\ \(10.2\)
fi

if [ "$DEVICE_CMD_ARG" == "iPadSimRetina" ]; then
echo Using iPad Retina Simulator
export DEVICE_ARG=iPad\ Retina\ \(10.2\)
fi

if [ "$DEVICE_CMD_ARG" == "iPhoneSim4s" ]; then
echo Using iPhone 4s Simulator
export DEVICE_ARG=iPhone\ 4s\ \(10.2\)
fi

if [ "$DEVICE_CMD_ARG" == "iPhoneSim5" ]; then
echo Using iPhone 5 Simulator
export DEVICE_ARG=iPhone\ 5\ \(10.2\)
fi

if [ "$DEVICE_CMD_ARG" == "iPhoneSim5s" ]; then
echo Using iPhone 5s Simulator
export DEVICE_ARG=iPhone\ 5s\ \(10.2\)
fi

if [ "$DEVICE_CMD_ARG" == "iPhoneSim6" ]; then
echo Using iPhone 6 Simulator
export DEVICE_ARG=iPhone\ 6\ \(10.2\)
fi

if [ "$DEVICE_CMD_ARG" == "iPhoneSim6Plus" ]; then
echo Using iPhone 6 Plus Simulator
export DEVICE_ARG=iPhone\ 6\ Plus\ \(10.2\)
fi

if [ "$DEVICE_CMD_ARG" == "iPhoneSim6s" ]; then
echo Using iPhone 6s Simulator
export DEVICE_ARG=iPhone\ 6s\ \(10.2\)
fi

if [ "$DEVICE_CMD_ARG" == "iPhoneSim6sWatch" ]; then
echo Using iPhone 6s Simulator + Apple Watch
export DEVICE_ARG=iPhone\ 6s\ \(10.2\)\ +\ Apple\ Watch\ -\ 38mm\ \(2.0\)
fi

if [ "$DEVICE_CMD_ARG" == "iPhoneSim6sPlus" ]; then
echo Using iPhone 6s Plus Simulator
export DEVICE_ARG=iPhone\ 6s\ \(10.2\)
fi

if [ "$DEVICE_CMD_ARG" == "iPhoneSim6sPlusWatch" ]; then
echo Using iPhone 6s Plus Simulator + Apple Watch
export DEVICE_ARG=iPhone\ 6s\ Plus\ \(10.2\)\ +\ Apple\ Watch\ -\ 42mm\ \(2.0\)
fi

if [ "$DEVICE_CMD_ARG" == "zumoipad" ]; then
echo Using Zumo IPad
export DEVICE_ARG=Zumo\ Test\ Team\ iPad\ 1\ \(10.2\)
fi

if [ "$DEVICE_CMD_ARG" == "zumoipadair" ]; then
echo Using Zumo IPad
export DEVICE_ARG=zumoipadair\ \(10.2\)
fi

if [ "$DEVICE_ARG" == "" ]
then
echo Unsupported device: "$2"
exit 1
fi

APP_NAME=ZumoE2ETestApp.app

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
instruments -w "$DEVICE_ARG" -t "$INSTRUMENT_TEMPLATE" "$APP_NAME" -e UIASCRIPT "ZumoAutomationWithData.js" -e UIARESULTSPATH "Results" || exit $?

exit $?
