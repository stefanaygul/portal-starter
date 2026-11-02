import { render, screen } from '@testing-library/react';
import CatalogList from './CatalogList.jsx';

const products = [
  { code: 'ADP-01', name: 'Terminal adaptörü' },
  { code: 'KBL-02', name: 'Bağlantı kablosu' },
];

test('lists every product with its code and name', () => {
  render(<CatalogList products={products} />);

  expect(screen.getByText('ADP-01')).toBeInTheDocument();
  expect(screen.getByText('Terminal adaptörü')).toBeInTheDocument();
  expect(screen.getByText('KBL-02')).toBeInTheDocument();
  expect(screen.getAllByRole('row')).toHaveLength(products.length + 1);
});

test('shows a message when the catalog is empty', () => {
  render(<CatalogList products={[]} />);

  expect(screen.getByText('Katalog boş.')).toBeInTheDocument();
});
