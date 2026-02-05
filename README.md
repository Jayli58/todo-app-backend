# Serverless TodoApp

A modern, cloud-native Todo List application built with **.NET 8** and designed for **AWS Serverless** architecture. This project implements a scalable Web API using **Amazon Lambda**, **DynamoDB**, and **Cognito** for authentication.

## 🚀 Tech Stack

*   **Framework**: [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (ASP.NET Core Web API)
*   **Hosting**: [AWS Lambda](https://aws.amazon.com/lambda/) (via `Amazon.Lambda.AspNetCoreServer`)
*   **Database**: [Amazon DynamoDB](https://aws.amazon.com/dynamodb/) (NoSQL)
*   **Authentication**: [Amazon Cognito](https://aws.amazon.com/cognito/) (JWT Bearer Auth)
*   **Background Jobs**: Separate Lambda functions for handling reminders.
*   **Testing**: [xUnit](https://xunit.net/) & [Moq](https://github.com/moq/moq4).
*   **Utilities**: 
    *   [Ulid](https://github.com/Cysharp/Ulid) for sortable unique identifiers.
    *   [Swagger/OpenAPI](https://swagger.io/) for API documentation.

## ✨ Features

*   **CRUD Operations**: Create, Read, Update, and Delete Todo items.
*   **Search**: Efficient filtering of todos.
*   **Reminders**: Schedule email reminders for tasks (architecture uses **DynamoDB Streams** triggered by **TTL expiration**).
*   **Secure**: Protected endpoints using JWT authentication via Amazon Cognito.
*   **Clean Architecture**: Separation of concerns with Services, Repositories, and specific Models.
*   **Global Error Handling**: Centralized exception management and standardized API responses.

## 🛠️ Getting Started

### Prerequisites

*   [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
*   [AWS CLI](https://aws.amazon.com/cli/) (configured with valid credentials)
*   An AWS Account (if running against real cloud resources)

### Installation

1.  **Clone the repository**:
    ```bash
    git clone https://github.com/your-username/your-repo.git
    cd TodoApp/MyApp
    ```

2.  **Restore dependencies**:
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

> **Note**: For local development, this project leverages **LocalStack** to simulate AWS services. This allows you to run **DynamoDB** (including Streams and TTL triggers) and **SES** locally without connecting to the real AWS cloud. The configuration and setup scripts for LocalStack (including **initial DB tables creation**) are located in the `MyApp/local-aws` directory. For email delivery, both SES and Resend were prepared; Resend is the deployed option in `RemainderLambda` because SES production approval is difficult to obtain.

> **Note**: After deploying the new `ActiveTodoId` index, run the backfill tool once to populate `ActiveTodoId`. You can optionally remove the legacy `StatusTodoId` attribute using `--cleanup-status-todo-id`.

## 🏃 Running Locally

You can run the application as a standard ASP.NET Core Web API locally:

```bash
cd MyApp
dotnet run
```

Once running, navigate to `http://localhost:5000/swagger` (or the port indicated in your console) to view the API documentation and test endpoints.

## 🧪 Testing

The solution includes a comprehensive unit test suite in the `TestProject`.

To run tests:
```bash
dotnet test
```

## 📂 Project Structure

*   **`MyApp/`**: The main Web API project.
    *   `Controllers/`: API Endpoints.
    *   `Services/`: Business logic implementations.
    *   `Data/`: Repositories and database access patterns.
    *   `Models/`: DTOs, Entities, and Enums.
*   **`RemainderLambda/`**: Separate project for background reminder processing (includes dedicated unit tests).
*   **`TestProject/`**: Unit tests for the main `MyApp` project.
*   **`Tools/BackfillStatusTodoId/`**: Backfill utility for `ActiveTodoId` (with optional cleanup of legacy `StatusTodoId`).

## ☁️ Deployment

This project is deployed using **AWS CDK**. Before deploying, you need to generate the build artifacts.

Run the following commands from the repository root:

1.  **Build the Web API** (`MyApp`):
    ```bash
    dotnet publish MyApp/MyApp.csproj -c Release -o MyApp/bin/lambda-publish
    ```

2.  **Build the Reminder Worker** (`RemainderLambda`):
    ```bash
    dotnet publish RemainderLambda/src/RemainderLambda/RemainderLambda.csproj -c Release -o RemainderLambda/bin/lambda-publish
    ```

After building, the artifacts will be ready for your CDK pipeline to deploy.
