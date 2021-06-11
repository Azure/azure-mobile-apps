# This bash script cleans and builds the Microsoft Azure Mobile Services iOS Framework

# Make sure we are executing in this script's directory
cd "$( cd "$( dirname "$0" )" && pwd )"

# Clean by removing the build directory
rm -rf Build
rm MicrosoftAzureMobile.framework.zip
rm -rf MicrosoftAzureMobile.framework

# Generate the build version
VERSION_START_YEAR=2013
DATE_VERSION=$((($(date +%Y) - $VERSION_START_YEAR) + 1))$(date +%m%d)

# Update the MicrosoftAzureMobile.h file with the build version
if [ "$SET_BUILD_VERSION" == "YES" ]; then
  HEADER_FILE=src/MicrosoftAzureMobile.h
  HEADER_BAK_FILE=$HEADER_FILE.bak
  mv $HEADER_FILE $HEADER_BAK_FILE
  more $HEADER_BAK_FILE | sed "s/\(WindowsAzureMobileServicesSdkBuildVersion\) 0/\1 $DATE_VERSION/" > $HEADER_FILE
fi

# Build the framework
xcodebuild OTHER_CFLAGS="-fembed-bitcode" -target Framework OBJROOT=./Build SYMROOT=./Build

# Move back to the original MicrosoftAzureMobile.h file
if [ "$SET_BUILD_VERSION" == "YES" ]; then
  rm $HEADER_FILE
  mv $HEADER_BAK_FILE $HEADER_FILE
  echo "BUILD VERSION SET."
fi

# Copy the framework into this directory and add the license
rsync -rlK ../sdk/Build/Release-iphoneos/MicrosoftAzureMobile.framework .
cp license.rtf MicrosoftAzureMobile.framework

# Zip the framework
zip -r MicrosoftAzureMobile.framework.zip MicrosoftAzureMobile.framework

# Lastly, copy to the build share
if [ "$COPY_TO_SHARE" == "YES" ]; then
  SHARE_PATH_ARRAY=$(echo $BUILD_SHARE_PATHS | tr ";" "\n")
  for SHARE_PATH in $SHARE_PATH_ARRAY
  do
    rsync -rlK MicrosoftAzureMobile.framework.zip $SHARE_PATH
  done
fi
