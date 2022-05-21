#!/bin/bash

NEWIP=$(curl icanhazip.com)
OLDIP=$(cat currentIP.txt)

FUNCTIONURL=""

if [ "$NEWIP" != "$OLDIP" ]; then
    echo $NEWIP > currentIP.txt
    curl -s -X POST -H "Content-Type: application/json" -d "{\"ip\":\"$NEWIP\"}" $FUNCTIONURL
fi