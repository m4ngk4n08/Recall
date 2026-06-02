import { Routes } from '@angular/router';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () => import('../features/dashboard/dashboard').then(m => m.Dashboard)
    },
    {
        path: 'ingest',
        loadComponent: () => import('../features/url-ingestor/url-ingestor').then(m => m.UrlIngestor)
    },
    {
        path: 'note',
        loadComponent: () => import('../features/thought-dump/thought-dump').then(m => m.ThoughtDump)
    },
    { path: '**', redirectTo: '' }
];
