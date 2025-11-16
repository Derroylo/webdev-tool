#!/bin/bash

# webDevCommand: shell_test
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


echo ""
echo "Testing colored text output..."

# Test: Colored text (foreground)
echo -e "\e[31mThis is red text\e[0m"
echo -e "\e[32mThis is green text\e[0m"
echo -e "\e[34mThis is blue text\e[0m"

# Test: Text with background color
echo -e "\e[30;47mBlack text on white background\e[0m"
echo -e "\e[33;41mYellow text on red background\e[0m"
echo -e "\e[37;44mWhite text on blue background\e[0m"

echo ""
echo "Testing text with icons..."

# Unicode icons/symbols test
echo -e "\xE2\x9C\x94 Success: Operation completed successfully."
echo -e "\xE2\x9D\x8C Error: Something went wrong."
echo -e "\xE2\x9A\xA0 Warning: Please double-check your input."
echo -e "\xE2\x9C\xA8 Info: System is up to date."

echo ""
echo "All shell output formatting tests completed."
