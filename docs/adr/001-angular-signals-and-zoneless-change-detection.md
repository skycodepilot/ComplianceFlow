# ADR 001: Angular Signals And Zoneless Change Detection

**Status:** Accepted
**Date:** 2026-02-02
**Deciders:** Ramon Reyes
**Technical Story:** N/A

## Context and Problem Statement
The system must decouple the UI from back-end processing, and allow compliance-checking workflows to handle code check requests in a performant manner without bogged-down UI update / refresh cycles (which would happen with more-traditional front-end / back-end communication protocols).

## Decision Drivers
* Need to support low-latency, high-performance UI work.
* Need to keep UI responsive and avoid excessive / unnecessary change detection.
* Need to simplify state management for asynchronous polling mechanisms.

## Considered Options
* **Refactored Component Classes:** Work within existing Angular components / frameworks to optimize normal Angular UI / network traffic.
* **External Message Broker:** Use RabbitMQ or Azure Service Bus.
* **Signals:** Use Angular Signals and Zoneless Change Detection to remove unnecessary pipeline activities.

## Decision Outcome
We chose **Angular 21 Signals** to manage the asynchronous "Manifest State." Since the backend is event-driven, the UI must poll for status updates. Signals allow us to reactively update the UI from "Submitted" to "Validated" without the overhead of `Zone.js` tracking every polling HTTP request, significantly reducing browser CPU usage during wait times.

1.  **Angular Signals:** Unlike the traditional zone.js-based system, which triggers full component tree checks after any change, signals allow Angular to identify exactly where and what has changed, updating only the affected UI parts.
2.  **Zoneless Change Detection:** With signals, Angular can move toward zoneless execution, removing the overhead of zone.js and enabling faster, more predictable updates—especially critical in large, real-time applications like dashboards or data grids.
3.  **Easier Debugging:** Without Zone.js wrapping stack traces, developers get clean, accurate error traces, making it simpler to identify issues.

## Pros and Cons of the Options

### Angular Signals
* **Good:** Targeted updates - Signals track dependencies automatically, updating only affected / necessary signals and reducing reload overhead.
* **Good:** Functions running in response to signal changes don't require manual subscriptions.
* **Good:** Reduces re-renders significantly, helping performance.
* **Bad:** Signals trigger change detection on reference changes, even if the actual data hasn't changed.
* **Bad:** Angular signals can be harder to debug than RxJS observables.

### Zoneless Change Detection
* **Good:** Improved performance - change detection only runs when needed
* **Good:** Clean and accurate error traces reduce time for error / log parsing
* **Good:** Removing Zone.js reduces the application's payload (faster load times)
* **Bad:** Manual change detection required - async operations or manual subscriptions must have markForCheck() or signal updates to refresh a view
* **Bad:** Without Zone.js monitoring async tasks, it's easier to forget to notify Angular of state changes, leading to bugs where the UI doesn’t reflect the latest data.