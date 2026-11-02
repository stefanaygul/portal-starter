import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import OrderForm from './OrderForm.jsx';

const products = [
  { code: 'ADP-01', name: 'Terminal adaptörü' },
  { code: 'KBL-02', name: 'Bağlantı kablosu' },
];

test('submits the selected product and quantity', async () => {
  const onSubmit = jest.fn().mockResolvedValue(undefined);
  const user = userEvent.setup();
  render(<OrderForm products={products} onSubmit={onSubmit} />);

  await user.selectOptions(screen.getByLabelText('Ekipman'), 'KBL-02');
  await user.clear(screen.getByLabelText('Miktar'));
  await user.type(screen.getByLabelText('Miktar'), '3');
  await user.click(screen.getByRole('button', { name: 'Talep oluştur' }));

  expect(onSubmit).toHaveBeenCalledWith({ productId: 'KBL-02', quantity: 3 });
  expect(await screen.findByRole('status')).toHaveTextContent('Talep oluşturuldu.');
});

test('shows the error message when the request is rejected', async () => {
  const onSubmit = jest.fn().mockRejectedValue(new Error('Talep reddedildi.'));
  const user = userEvent.setup();
  render(<OrderForm products={products} onSubmit={onSubmit} />);

  await user.selectOptions(screen.getByLabelText('Ekipman'), 'ADP-01');
  await user.click(screen.getByRole('button', { name: 'Talep oluştur' }));

  expect(await screen.findByRole('alert')).toHaveTextContent('Talep reddedildi.');
});
