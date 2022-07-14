#!/bin/sh
if [ -z "$1" ]
then
	echo "Please pass ip address or hostname of odroid as first argument"
else
	 rsync -avz --progress --partial publish.bz2 install_on_odroid.sh odroid@$1:~/Projects/OdroidPianoteq
fi

