#!/usr/bin/env bash

# Step 1: Install devcontainer CLI and nvm if not already installed
echo "Step 1: Checking and installing devcontainer CLI and nvm..."

# Check if devcontainer CLI is installed
if ! command -v devcontainer &> /dev/null; then
    echo "Installing devcontainer CLI..."
    
    # Install nvm if not already installed
    if [ ! -d "/usr/local/share/nvm" ] && [ ! -d "$HOME/.nvm" ]; then
        echo "Installing nvm..."
        curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.0/install.sh | bash
    fi
    
    # Set NVM_DIR based on where nvm is actually installed
    if [ -d "/usr/local/share/nvm" ]; then
        export NVM_DIR="/usr/local/share/nvm"
    else
        export NVM_DIR="$HOME/.nvm"
    fi
    
    echo "nvm is installed at: $NVM_DIR"
    [ -s "$NVM_DIR/nvm.sh" ] && \. "$NVM_DIR/nvm.sh"
    [ -s "$NVM_DIR/bash_completion" ] && \. "$NVM_DIR/bash_completion"
    
    # Install Node.js using nvm if not already installed
    if ! command -v node &> /dev/null; then
        echo "Installing Node.js using nvm..."
        # Source nvm again to ensure it's available in this context
        [ -s "$NVM_DIR/nvm.sh" ] && \. "$NVM_DIR/nvm.sh"
        
        nvm install node
        nvm use node
        # Add node and npm to PATH for this session
        export PATH="$NVM_DIR/versions/node/$(nvm version node)/bin:$PATH"
    else
        echo "Node.js is already installed"
        # Ensure nvm is sourced and PATH is set correctly
        [ -s "$NVM_DIR/nvm.sh" ] && \. "$NVM_DIR/nvm.sh"
        export PATH="$NVM_DIR/versions/node/$(nvm version node)/bin:$PATH"
    fi
    
    # Install devcontainer CLI using npm
    echo "Installing devcontainer CLI using npm..."
    # Ensure npm is available
    [ -s "$NVM_DIR/nvm.sh" ] && \. "$NVM_DIR/nvm.sh"
    export PATH="$NVM_DIR/versions/node/$(nvm version node)/bin:$PATH"
    npm install -g @devcontainers/cli
else
    echo "devcontainer CLI is already installed"
fi

# Build the application
echo "Building the application..."
dotnet build --configuration Release --no-restore --self-contained false /p:PublishSingleFile=true /p:PublishTrimmed=true

# Create the zip file
echo "Creating webdev-tool.zip..."
cd ./bin/Release/net9.0 && zip -r ../../../webdev-tool.zip ./* && cd -

echo "Step 1 completed: devcontainer CLI and nvm are ready, and webdev-tool.zip has been created."

# Step 2: Install webdev tool on host and start devcontainer
echo "Step 2: Installing webdev tool on host and starting devcontainer..."

# Check if webdev-tool.zip exists
if [ ! -f "webdev-tool.zip" ]; then
    echo "Error: webdev-tool.zip not found. Please ensure the build completed successfully."
    exit 1
fi

# Install webdev tool on host first (needed for initializeCommand)
echo "Installing webdev tool on host..."
rm -rf ~/webdev-host
mkdir ~/webdev-host
unzip -o webdev-tool.zip -d ~/webdev-host/
chmod +x ~/webdev-host/webdev.sh
sudo ln -sf ~/webdev-host/webdev.sh /usr/local/bin/webdev

# Navigate to the devcontainer directory
cd devcontainer-testenv

# Reset environment variables
unset WEBDEV_WORKSPACE_FOLDER

# Stop the devcontainer if it is running
echo "Stopping any running devcontainers..."
if [ -n "$(docker ps -q -f name=devcontainer-app)" ]; then
    docker stop $(docker ps -q -f name=devcontainer-app)
fi

# Start the devcontainer
echo "Starting devcontainer..."
devcontainer up --workspace-folder . --remove-existing-container

# Wait a moment for the container to be fully ready
sleep 5

# Copy the zip file to the container
echo "Copying webdev-tool.zip to container..."
cp /workspaces/webdev-tool/webdev-tool.zip /workspaces/webdev-tool/devcontainer-testenv/webdev-tool.zip
devcontainer exec --workspace-folder . mkdir -p /home/webdev/webdev
devcontainer exec --workspace-folder . mv webdev-tool.zip /home/webdev/webdev/

# Unzip the file in the container
echo "Unzipping webdev-tool.zip in container..."
devcontainer exec --workspace-folder . bash -c "cd /home/webdev/webdev && unzip -o webdev-tool.zip"
devcontainer exec --workspace-folder . bash -c "chmod +x /home/webdev/webdev/webdev.sh"
devcontainer exec --workspace-folder . bash -c "sudo ln -s /home/webdev/webdev/webdev.sh /usr/local/bin/webdev"

echo "Step 2 completed: Devcontainer is running and webdev-tool.zip has been copied and extracted to ~/webdev/"

# Return to the original directory
cd ..

# Delete the zip file
echo "Deleting webdev-tool.zip..."
rm webdev-tool.zip

