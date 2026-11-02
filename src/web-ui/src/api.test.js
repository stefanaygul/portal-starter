import { createOrder, fetchOrders, fetchProducts } from './api.js';

function jsonResponse(status, body) {
  return {
    ok: status >= 200 && status < 300,
    status,
    text: async () => (body === undefined ? '' : JSON.stringify(body)),
  };
}

beforeEach(() => {
  global.fetch = jest.fn();
});

test('fetchProducts reads the catalog from a relative path', async () => {
  global.fetch.mockResolvedValue(jsonResponse(200, [{ code: 'ADP-01', name: 'Terminal adaptörü' }]));

  const products = await fetchProducts();

  expect(global.fetch).toHaveBeenCalledWith('/api/products', undefined);
  expect(products).toEqual([{ code: 'ADP-01', name: 'Terminal adaptörü' }]);
});

test('fetchOrders reads the order list from a relative path', async () => {
  global.fetch.mockResolvedValue(jsonResponse(200, []));

  await expect(fetchOrders()).resolves.toEqual([]);
  expect(global.fetch).toHaveBeenCalledWith('/api/orders', undefined);
});

test('createOrder posts the order as JSON', async () => {
  global.fetch.mockResolvedValue(jsonResponse(201, { id: 1, productId: 'ADP-01', quantity: 2 }));

  await createOrder({ productId: 'ADP-01', quantity: 2 });

  const [path, options] = global.fetch.mock.calls[0];
  expect(path).toBe('/api/orders');
  expect(options.method).toBe('POST');
  expect(options.headers['Content-Type']).toBe('application/json');
  expect(JSON.parse(options.body)).toEqual({ productId: 'ADP-01', quantity: 2 });
});

test('a failed request reports the message sent by the API', async () => {
  global.fetch.mockResolvedValue(jsonResponse(400, { error: 'Talep reddedildi.' }));

  await expect(createOrder({ productId: 'ADP-01', quantity: 2 })).rejects.toThrow('Talep reddedildi.');
});

test('a failed request without a body reports the HTTP status', async () => {
  global.fetch.mockResolvedValue(jsonResponse(500));

  await expect(fetchProducts()).rejects.toThrow('HTTP 500');
});
