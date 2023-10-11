# SMS Microservice Documentation

## Problem Statement
The goal of this microservice is to enable M-KOPA, a company that primarily communicates with customers via SMS, to interact with a 3rd-party SMS service. The microservice listens to a message queue for incoming SMS commands, sends SMS messages to customers, and publishes "SmsSent" events to a global event bus. Key objectives include ensuring reliable SMS delivery and handling potential issues gracefully.

This documentation provides an overview of the architecture, component details, and design decisions for the SMS Microservice implemented in C#.NET. This microservice interacts with a 3rd-party SMS service through asynchronous patterns, handling SMS message delivery to customers.

## Design and Patterns
### Asynchronous Messaging
The core of this microservice revolves around asynchronous messaging. It uses the following patterns:

- Message Queue: A message queue is used to receive and process SMS commands asynchronously. This decouples message producers (clients sending SMS commands) from consumers (this microservice), enabling scalability and fault tolerance.

- Event-Driven Architecture: Events, such as "SmsSent," are published to a global event bus to notify other parts of the system about significant SMS events. This enables loose coupling between components and supports real-time updates.

### Dependency Injection
Dependency Injection is used extensively to manage and provide necessary components and services. This promotes modularity, testability, and flexibility in choosing concrete implementations for various components.

### Retry Strategy
To ensure the reliability of SMS message delivery, a retry strategy is implemented when sending SMS messages to the 3rd-party SMS service. It retries a configurable number of times with a delay between retries.


## Architecture Overview

The SMS Microservice comprises several key components that work together to ensure reliable SMS message delivery and event publishing:
<pre>
  +-----------------+      +------------------------+       +------------------+
  | SendSms Command |  --> | SMS Microservice (C#)  |  --> | 3rd Party SMS API  |
  +-----------------+      +------------------------+       +------------------+
                            |    - Message Queue     |
                            |    - HTTP Request      |
                            |    - Event Bus         |
                            |    - Logger            |
                            +------------------------+
</pre>

### MessageQueueListener

- **Purpose**: Listens to the message queue, specifically the `SendSms` commands, and processes them.
- **Dependencies**: `IConfiguration` for configuration settings, `IEventProcessor` for message processing, and `IMessageQueue` for message queue connection.
- **Execution**: The `ExecuteAsync` method subscribes to the message queue, invoking the `HandleMessage` method upon receiving a message.
- **Message Handling**: When a command is received, it logs the event and delegates message processing to the `EventProcessor`.

### EventProcessor

- **Purpose**: Handles the deserialization and processing of messages received from the message queue.
- **Dependencies**: `IServiceScopeFactory` to create service scopes for dependency injection.
- **Message Processing**: It deserializes incoming messages, determines the command type, and processes it accordingly. Currently, it supports the "SendSms" command type.
- **Dependency Injection**: It resolves the `ISmsService` from the service scope and delegates the message processing to it.

### SmsService

- **Purpose**: Responsible for sending SMS messages to customers and publishing "SmsSent" events to the event bus.
- **Dependencies**: `IMapper` for mapping DTOs, `IHttpSmsDataClient` for making HTTP requests, `IEventBus` for publishing events, and `ILoggerLibrary` for logging.
- **Message Validation**: It validates incoming SMS messages to ensure they contain valid data.
- **Duplicate Message Handling**: It checks for duplicate messages within a specified time frame and avoids immediate resending.
- **Message Sending**: It sends SMS messages via an HTTP request to the 3rd-party SMS service and publishes "SmsSent" events if successful.

### HttpSmsDataClient

- **Purpose**: Handles HTTP communication with the 3rd-party SMS service.
- **Dependencies**: `HttpClient` for making HTTP requests and reads configuration settings from `IConfiguration`.
- **Retry Logic**: It incorporates retry logic when sending SMS messages to the 3rd-party service, with a maximum number of retry attempts and a delay between retries.


## Assumptions and Trade-offs

### Abstractions and Technology Agnosticism
The main code is written against abstractions for components such as message queues, event buses, and loggers. Concrete implementations will be added later. This allows for flexibility in choosing the best technologies for the specific environment. In some cases, some concrete cases were created purposely for functional testing.
### Unknown API Contract
The contract for the 3rd-party SMS API is not yet known. A concrete client will be developed separately. This approach allows for flexibility in adapting to different API specifications. A simple concrete dummy api was generated to simulate a 3rd party api.
### Reliability Over Efficiency
The microservice prioritizes reliable SMS message delivery to customers. It implements a delay between retry attempts, which may add a slight delay in message delivery but ensures that messages are sent successfully.
### Handling Duplicate Messages
Duplicate SMS messages are handled by not immediately resending them. Instead, the system checks for recent duplicates and avoids resending within a specified time frame (e.g., 2 minutes). This trade-off aims to prevent annoyance to customers while ensuring message delivery.
### Error Handling: 
Errors in message processing and communication with the 3rd-party service are logged.
### Configuration
Configuration settings (e.g., API endpoints, retry settings) are expected to be available in configuration files (e.g., `appsettings.json`). This approach allows for easy configuration changes without code modifications.


## Runing the Application

### Prerequisites
Before running the application, make sure you have the following prerequisites installed on your development environment:

1. Docker to run RabbitMQ.
2. Visual Studio with .NET SDK for running the C# solution.

### Step 1: Start RabbitMQ Docker Container
Open a terminal or command prompt.
Use the following Docker command to spin up a RabbitMQ container and expose port 8080:

```bash
docker run -d --hostname rmq --name rabbit-server -p 8080:15672 -p 5672:5672 rabbitmq:3-management
```
This command will start a RabbitMQ container with the RabbitMQ Management Plugin enabled, making it accessible on port 8080.

### Step 2: Open the Solution in Visual Studio
- Open Visual Studio.
- Open the SMS Microservice solution within Visual Studio. Make sure all the projects within the solution are loaded.

### Step 3. Build and Run the Solution
In Visual Studio, right-click on the solution in the Solution Explorer and select "Build" to compile the projects.
- Once the build is successful, right-click on the solution again and select "Set StartUp Projects." and start all projects

### Step 4: Make a Request from the SMS Client
In the solution, locate the "SMSClient" project.
- Create a request to send an SMS message to the SMS Microservice. Ensure that the request includes valid phone number and SMS text parameters.
- Execute the request within the "SMSClient" project.

### Step 5: Observe Interaction
As you make the request from the SMS Client project, observe the interaction of the entire flow.
- Check the console outputs for logs and messages generated by the SMS Microservice, including the message handling and publication of "SmsSent" events.

## Runing Unit Tests

## Unit Tests

The SMS Microservice includes a set of unit tests to ensure the correctness of its functionality. These tests cover various scenarios, including:

- Validation of valid and invalid SMS messages.
- Successful message delivery to a 3rd-party service.
- Error handling in message processing and communication with the 3rd-party service.

These unit tests are essential for maintaining the reliability and robustness of the microservice.

1. Navigate to the ./test/SmsMicroservice.Tests/ project
2. Generate report code coverage
```bash
dotnet test /p:CollectCoverage=true
```
or Run from the VS IDE

Thank you <3.