import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import basicSsl from '@vitejs/plugin-basic-ssl'
import { copyFileSync } from 'fs'
import { join } from 'path'

// GitHub Pages SPA 라우팅을 위한 플러그인
const githubPagesSPA = () => {
  return {
    name: 'github-pages-spa',
    writeBundle() {
      // 빌드 후 404.html을 index.html로 복사
      const distPath = join(__dirname, 'dist')
      try {
        copyFileSync(join(__dirname, 'public', '404.html'), join(distPath, '404.html'))
      } catch (e) {
        console.warn('404.html 복사 실패:', e)
      }
    }
  }
}

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    react({
      include: "**/*.{jsx,tsx,js,ts}",
    }),
    githubPagesSPA(),
    // basicSsl() // HTTP로 변경 (Unity 연동용)
  ],
  base: '/Rhythm-game/', // GitHub Pages용 base 경로
  esbuild: {
    loader: 'tsx',
    include: /src\/.*\.[jt]sx?$/,
    exclude: []
  },
  optimizeDeps: {
    esbuildOptions: {
      loader: {
        '.js': 'jsx',
        '.ts': 'tsx',
        '.jsx': 'jsx',
        '.tsx': 'tsx'
      }
    }
  },
  server: {
    port: 5173,
    host: '0.0.0.0',
    proxy: {
      '/socket.io': {
        target: 'https://127.0.0.1:5000',
        changeOrigin: true,
        secure: false, // Accept self-signed backend cert
        ws: true
      }
    }
  }
})