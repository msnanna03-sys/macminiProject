import { Route, Routes } from 'react-router-dom'
import Header from './components/Header'
import PostList from './pages/PostList'
import PostDetail from './pages/PostDetail'
import './App.css'

export default function App() {
  return (
    <div className="app">
      <Header />
      <main className="app__main">
        <Routes>
          <Route path="/" element={<PostList />} />
          <Route path="/posts/:id" element={<PostDetail />} />
        </Routes>
      </main>
    </div>
  )
}
