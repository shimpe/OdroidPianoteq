#!/bin/sh
rm ./publish -rf
tar xvjf publish.bz2
rm publish.bz2
sudo ./SystemdServiceCreator $USER $HOME /etc/systemd/system
./UserInterface
echo "Done"

