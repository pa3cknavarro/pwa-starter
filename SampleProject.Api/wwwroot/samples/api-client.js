/**
 * Shared helper for calling the Ecommerce Location API from the sample pages.
 * Handles the required X-Api-Key header, JSON parsing, and basic error display.
 */
const ApiClient = (function () {
  // Change this if your API runs on a different origin/port
  const DEFAULT_BASE = window.location.origin; // same origin when served by the API
  const API_KEY = 'demo-api-key-12345';

  /**
   * Low-level fetch wrapper that always sends the API key and parses JSON.
   * @param {string} path - relative path starting with /api/...
   * @param {RequestInit} options - fetch options (method, body, headers, …)
   * @returns {Promise<{ok:boolean, status:number, data:any, error?:object}>}
   */
  async function request(path, options = {}) {
    const url = (options.baseUrl || DEFAULT_BASE) + path;
    const headers = {
      'Accept': 'application/json',
      'X-Api-Key': API_KEY,
      ...(options.headers || {})
    };

    // Automatically set Content-Type for JSON bodies
    if (options.body && typeof options.body === 'object') {
      headers['Content-Type'] = 'application/json';
      options.body = JSON.stringify(options.body);
    }

    // Optional Idempotency-Key for write operations
    if (options.idempotencyKey) {
      headers['X-Idempotency-Key'] = options.idempotencyKey;
    }

    try {
      const res = await fetch(url, { ...options, headers });
      let data = null;
      const text = await res.text();
      if (text) {
        try { data = JSON.parse(text); } catch { data = text; }
      }
      return { ok: res.ok, status: res.status, data, headers: res.headers };
    } catch (err) {
      return {
        ok: false,
        status: 0,
        data: null,
        error: { title: 'Network Error', detail: err.message }
      };
    }
  }

  // Convenience helpers
  return {
    get: (path, query = {}) => {
      const qs = new URLSearchParams(query).toString();
      return request(path + (qs ? '?' + qs : ''), { method: 'GET' });
    },
    post: (path, body, idempotencyKey) =>
      request(path, { method: 'POST', body, idempotencyKey }),
    put: (path, body, idempotencyKey) =>
      request(path, { method: 'PUT', body, idempotencyKey }),
    delete: (path, idempotencyKey) =>
      request(path, { method: 'DELETE', idempotencyKey }),
    // Expose raw request if needed
    request
  };
})();
