import { Link } from 'react-router-dom'
import { posts } from '../data/posts'
import VoiceMemoUploader from '../components/VoiceMemoUploader'

export default function PostList() {
  return (
    <>
      <VoiceMemoUploader />
      <section>
        <h1>포스트 목록</h1>
        <ul className="post-list">
          {posts.map((post) => (
            <li key={post.id} className="post-list__item">
              <Link to={`/posts/${post.id}`}>
                <h2>{post.title}</h2>
              </Link>
              <p className="post-list__date">{post.date}</p>
              <p>{post.summary}</p>
            </li>
          ))}
        </ul>
      </section>
    </>
  )
}
