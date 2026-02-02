# ADR 002: Mass Transit and State Machine Sagas

**Status:** Accepted
**Date:** 2026-02-02
**Deciders:** Ramon Reyes
**Technical Story:** N/A

## Context and Problem Statement
The system must validate manifests against against international trade laws (HTS Codes), which may require multiple external data sources or services in the future.

## Decision Drivers
* Need to protect entire compliance workflow
* Need to establish "ground rules" for processing compliance checks
* Need to ensure "integrity" of compliance submissions

## Considered Options
* **Traditional Client-Server Communication:** Non-event-driven communications using simple HTTP requests, with an option to fail out the process during back-end processing.
* **Pub/Sub:** Use Pub/Sub model to encourage async / decoupled communication.

## Decision Outcome
We chose the Orchestration Pattern using **Mass Transit State Machines**. This centralizes the logic for "Reference Validation" and "HTS Code Validation" into a single definition, allowing us to easily add steps (like "Sanctions Check") later without breaking existing consumers.

1.  **Mass Transit:** Pairing Mass Transit with RabbitMQ provides asynchronous, loosely-coupled messaging and helps create resilient, scalable experiences.
2.  **State Machine Sagas:** This pattern is ideal for workflows like flight booking, order processing, or payment systems where multiple services must coordinate asynchronously while maintaining eventual consistency.

## Pros and Cons of the Options

### Mass Transit
* **Good:** More scalable than direct HTTP / REST communication (along with some other less task-oriented approaches via Azure Service Bus, Kafka, or gRPC).
* **Good:** Provides built-in support for retry policies, exception handling, in-memory testing, request-response patterns, and distributed sagas.
* **Good:** Simplifies consumer and producer logic through dependency injection and a consistent abstraction layer.
* **Bad:** Mass Transit influences app dev more into .NET ecosystem.

### State Machine Sagas
* **Good:** Pairs with Mass Transit to define type-safe sagas in .NET.
* **Good:** The current saga state is always known, aiding monitoring and debugging.
* **Good:** Supports long-running, complex workflows with retries and compensations.
* **Bad:** Services must handle idempotency to safely retry messages.
* **Bad:** Requires robust monitoring and tracking to audit saga progress and detect failures.