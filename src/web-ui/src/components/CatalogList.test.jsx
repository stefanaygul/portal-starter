import { render, screen, within } from '@testing-library/react';
import CatalogList from './CatalogList.jsx';

const products = [
  { code: 'ADP-01', name: 'Terminal adaptörü', inStock: true },
  { code: 'KBL-02', name: 'Bağlantı kablosu', inStock: false },
];

test('lists every product with its code and name', () => {
  render(<CatalogList products={products} />);

  expect(screen.getByText('ADP-01')).toBeInTheDocument();
  expect(screen.getByText('Terminal adaptörü')).toBeInTheDocument();
  expect(screen.getByText('KBL-02')).toBeInTheDocument();
  expect(screen.getAllByRole('row')).toHaveLength(products.length + 1);
});

test('shows the stock status of every product', () => {
  render(<CatalogList products={products} />);

  const [, inStockRow, outOfStockRow] = screen.getAllByRole('row');
  expect(within(inStockRow).getByText('Stokta')).toBeInTheDocument();
  expect(within(outOfStockRow).getByText('Stokta yok')).toBeInTheDocument();
});

test('shows a message when the catalog is empty', () => {
  render(<CatalogList products={[]} />);

  expect(screen.getByText('Katalog boş.')).toBeInTheDocument();
});
