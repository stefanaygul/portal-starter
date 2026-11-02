export default function CatalogList({ products }) {
  return (
    <section aria-labelledby="catalog-title">
      <h2 id="catalog-title">Ekipman kataloğu</h2>
      {products.length === 0 ? (
        <p>Katalog boş.</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Kod</th>
              <th>Ekipman</th>
            </tr>
          </thead>
          <tbody>
            {products.map((product) => (
              <tr key={product.code}>
                <td>{product.code}</td>
                <td>{product.name}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  );
}
