#!/bin/sh
rm ./publish -rf
rm ./pubish.bz2
dotnet publish -r linux-arm64 -c Release --self-contained true -p:PublishTrimmed=true -p:PublishSingleFile=true -o ./publish
tar cvjf publish.bz2 ./publish

