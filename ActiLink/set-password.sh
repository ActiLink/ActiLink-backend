#!/bin/bash

# create MSSQL_SA_PASSWORD environment variable in .env file

if [ -z "$1" ]; then
    exit 1
fi

VAR_NAME="MSSQL_SA_PASSWORD"
VAR_VALUE="$1"

if [ ! -f .env ]; then
    touch .env
fi

if grep -q "^$VAR_NAME=" .env; then
    sed -i "s/^$VAR_NAME=.*/$VAR_NAME=$VAR_VALUE/" .env
else
    echo "$VAR_NAME=$VAR_VALUE" >> .env
fi
