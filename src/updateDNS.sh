#!/bin/bash

NEWIP=$(curl icanhazip.com)
OLDIP=$(cat currentIP.txt)

FUNCTIONURL=""

if [ "$NEWIP" != "$OLDIP" ]; then
    echo $NEWIP > currentIP.txt
    curl -s -X POST -d "$NEWIP" $FUNCTIONURL
fi