#!/usr/bin/env bash

dotnet build --configuration Release --no-restore --self-contained false /p:PublishSingleFile=true /p:PublishTrimmed=true

cd ./bin/Release/net9.0 && zip -r ../../../webdev-tool.zip ./* && cd -