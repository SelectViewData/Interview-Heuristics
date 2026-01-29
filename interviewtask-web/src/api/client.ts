import createFetchClient from "openapi-fetch";
import createClient from "openapi-react-query";

import type { paths } from "./types";

export const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL ??
  "http://localhost:5080") as string;

export const fetchClient = createFetchClient<paths>({
  baseUrl: apiBaseUrl,
});

export const $api = createClient(fetchClient);
