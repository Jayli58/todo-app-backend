## Serverless TodoApp

Serverless TodoApp is a cloud-native Todo List API built with **.NET 8** and AWS serverless services. It ships a Lambda-hosted ASP.NET Core Web API, DynamoDB storage, Cognito auth, and a reminder worker Lambda.

### TL;DR

![Architecture](./todo-arch-layout.svg)

- ASP.NET Core Web API running on AWS Lambda with `Amazon.Lambda.AspNetCoreServer`.
- DynamoDB for storage, Cognito for JWT auth, and a reminder Lambda for TTL-based email delivery.

### Tech Stack

- **Framework**: [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (ASP.NET Core Web API)
- **Hosting**: [AWS Lambda](https://aws.amazon.com/lambda/) (via `Amazon.Lambda.AspNetCoreServer`)
- **Database**: [Amazon DynamoDB](https://aws.amazon.com/dynamodb/) (NoSQL)
- **Authentication**: [Amazon Cognito](https://aws.amazon.com/cognito/) (JWT Bearer Auth)
- **Background Jobs**: Separate Lambda functions for handling reminders
- **Testing**: [xUnit](https://xunit.net/) & [Moq](https://github.com/moq/moq4)
- **Utilities**:
  - [Ulid](https://github.com/Cysharp/Ulid) for sortable unique identifiers
  - [Swagger/OpenAPI](https://swagger.io/) for API documentation

### Features

- **CRUD Operations**: Create, Read, Update, and Delete Todo items
- **Search**: Keyword search with status filters (All, Active, Completed)
- **Load More**: Button to fetch additional todos
- **Reminders**: Schedule email reminders for tasks (uses **DynamoDB Streams** triggered by **TTL expiration**)

### Repo Contents

- `MyApp/`: Main Web API project
- `RemainderLambda/`: Background reminder processing (includes dedicated unit tests)
- `TestProject/`: Unit tests for the main `MyApp` project
- `Tools/BackfillStatusTodoId/`: Backfill utility for `ActiveTodoId` (with optional cleanup of legacy `StatusTodoId`)
- `Tools/BackfillTodoSearchLower/`: Backfill utility for `TitleLower`/`ContentLower` to support case-insensitive search

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [AWS CLI](https://aws.amazon.com/cli/) (configured with valid credentials)
- An AWS Account (if running against real cloud resources)

### Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/Jayli58/todo-app-backend.git
   cd TodoApp/MyApp
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

### Configuration

The application uses `appsettings.json` for configuration. You may need to update the AWS region or DynamoDB table names if you are connecting to specific cloud resources.

Example `appsettings.json` structure:
```json
{
  "AWS": {
    "DynamoDB": {
      "Region": "us-east-1"
    }
  },
  "Cognito": {
    "Region": "us-east-1",
    "UserPoolId": "YOUR_USER_POOL_ID",
    "ClientId": "YOUR_CLIENT_ID"
  },
  "Frontend": {
    "Url": "https://your-frontend.example"
  }
}
```

These values are mapped in CDK to the Lambda environment variables (`AWS__DynamoDB__Region`, `Cognito__Region`, `Cognito__UserPoolId`, `Cognito__ClientId`, `Domain`, `Frontend__Url`).

Local development example:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AWS": {
    "DynamoDB": {
      "ServiceURL": "http://localhost:4566",
      "Region": "ap-southeast-2",
      "AccessKeyId": "test",
      "SecretAccessKey": "test"
    }
  },
  "Cognito": {
    "Region": "ap-southeast-2",
    "UserPoolId": "YOUR_USER_POOL_ID",
    "ClientId": "YOUR_CLIENT_ID"
  },
  "Frontend": {
    "Url": "http://localhost:3000"
  }
}
```

RemainderLambda local `.env` example:
```
SES_AUTH_REGION=ap-southeast-2
MAIL_SENDER=YOUR_MAIL_ADDRESS
USE_LOCALSTACK=true
RESEND_API_KEY=YOUR_API_KEY
TEST_MAIL_RECEIVER=YOUR_MAIL_ADDRESS
```

## Running Locally

You can run the application as a standard ASP.NET Core Web API locally:

```bash
cd MyApp
dotnet run
```

Once running, navigate to `http://localhost:5000/swagger` (or the port indicated in your console) to view the API documentation and test endpoints.

## Testing

The solution includes a comprehensive unit test suite in the `TestProject`.

To run tests:
```bash
dotnet test
```

## Deployment

This project is deployed using **AWS CDK**. Before deploying, you need to generate the build artifacts.

Run the following commands from the repository root:

1. **Build the Web API** (`MyApp`):
   ```bash
   dotnet publish MyApp/MyApp.csproj -c Release -o MyApp/bin/lambda-publish
   ```

2. **Build the Reminder Worker** (`RemainderLambda`):
   ```bash
   dotnet publish RemainderLambda/src/RemainderLambda/RemainderLambda.csproj -c Release -o RemainderLambda/bin/lambda-publish
   ```

After building, the artifacts will be ready for your CDK pipeline to deploy.

## Notes

- For local development, this project leverages **LocalStack** to simulate AWS services. This allows you to run **DynamoDB** (including Streams and TTL triggers) and **SES** locally without connecting to the real AWS cloud. The configuration and setup scripts for LocalStack (including **initial DB tables creation**) are located in `MyApp/local-aws`. For email delivery, both SES and Resend were prepared; Resend is the deployed option in `RemainderLambda` because SES production approval is difficult to obtain.
- After deploying the new `ActiveTodoId` index, run the backfill tool once to populate `ActiveTodoId`. You can optionally remove the legacy `StatusTodoId` attribute using `--cleanup-status-todo-id`.
- After enabling case-insensitive search, run the backfill tool once to populate `TitleLower`/`ContentLower` for existing todos: `dotnet run --project Tools/BackfillTodoSearchLower -- --region <region> --table Todos`. Use `--dry-run` to preview changes.
