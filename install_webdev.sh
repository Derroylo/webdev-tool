#!/usr/bin/env bash

# Disable job control
set +m

# Install the .NET SDK in version 9.0, if not already installed
if ! dotnet --list-sdks | grep -q "9.0"; then
    echo "Installing .NET SDK 9.0..."
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
    chmod +x dotnet-install.sh
    ./dotnet-install.sh --version 9.0
    rm dotnet-install.sh
else
    echo ".NET SDK 9.0 is already installed."
fi

# Download the latest version of webdev-tool
curl -s https://api.github.com/repos/Derroylo/webdev-tool/releases/latest | grep "browser_download_url.*zip" | cut -d : -f 2,3 | tr -d \" | wget -qi -

# Create the webdev directory, unzip the tool, and set permissions
mkdir -p ~/webdev
unzip webdev-tool.zip -d ~/webdev/
rm webdev-tool.zip
chmod +x $HOME/webdev/webdev-tool.sh
ln -s $HOME/webdev/webdev.sh /usr/local/bin/webdev

# Show a message to the user and what he can do next etc.
WEBDEV_DISABLE_HEADER=1 dotnet "$HOME/webdev/webdev-tool.dll" "dotnet run webdev-install-summary"