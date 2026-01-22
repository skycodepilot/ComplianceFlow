import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ManifestService } from './manifest.service';

@Component({
    selector: 'app-root',
    standalone: true,
    imports: [FormsModule], // Needed for [(ngModel)]
    template: `
    <div class="container">
      <h1>ComplianceFlow v21</h1>
      
      <div class="card input-group">
        <input [(ngModel)]="ref" placeholder="Reference ID (e.g. SHIP-001)" />
        <input [(ngModel)]="code" placeholder="HTS Code (9999.99 = Reject)" />
        
        <button 
          (click)="svc.submitManifest(ref, [code])"
          [disabled]="svc.status() === 'loading'">
          
          @if (svc.status() === 'loading') {
            <span>⏳ Processing...</span>
          } @else {
            <span>Submit Manifest</span>
          }
        </button>
      </div>

      @if (svc.error()) {
        <div class="banner error">
          🚨 {{ svc.error() }}
        </div>
      }

      @if (svc.state(); as s) {
        <div class="card result" 
             [class.valid]="s.currentState === 'Validated'"
             [class.invalid]="s.currentState === 'Rejected'">
          
          <h2>State: {{ s.currentState }}</h2>
          <p>Reference: {{ s.referenceNumber }}</p>
          
          @if (s.currentState === 'Rejected') {
            <p class="reason">⚠️ Item Restricted by Global Trade Compliance.</p>
          }
          @if (s.currentState === 'Validated') {
             <p class="reason">✅ Approved for Export.</p>
          }
        </div>
      }
    </div>
  `,
    styles: [`
    .container { max-width: 600px; margin: 2rem auto; font-family: sans-serif; }
    .card { padding: 1.5rem; border: 1px solid #ccc; border-radius: 8px; margin-bottom: 1rem; }
    .input-group { display: flex; gap: 0.5rem; flex-direction: column; }
    input, button { padding: 0.8rem; font-size: 1rem; }
    button { background: #007bff; color: white; border: none; cursor: pointer; }
    button:disabled { background: #ccc; }
    
    .result { background: #f8f9fa; transition: all 0.3s; }
    .valid { border-left: 10px solid #28a745; background: #e6ffec; }
    .invalid { border-left: 10px solid #dc3545; background: #ffe6e6; }
    .error { background: #ffcccc; color: #cc0000; padding: 1rem; border-radius: 8px; }
  `]
})
export class App { // The CLI might call this "App" or "AppComponent"
    svc = inject(ManifestService); // Public so template can access signals
    ref = 'SHIP-2026';
    code = '8542.31';
}