import { useState } from 'react';

export default function OrderForm({ products, onSubmit }) {
  const [productId, setProductId] = useState('');
  const [quantity, setQuantity] = useState('1');
  const [status, setStatus] = useState(null);

  async function handleSubmit(event) {
    event.preventDefault();
    setStatus(null);

    try {
      await onSubmit({ productId, quantity: Number(quantity) });
      setStatus({ type: 'success', text: 'Talep oluşturuldu.' });
    } catch (error) {
      setStatus({ type: 'error', text: error.message });
    }
  }

  return (
    <section aria-labelledby="order-form-title">
      <h2 id="order-form-title">Yeni talep</h2>
      <form onSubmit={handleSubmit}>
        <label>
          Ekipman
          <select value={productId} onChange={(event) => setProductId(event.target.value)}>
            <option value="">Ekipman seçin</option>
            {products.map((product) => (
              <option key={product.code} value={product.code}>
                {product.code} - {product.name}
              </option>
            ))}
          </select>
        </label>
        <label>
          Miktar
          <input
            type="number"
            value={quantity}
            onChange={(event) => setQuantity(event.target.value)}
          />
        </label>
        <button type="submit">Talep oluştur</button>
      </form>
      {status && (
        <p role={status.type === 'error' ? 'alert' : 'status'} className={`message ${status.type}`}>
          {status.text}
        </p>
      )}
    </section>
  );
}
