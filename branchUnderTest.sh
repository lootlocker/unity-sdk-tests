#!/bin/bash
CURRENT_BRANCH=$(git branch --show-current)
REQUESTED_TEST_BRANCH=$(git log -1 --pretty=%B | sed -nr 's/[Tt][Ee][Ss][Tt][Ss]\s[Bb][Rr][Aa][Nn][Cc][Hh]:\s(.*)/\1/p')
if [ "$CURRENT_BRANCH" = "main"  ] || [ -z "$REQUESTED_TEST_BRANCH" ];
then
    echo "main"
else
    echo "$REQUESTED_TEST_BRANCH"
fi
