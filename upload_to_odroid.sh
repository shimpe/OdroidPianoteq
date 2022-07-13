#!/bin/sh
if [ -z "$1" ]
then
	echo "Please pass ip address or hostname of odroid as first argument"
else
	scp publish.bz2 odroid@$1:/home/odroid/Projects/OdroidPianoteq
	scp unpack_on_odroid.sh odroid@$1:/home/odroid/Projects/OdroidPianoteq
fi

