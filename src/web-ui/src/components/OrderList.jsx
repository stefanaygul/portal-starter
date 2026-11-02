export default function OrderList({ orders, products }) {
  const nameByCode = new Map(products.map((product) => [product.code, product.name]));

  return (
    <section aria-labelledby="order-list-title">
      <h2 id="order-list-title">Talepler</h2>
      {orders.length === 0 ? (
        <p>Henüz talep yok.</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>No</th>
              <th>Kod</th>
              <th>Ekipman</th>
              <th>Miktar</th>
              <th>Tarih</th>
            </tr>
          </thead>
          <tbody>
            {orders.map((order) => (
              <tr key={order.id}>
                <td>{order.id}</td>
                <td>{order.productId}</td>
                <td>{nameByCode.get(order.productId) ?? ''}</td>
                <td>{order.quantity}</td>
                <td>{new Date(order.createdAt).toLocaleString('tr-TR')}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  );
}
