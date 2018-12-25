# HealthCheckAgent
A PoC health check agent and metrics submitter using Azure Functions that works as shown below:

1. The application under monitoring exposes well known health endpoints
2. The Azure Function thats acting as a health check agent polls this health endpoint every 1 minute.
3. Processes the health report and sends metrics to DataDog using REST API.
4. The metrics can then be graphed in DataDog.

![](https://i.imgur.com/1cb6CB3.png)
