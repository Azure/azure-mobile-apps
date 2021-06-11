#!/bin/bash -x

# Make sure we are executing in this script's directory
cd "$( cd "$( dirname "$0" )" && pwd )"
rm -R Results
mkdir Results

DIR="$( pwd )"

#           $0  $1(optional)
echo Usage: $0 \<iOSsdkZip\>
echo Where
echo   \<iOSsdkZip\> - the zip file location of the framework to test against \(optional\)

rm -Rf $DIR/ZumoE2ETestApp/MicrosoftAzureMobile.framework
if [ $1 ]
then
	rm -f sdk.zip
	# Copy specified framework
	echo using framework $1
	rsync -rlK $1 .
	unzip -o MicrosoftAzureMobile.framework.zip
else
	# Copy in current version of the framework
	echo building sdk to use with E2E app
	bash ../sdk/build.command
	cp -R ../sdk/MicrosoftAzureMobile.framework .
fi

xcodebuild -sdk iphonesimulator10.2 || exit 1

# Copy the E2EAPP into this directory
rsync -rlK build/Release-iphonesimulator/ZumoE2ETestApp.app .


# Zip the E2EAPP
zip -r ZumoE2ETestApp.app.zip ZumoE2ETestApp.app

# Lastly, copy to the build share
if [ "$COPY_TO_SHARE" == "YES" ]; then
	SHARE_PATH_ARRAY=$(echo $BUILD_SHARE_PATHS | tr ";" "\n")
	for SHARE_PATH in $SHARE_PATH_ARRAY
		do
			echo "Copying E2EAPP to " $SHARE_PATH
			mkdir -p $SHARE_PATH
			rsync -arlK ZumoE2ETestApp.app.zip $SHARE_PATH
		done
fi

