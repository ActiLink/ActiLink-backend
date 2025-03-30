!/bin/bash

ENV_FILE=".env"
VAR_NAME="JWT_SECRET"

# generate secret key
VAR_VALUE=$(openssl rand -base64 32)

touch "$ENV_FILE"

found=false

TMP_FILE="${ENV_FILE}.tmp"

while IFS= read -r line || [[ -n "$line" ]]; do
        if [[ "$line" == "$VAR_NAME="* ]]; then
                echo "$VAR_NAME=$VAR_VALUE" >> "$TMP_FILE"
                found=true
        else
                echo "$line" >> "$TMP_FILE"
        fi
done < "$ENV_FILE"

if [[ "$found" = false ]]; then
        echo "$VAR_NAME=$VAR_VALUE" >> "$TMP_FILE"
fi

mv "$TMP_FILE" "$ENV_FILE"

echo "Variable $VAR_NAME created and saved to $ENV_FILE"
