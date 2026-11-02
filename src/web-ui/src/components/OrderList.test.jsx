import { render, screen } from '@testing-library/react';
import OrderList from './OrderList.jsx';

const products = [{ code: 'ADP-01', name: 'Terminal adaptörü' }];

test('shows a message when there are no orders', () => {
  render(<OrderList orders={[]} products={products} />);

  expect(screen.getByText('Henüz talep yok.')).toBeInTheDocument();
});

test('lists orders with the product name from the catalog', () => {
  const orders = [{ id: 7, productId: 'ADP-01', quantity: 2, createdAt: '2026-01-15T09:30:00Z' }];

  render(<OrderList orders={orders} products={products} />);

  const row = screen.getByRole('row', { name: /ADP-01/ });
  expect(row).toHaveTextContent('7');
  expect(row).toHaveTextContent('Terminal adaptörü');
  expect(row).toHaveTextContent('2');
});
