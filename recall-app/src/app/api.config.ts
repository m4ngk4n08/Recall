import { environment } from "../environments/environment"

const BASE_URL  = environment.apiUrl;

export const API_ENDPOINTS = {
    ingest: `${BASE_URL}/ingest`,
    items: `${BASE_URL}/items`,
    chat: `${BASE_URL}/chat`
}