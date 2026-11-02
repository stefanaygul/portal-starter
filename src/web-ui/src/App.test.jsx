import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import App from './App.jsx';
import { createOrder, fetchOrders, fetchProducts } from './api.js';

jest.mock('./api.js');

const products = [{ code: 'ADP-01', name: 'Terminal adaptörü' }];

beforeEach(() => {
  jest.resetAllMocks();
  fetchProducts.mockResolvedValue(products);
  fetchOrders.mockResolvedValue([]);
});

test('loads the catalog and the order list on start', async () => {
  render(<App />);

  expect(await screen.findByRole('cell', { name: 'Terminal adaptörü' })).toBeInTheDocument();
  expect(screen.getByText('Henüz talep yok.')).toBeInTheDocument();
});

test('reloads the order list after a new order is created', async () => {
  const created = { id: 1, productId: 'ADP-01', quantity: 1, createdAt: '2026-01-15T09:30:00Z' };
  createOrder.mockResolvedValue(created);
  const user = userEvent.setup();
  render(<App />);
  await screen.findByRole('cell', { name: 'Terminal adaptörü' });
  fetchOrders.mockResolvedValue([created]);

  await user.selectOptions(screen.getByLabelText('Ekipman'), 'ADP-01');
  await user.click(screen.getByRole('button', { name: 'Talep oluştur' }));

  expect(createOrder).toHaveBeenCalledWith({ productId: 'ADP-01', quantity: 1 });
  expect(await screen.findByRole('status')).toHaveTextContent('Talep oluşturuldu.');
  expect(screen.queryByText('Henüz talep yok.')).not.toBeInTheDocument();
});

test('shows an error when the APIs cannot be reached', async () => {
  fetchProducts.mockRejectedValue(new Error('İstek başarısız oldu (HTTP 502).'));

  render(<App />);

  expect(await screen.findByRole('alert')).toHaveTextContent('HTTP 502');
});
