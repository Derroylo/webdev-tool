#!/usr/bin/env bash

# This file is the entrypoint for the tool and is required for the self-update

# Get the location of this script
SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")

# Directory where webdev is located
WEBDEVDIR=$SCRIPTPATH

# run the application and pass all arguments to it
dotnet "$WEBDEVDIR/webdev-tool.dll" "$@"

# Check if we want to start services
if [ -f "$WEBDEVDIR/.services_start" ]; then
    startCommand=$(<"$WEBDEVDIR/.services_start")

    rm "$WEBDEVDIR/.services_start"

    docker-compose $startCommand
fi

# Check if we want to stop services
if [ -f "$WEBDEVDIR/.services_stop" ]; then
    stopCommand=$(<"$WEBDEVDIR/.services_stop")

    rm "$WEBDEVDIR/.services_stop"

    docker-compose $stopCommand
fi

# Check if we want to change the nodejs version
if [ -f "$WEBDEVDIR/.nodejs" ]; then
    version=$(<"$WEBDEVDIR/.nodejs")

    rm "$WEBDEVDIR/.nodejs"

    . ~/.nvm/nvm.sh

    nvm use $version
    
    nvm alias default $version
fi