# ComplianceFlow

A robust, event-driven distributed system designed to validate international shipping manifests against Trade Compliance regulations.

**ComplianceFlow** demonstrates a vertical slice of a modern enterprise architecture, utilizing **Angular 21 (Signals + Zoneless)** for the frontend and **.NET 8** with **MassTransit Sagas** for backend orchestration.

![Architecture Diagram](https://mermaid.ink/img/pako:eNptkU1rwzAMhv9K0KkF9zB622Ew2MvYoaWnIYQ4tY3A9kOyo5TSf5_jPGi3nSzkffRR0t4Yc6UYY-Op9sZ4fD1Lw_H8JgPPH-J0J04f4vREO5-lP8vjR-39y_qjO_h9f5Gnd_Fj_3C4y-v1Vfph-OytP-nL8dF73w7D8-G2n6Tz1j-L07s8vi8_D4d3GeZf_Tft-2eZ7sXD86_2F-2nS_uX_uXw1s0_RylV1aBCVqAF1aBK1qANWKA1WIs2YIvWoi3YirVoG7ZhbdiOnVibdmAv1qE92If1aB_2YwPah4PYgA5iIzagQ9iIDehQNmIDOoyN2IiN2ISN2IRN2IyN2IzN2ILN2Iqt2Iqt2Iat2Iat2Ibt2I7t2I7t2IEd2IEd2IEd2Ind2Ind2I092I092I092IM92IM92IM92Is92Is92Iv92I_92I8DOIgDOIgDOIgDOISDOISDOISDOIzDOIzDOIzDOILDOILDOILDOIrDOIrDOIrDOImTOImTOImTOIWTOIWTOIWTOI3TOI3TOI3TOIPTOIPTOIPTOIPT_wdYw6oG)

## 🏗️ Architecture Highlights

* **Frontend:** Angular 21 (latest), Zoneless Change Detection, Signals, Control Flow syntax (`@if`).
* **Backend:** .NET 8 Web API, Entity Framework Core.
* **Messaging:** MassTransit State Machine Sagas (Orchestration Pattern) over RabbitMQ.
* **Infrastructure:** Dockerized SQL Server 2022 and RabbitMQ Management.
* **Reliability:** Integration Tests using **Testcontainers** to verify the full saga lifecycle.

## 🚀 Prerequisites

Before running the solution, ensure you have the following installed:

* **Docker Desktop** (Must be running)
* **.NET 8 SDK**
* **Node.js v22+** (Required for Angular 21)

## 🛠️ Getting Started

### 1. Infrastructure (Docker)
Start the supporting services (SQL Server & RabbitMQ).
```bash
docker-compose up -d
```
*Wait for the containers to reach a "Started" state.*

### 2. Backend (API)
The API will automatically create the database schema on first run.

1.  Navigate to the API folder:
    ```bash
    cd ComplianceFlow.Api
    ```
2.  **Security Note:** Open `appsettings.Development.json` and ensure the `ConnectionStrings:DefaultConnection` password matches the `MSSQL_SA_PASSWORD` in your `docker-compose.yml`.
3.  Run the application:
    ```bash
    dotnet run
    ```
    * **API Url:** `https://localhost:44391`
    * **Swagger Docs:** `https://localhost:44391/swagger`

### 3. Frontend (UI)
1.  Navigate to the UI folder:
    ```bash
    cd ComplianceFlow.UI
    ```
2.  Install dependencies:
    ```bash
    npm install
    ```
3.  Start the development server:
    ```bash
    npm start
    ```
4.  Open your browser to: **`http://localhost:4200`**

## 🧪 How to Test

### The "Happy Path" (Validation)
1.  Enter Reference: `SHIP-GOOD`
2.  Enter HTS Code: `8542.31` (default)
3.  Click **Submit**.
4.  **Result:** The UI polls the backend until the Saga completes. You will see a **Green Card** indicating "Approved for Export."

### The "Compliance Block" (Rejection)
1.  Enter Reference: `SHIP-SNEAK`
2.  Enter HTS Code: `9999.99` (Restricted Item)
3.  Click **Submit**.
4.  **Result:** The system detects the restricted code. You will see a **Red Card** indicating "Item Restricted by Global Trade Compliance."

## 📂 Project Structure

* **ComplianceFlow.Api:** The REST entry point. Publishes `SubmitManifest` commands.
* **ComplianceFlow.Contracts:** Shared message types (commands/events) with strict Namespacing.
* **ComplianceFlow.UI:** Lightweight Angular 21 dashboard using `manifest.service.ts` for polling.
* **ComplianceFlow.IntegrationTests:** Full-stack tests using Testcontainers.

---
*Built as a reference architecture for Event-Driven Compliance Systems.*