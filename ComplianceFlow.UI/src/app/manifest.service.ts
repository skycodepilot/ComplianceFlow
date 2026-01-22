import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

export interface ManifestState {
    correlationId: string;
    referenceNumber: string;
    currentState: string; // "Received", "Validating", "Validated", "Rejected"
    updatedAt: string;
}

@Injectable({ providedIn: 'root' })
export class ManifestService {
    private http = inject(HttpClient);
    private apiUrl = 'https://localhost:44391/api/manifests'; // Ensure this matches your API port

    // SIGNALS: The new state management standard
    public state = signal<ManifestState | null>(null);
    public status = signal<'idle' | 'loading' | 'success' | 'error'>('idle');
    public error = signal<string | null>(null);

    async submitManifest(reference: string, codes: string[]) {
        this.status.set('loading');
        this.error.set(null);
        this.state.set(null);

        try {
            // 1. Submit the Command
            const response = await firstValueFrom(
                this.http.post<{ manifestId: string }>(this.apiUrl, {
                    referenceNumber: reference,
                    htsCodes: codes
                })
            );

            // 2. Start Polling for the Result
            this.pollForStatus(response.manifestId);

        } catch (e: any) {
            this.status.set('error');
            this.error.set(e.message || 'Submission Failed');
        }
    }

    private async pollForStatus(id: string) {
        // Poll for 10 seconds (20 attempts * 500ms)
        for (let i = 0; i < 20; i++) {
            try {
                const result = await firstValueFrom(
                    this.http.get<ManifestState>(`${this.apiUrl}/${id}`)
                );

                this.state.set(result);

                // If we hit a "Terminal State", we are done.
                if (['Validated', 'Rejected'].includes(result.currentState)) {
                    this.status.set('success');
                    return;
                }
            } catch (e) {
                // Ignore 404s while the Saga is starting up
            }

            // Wait 500ms
            await new Promise(resolve => setTimeout(resolve, 500));
        }

        this.status.set('error');
        this.error.set('Validation timed out.');
    }
}