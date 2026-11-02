// All calls use relative paths: the same build works behind the Vite dev
// proxy and behind Nginx, without knowing where the APIs really live.

async function request(path, options) {
  const response = await fetch(path, options);
  const text = await response.text();
  const body = text ? parseJson(text) : null;

  if (!response.ok) {
    const message =
      body?.error ?? body?.detail ?? body?.title ?? `İstek başarısız oldu (HTTP ${response.status}).`;
    throw new Error(message);
  }

  return body;
}

function parseJson(text) {
  try {
    return JSON.parse(text);
  } catch {
    return null;
  }
}

export function fetchProducts() {
  return request('/api/products');
}

export function fetchOrders() {
  return request('/api/orders');
}

export function createOrder({ productId, quantity }) {
  return request('/api/orders', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ productId, quantity }),
  });
}
