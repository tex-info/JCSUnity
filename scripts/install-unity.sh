#! /bin/sh

# See $BASE_URL/$HASH/unity-$VERSION-$PLATFORM.ini for complete list
# of available packages, where PLATFORM is `osx` or `win`
BASE_URL=https://download.unity3d.com/download_unity/0a46ddfcfad4
VERSION=2018.2.12f1
UNITY_DOWNLOAD_CACHE="$(pwd)/unity_download_cache"

UNITY_OSX_PACKAGE="MacEditorInstaller/Unity-$VERSION.pkg"
UNITY_WINDOWS_TARGET_PACKAGE="MacEditorTargetInstaller/UnitySetup-Windows-Mono-Support-for-Editor-$VERSION.pkg"


# Downloads a file if it does not exist
download() {
	
	FILE=$1
	URL="$BASE_URL/$FILE"

	#download package if it does not already exist in cache
	if [ ! -e $UNITY_DOWNLOAD_CACHE/`basename "$FILE"` ] ; then
		echo "$FILE does not exist. Downloading from $URL: "
		curl -o $UNITY_DOWNLOAD_CACHE/`basename "$FILE"` "$URL"
	else
		echo "$FILE Exists. Skipping download."
	fi
}

install() {
	PACKAGE=$1
	download "$PACKAGE"
	
	echo "Installing "`basename "$PACKAGE"`
	sudo installer -dumplog -package $UNITY_DOWNLOAD_CACHE/`basename "$PACKAGE"` -target /
}



echo "Contents of Unity Download Cache:"
ls "$UNITY_DOWNLOAD_CACHE"

echo "Installing Unity..."
install "$UNITY_OSX_PACKAGE"
install "$UNITY_WINDOWS_TARGET_PACKAGE"
