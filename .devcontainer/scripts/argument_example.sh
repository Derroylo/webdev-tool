#!/usr/bin/env bash

# Infos for WebDevTool
# webDevCommand: arg
# webDevDescription: I am an argument example command. First argument is the username, then age and then the full name. This could look like this: gpt arg user 20 'John Doe'
# webDevArgument: Username
# webDevArgument: The age of the user
# webDevArgument: The full name of the user

echo "Example output"
echo "Username: $1"
echo "Age: $2"
echo "Full name: $3"