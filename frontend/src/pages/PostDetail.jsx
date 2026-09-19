import { Link, useParams } from 'react-router-dom'
import { posts } from '../data/posts'

export default function PostDetail() {
  const { id } = useParams()
  const post = posts.find((p) => p.id === id)

  if (!post) {
    return (
      <section>
        <p>포스트를 찾을 수 없습니다.</p>
        <Link to="/">목록으로 돌아가기</Link>
      </section>
    )
  }

  return (
    <article>
      <Link to="/" className="post-detail__back">
        ← 목록으로
      </Link>
      <h1>{post.title}</h1>
      <p className="post-list__date">{post.date}</p>
      <p>{post.content}</p>
    </article>
  )
}
