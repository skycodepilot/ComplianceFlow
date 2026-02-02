# ADR 003: Docker Containers

**Status:** Accepted
**Date:** 2026-02-02
**Deciders:** Ramon Reyes
**Technical Story:** N/A

## Context and Problem Statement
The system must provide an easily-deployable infrastructure for development and deployment, where the back-end features can exist independent of any front-end implementation.

## Decision Drivers
* Need for better development processes (cloning, development, testing) independent of specific developer environments
* Need to provide consistent environments for business logic and database table(s).

## Considered Options
* **Traditional On-Prem Or Cloud:** Standardized development equipment and hosting norms for app / database development.

## Decision Outcome
We chose to implement **Docker** to provide a consistent and reliable development environment for both the business layer and the database table(s) required.

1.  **Business Layer / Server-Side Logic:** Docker as a means of standardizing development and preventing server-side "it works on my machine" issues during local dev.
2.  **Separation Of Concerns:** Docker-ized server-side work keeps front-end implementations totally removed, allowing separation of concerns and also to let different teams drive front and back-end development.

## Pros and Cons of the Options

### Docker-ized Business Layer / Server-Side Logic
* **Good:** Expectation-leveling to remove concerns of local environment issues during development.
* **Good:** Allows faster paths to integration tests of server-side components.
* **Bad:** Developers need to remember that smoke testing requires separate setup from integration tests.