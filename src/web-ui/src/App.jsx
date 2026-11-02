import { useCallback, useEffect, useState } from 'react';
import { createOrder, fetchOrders, fetchProducts } from './api.js';
import CatalogList from './components/CatalogList.jsx';
import OrderForm from './components/OrderForm.jsx';
import OrderList from './components/OrderList.jsx';

export default function App() {
  const [products, setProducts] = useState([]);
  const [orders, setOrders] = useState([]);
  const [loadError, setLoadError] = useState(null);

  const reloadOrders = useCallback(async () => {
    setOrders(await fetchOrders());
  }, []);

  useEffect(() => {
    Promise.all([fetchProducts().then(setProducts), reloadOrders()]).catch((error) =>
      setLoadError(error.message),
    );
  }, [reloadOrders]);

  async function handleCreate(order) {
    await createOrder(order);
    await reloadOrders();
  }

  return (
    <main>
      <h1>Saha Ekipman Talep Portalı</h1>
      {loadError && (
        <p role="alert" className="message error">
          Veriler yüklenemedi: {loadError}
        </p>
      )}
      <CatalogList products={products} />
      <OrderForm products={products} onSubmit={handleCreate} />
      <OrderList orders={orders} products={products} />
    </main>
  );
}
