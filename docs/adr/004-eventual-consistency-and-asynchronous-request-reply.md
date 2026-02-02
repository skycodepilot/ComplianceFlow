# ADR 004: Eventual Consistency and Asynchronous Request-Reply

**Status:** Accepted
**Date:** 2026-02-02
**Deciders:** Ramon Reyes
**Technical Story:** N/A

## Context and Problem Statement
Validating shipping manifests involves a distributed workflow that may expand to include multiple external checks (Sanctions, HTS Codes, Restricted Parties). If the User Interface (UI) were to wait for all these processes to complete synchronously within a single HTTP request, it would lead to a poor user experience, potential browser timeouts, and "thread starvation" on the web server.

## Decision Drivers
* **Reliability:** Need to prevent browser timeouts during long-running validation processes.
* **Scalability:** Need to decouple the immediate user response from the duration of backend processing.
* **User Experience:** Need to provide immediate feedback ("Submission Received") while complex logic runs in the background.

## Considered Options
* **Synchronous HTTP (Blocking):** The API keeps the connection open until the Saga completes.
* **WebSockets / SignalR:** Real-time push notifications to the client.
* **Asynchronous Request-Reply (Polling):** The API accepts the work immediately, and the client checks back periodically for the result.

## Decision Outcome
We chose to implement the **Asynchronous Request-Reply** pattern with client-side polling.

1.  **Immediate Acceptance:** The API returns `202 Accepted` and a unique `CorrelationId` immediately upon submission.
2.  **Saga Orchestration:** The backend processes the validation asynchronously using MassTransit Sagas.
3.  **Client Polling:** The Angular UI uses Signals to manage a polling interval, checking the `ManifestState` endpoint until the status transitions to a final state (`Validated` or `Rejected`).

## Pros and Cons of the Options

### Asynchronous Request-Reply (Polling)
* **Good:** Decouples the frontend from backend latency; the UI never freezes or times out waiting for a server response.
* **Good:** Uses standard HTTP protocols; does not require maintaining persistent WebSocket connections or additional infrastructure (like Azure SignalR Service).
* **Good:** Resilient; if the backend is momentarily slow, the UI simply polls again without losing the connection.
* **Bad:** "Chatty" network traffic; multiple HTTP requests are made to check status, increasing network load compared to a single push notification.
* **Bad:** Requires more complex frontend logic to manage the polling lifecycle (start, stop, timeout handle).