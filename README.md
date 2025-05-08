# CLI Tool for DevContainer

## Purpose
This CLI Tool aims to make it easier to use devcontainer for web development.

## Documentation
The documentation can be found under [WebDev Documentation](https://derroylo.github.io).

## Local installation in Linux or WSL2

Add the microsoft repository as the official ubuntu package manager doesn´t contain the dotnet runtime in version 9
```
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt update
```

Install the dotnet runtime
```
sudo apt-get install -y dotnet-runtime-9.0
```

Download the latest release of this tool
```
curl -s https://api.github.com/repos/Derroylo/webdev-tool/releases/latest | grep "browser_download_url.*zip" | cut -d : -f 2,3 | tr -d \" | wget -qi -
```

Unzip the downloaded file, create a folder in your home directory and add an alias to your bashrc.
__If you are not using bashrc, you might need to change the following lines accordingly.__
```
mkdir ~/webdev
unzip webdev-tool.zip -d ~/webdev/
rm webdev-tool.zip
echo "alias webdev='dotnet $HOME/webdev/webdev.sh'" > .bash_aliases
chmod +x $HOME/webdev/webdev-tool.sh
source ~/.bashrc
```

If everything worked, then you should be able to use the `webdev` command in the terminal.

## Issues, Feature requests etc.
Create an issue if you encounter problems with this tool or have suggestions on what to add next.