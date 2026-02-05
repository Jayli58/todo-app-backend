#!/usr/bin/env bash
set -euo pipefail

echo "[init] Creating DynamoDB tables + Streams + TTL..."

# Create Todos table (UserId PK, TodoId SK)
awslocal dynamodb create-table \
  --table-name Todos \
  --attribute-definitions \
      AttributeName=UserId,AttributeType=S \
      AttributeName=TodoId,AttributeType=S \
      AttributeName=ActiveTodoId,AttributeType=S \
  --key-schema \
      AttributeName=UserId,KeyType=HASH \
      AttributeName=TodoId,KeyType=RANGE \
  --global-secondary-indexes \
      "[\
        {\
          \"IndexName\": \"UserIdActiveTodoId\",\
          \"KeySchema\": [\
            {\"AttributeName\":\"UserId\",\"KeyType\":\"HASH\"},\
            {\"AttributeName\":\"ActiveTodoId\",\"KeyType\":\"RANGE\"}\
          ],\
          \"Projection\": {\"ProjectionType\":\"ALL\"}\
        }\
      ]" \
  --billing-mode PAY_PER_REQUEST \
  # Create the table quietly
  >/dev/null 2>&1 || echo "[init] Todos already exists"

# Create TodoReminders table (UserId PK, TodoId SK)
awslocal dynamodb create-table \
  --table-name TodoReminders \
  --attribute-definitions \
      AttributeName=UserId,AttributeType=S \
      AttributeName=TodoId,AttributeType=S \
  --key-schema \
      AttributeName=UserId,KeyType=HASH \
      AttributeName=TodoId,KeyType=RANGE \
  --billing-mode PAY_PER_REQUEST \
  --stream-specification StreamEnabled=true,StreamViewType=NEW_AND_OLD_IMAGES \
  >/dev/null 2>&1 || echo "[init] TodoReminders already exists"

# Enable TTL on TodoReminders
awslocal dynamodb update-time-to-live \
  --table-name TodoReminders \
  --time-to-live-specification "Enabled=true, AttributeName=RemindAtEpoch" \
  >/dev/null 2>&1 || echo "[init] TTL already enabled or not supported"

echo "[init] Done."
