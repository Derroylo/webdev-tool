#!/usr/bin/env bash

# Disable job control
set +m

# This file is the entrypoint for the tool and is required for the self-update

# Get the location of this script
SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")

# Directory where webdev is located
WEBDEVDIR=$SCRIPTPATH

# run the application and pass all arguments to it
dotnet "$WEBDEVDIR/webdev-tool.dll" "$@"

# Check if the update folder exists
if [ -d "$WEBDEVDIR/update" ]; then
    cd $WEBDEVDIR

    # Move all files from the update folder to the current one and remove it afterwards
    rsync -a update/* .

    # Remove the update folder
    rm -rf update

    # Set execution rights for the shell script
    chmod +x webdev.sh
fi

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

    source /usr/local/share/nvm/nvm.sh
    
    nvm install $version
  
    nvm use $version --save
fi

# Check if we want to start a devcontainer workspace
if [ -f "$WEBDEVDIR/.devcontainer_up" ]; then
    rm "$WEBDEVDIR/.devcontainer_up"
       
    devcontainer up --workspace-folder .
fi

if [ -f "$WEBDEVDIR/.workspaces_start" ]; then
    # Read the file $WEBDEVDIR/.workspaces_start and iterate over each line
    while IFS= read -r line; do
        # Check if the line is not empty
        if [ -n "$line" ]; then
            # Start the workspace
            cd "$line" || continue
            devcontainer up --workspace-folder .
            cd "$WEBDEVDIR" || exit 1
        fi
    done < "$WEBDEVDIR/.workspaces_start"
  
    rm "$WEBDEVDIR/.workspaces_start"
       
    # Show a summary of the workspaces to help user getting started
    WEBDEV_DISABLE_HEADER=1 dotnet "$WEBDEVDIR/webdev-tool.dll" "workspaces-start-summary"
fi

if [ -f "$WEBDEVDIR/.workspaces_tasks" ] && [[ ! "$@" == *"--not-main"* ]]; then   
    WORKSPACE_DIR=$(pwd)
    
    while IFS= read -r line; do
        # Check if the line is not empty
        if [ -n "$line" ]; then
            # Execute the commands given in the variable
            eval "$line"
            cd "$WORKSPACE_DIR" || exit 1
        fi
    done < "$WEBDEVDIR/.workspaces_tasks"
  
    rm "$WEBDEVDIR/.workspaces_tasks"
fi

# Check if we want to open a terminal in the devcontainer
if [ -f "$WEBDEVDIR/.terminal" ]; then
    id=$(<"$WEBDEVDIR/.terminal")
    
    rm "$WEBDEVDIR/.terminal"

    docker exec -it -w /var/www/html $id bash
fi