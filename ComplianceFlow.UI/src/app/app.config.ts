import { ApplicationConfig, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
    providers: [
        // 1. The "Sarah Special" - No Zone.js overhead
        provideZonelessChangeDetection(),

        // 2. Standard Routing
        provideRouter(routes),

        // 3. Modern Fetch API for HTTP
        provideHttpClient(withFetch())
    ]
};