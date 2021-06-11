#!/bin/sh

cd `dirname $0`
echo $1
export DEVICE_ARG=
export DEVICE_CMD_ARG=$1

killall "Simulator"

sleep 5

echo "Waiting to kill simulator..."

# Get the sim list with the UUIDs
OUTPUT="$(xcrun simctl list)"
# Parse out the UUIDs and saves them to file
echo $OUTPUT | awk -F "[()]" '{ for (i=2; i<NF; i+=2) print $i }' | grep '^[-A-Z0-9]*$' > output.txt
# Iterate through file and reset sim
for UUID in `awk '{ print $1 }' output.txt`
do
xcrun simctl erase $UUID
done
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
if [ "$DEVICE_ARG" == "" ]
then
echo Unsupported device: "$0"
exit 1
fi
xcrun instruments -w "$DEVICE_ARG"

echo "waiting for simulator...."
sleep 60
