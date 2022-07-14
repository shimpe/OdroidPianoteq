#!/bin/sh
if [ -z "$1" ]
then
        echo "Please pass ip address or hostname of odroid as first argument"
else
	echo $1:5901
	vncviewer $1:5901
fi

