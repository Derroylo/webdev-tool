#!/bin/bash

# webDevCommand: progressbar_test
# webDevBranch: shopware
# webDevBranchDescription: Commands for shopware
# webDevDescription: Setup shopware

echo "Testing progress bar..."

progressbar() {
    local progress=${1}
    local width=40
    local done=$((progress * width / 100))
    local left=$((width - done))
    # Build the progress bar string
    fill=$(printf "%${done}s")
    empty=$(printf "%${left}s")
    printf "\rProgress : [%-${width}s] %3d%%" "${fill// /#}${empty// /-}" "${progress}"
}

for i in $(seq 0 2 100); do
    progressbar $i
    sleep 0.05
done
echo ""
echo "Done."
