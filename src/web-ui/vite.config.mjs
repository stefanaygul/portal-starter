import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// The UI always calls relative /api/... paths. In development the Vite dev
// server forwards them to the locally running APIs; in the container image
// Nginx does the same job (deploy/nginx.conf).
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api/products': 'http://localhost:5101',
      '/api/orders': 'http://localhost:5102',
    },
  },
});
