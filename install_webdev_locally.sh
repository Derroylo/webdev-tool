#!/usr/bin/env bash

echo "Building the application..."
dotnet build --configuration Release --no-restore --self-contained false /p:PublishSingleFile=true /p:PublishTrimmed=true

# Create the zip file
echo "Creating webdev-tool.zip..."
cd ./bin/Release/net9.0 && zip -r ../../../webdev-tool.zip ./* && cd -

# Check if webdev-tool.zip exists
if [ ! -f "webdev-tool.zip" ]; then
    echo "Error: webdev-tool.zip not found. Please ensure the build completed successfully."
    exit 1
fi

# Install webdev tool on host first
echo "Installing webdev tool on host..."
rm -rf ~/webdev-host
mkdir ~/webdev-host
unzip -o webdev-tool.zip -d ~/webdev-host/
chmod +x ~/webdev-host/webdev.sh
sudo ln -sf ~/webdev-host/webdev.sh /usr/local/bin/webdev